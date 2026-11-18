using System.Diagnostics;
using ContractManagement.Scheduling;

namespace ContractManagement.Scheduling.Tests;

public class BalanceScheduleBuilderTests
{
    private static readonly BalanceScheduleBuilder Builder = new();
    private static readonly Guid Stream = Guid.NewGuid();

    private static DateOnly D(int y, int m, int d) => new(y, m, d);

    private static PaymentEntry P(DateOnly due, decimal amount) => new(Stream, due, amount);

    private static ExpenseEntry Day(DateOnly date, decimal amount) => new(date, date, amount);

    /// <summary>One Overtime row per calendar month, from the start month, count months, as the generator makes them.</summary>
    private static List<ExpenseEntry> Monthly(int year, int month, int count, decimal amount)
    {
        var rows = new List<ExpenseEntry>();
        var first = new DateOnly(year, month, 1);
        for (var i = 0; i < count; i++)
        {
            var s = first.AddMonths(i);
            rows.Add(new ExpenseEntry(s, s.AddMonths(1).AddDays(-1), amount));
        }
        return rows;
    }

    private static IReadOnlyList<BalanceRow> Build(IReadOnlyList<PaymentEntry> p, IReadOnlyList<ExpenseEntry> e)
    {
        var rows = Builder.Build(p, e);
        AssertInvariants(rows);
        return rows;
    }

    private static void AssertInvariants(IReadOnlyList<BalanceRow> rows)
    {
        if (rows.Count == 0) return;
        Assert.Equal(0m, rows[^1].CumulativeBalance);
        Assert.All(rows, r =>
        {
            Assert.True(r.StAccruedLiability >= 0 && r.LtAccruedLiability >= 0
                && r.StPrepaidAsset >= 0 && r.LtPrepaidAsset >= 0);
            Assert.Equal(Math.Abs(r.CumulativeBalance),
                r.StAccruedLiability + r.LtAccruedLiability + r.StPrepaidAsset + r.LtPrepaidAsset);
            if (r.CumulativeBalance < 0) Assert.Equal(0m, r.StPrepaidAsset + r.LtPrepaidAsset);
            if (r.CumulativeBalance > 0) Assert.Equal(0m, r.StAccruedLiability + r.LtAccruedLiability);
        });
    }

    // E1 (AC2.1.1): prepaid, ends within 12 months, all short term
    [Fact]
    public void E1_Prepaid_EndsWithin12Months_AllShortTerm()
    {
        var rows = Build([P(D(2027, 1, 15), 12000m)], Monthly(2027, 1, 12, 1000m));
        Assert.Equal(12, rows.Count);
        var jan = rows[0];
        Assert.Equal(D(2027, 1, 1), jan.PeriodStart);
        Assert.Equal(D(2027, 1, 31), jan.PeriodEnd);
        Assert.Equal(1000m, jan.Expense);
        Assert.Equal(11000m, jan.AccrualPrepaid);
        Assert.Equal(11000m, jan.CumulativeBalance);
        Assert.Equal(11000m, jan.StPrepaidAsset);
        Assert.Equal(0m, jan.LtPrepaidAsset);
        Assert.Equal(-1000m, rows[1].AccrualPrepaid);
        Assert.Equal(10000m, rows[1].CumulativeBalance);
        Assert.Equal(10000m, rows[1].StPrepaidAsset);
        Assert.Equal(0m, rows[11].CumulativeBalance);
        Assert.Equal(0m, rows[11].StPrepaidAsset);
    }

    // E2 (AC2.2.1): prepaid with a long-term part
    [Fact]
    public void E2_Prepaid_WithLongTermPart()
    {
        var rows = Build([P(D(2027, 1, 15), 24000m)], Monthly(2027, 1, 48, 500m));
        Assert.Equal(23500m, rows[0].CumulativeBalance);
        Assert.Equal(17500m, rows[12].CumulativeBalance);
        Assert.Equal(6000m, rows[0].StPrepaidAsset);
        Assert.Equal(17500m, rows[0].LtPrepaidAsset);
    }

    // E3 (AC2.1.2): accrual, months without rows are skipped
    [Fact]
    public void E3_Accrual_AllAtOnceThenMonthlyPayments()
    {
        var payments = Enumerable.Range(0, 9).Select(i => P(D(2027, 4, 1).AddMonths(i), 1000m)).ToList();
        var rows = Build(payments, [Day(D(2027, 1, 31), 9000m)]);
        Assert.Equal(10, rows.Count);
        var jan = rows[0];
        Assert.Equal(9000m, jan.Expense);
        Assert.Equal(-9000m, jan.AccrualPrepaid);
        Assert.Equal(-9000m, jan.CumulativeBalance);
        Assert.Equal(9000m, jan.StAccruedLiability);
        Assert.Equal(0m, jan.LtAccruedLiability);
        Assert.Equal(D(2027, 4, 1), rows[1].PeriodStart);
        Assert.Equal(0m, rows[1].Expense);
        Assert.Equal(1000m, rows[1].AccrualPrepaid);
        Assert.Equal(-8000m, rows[1].CumulativeBalance);
        Assert.Equal(0m, rows[9].CumulativeBalance);
    }

    // E4: partial accrual reversal, -9000 now and -3000 twelve months later
    [Fact]
    public void E4_PartialAccrualReversal_SplitsStAndLt()
    {
        var rows = Build(
            [P(D(2027, 6, 15), 6000m), P(D(2028, 2, 15), 3000m)],
            [Day(D(2027, 1, 15), 9000m)]);
        Assert.Equal(-9000m, rows[0].CumulativeBalance);
        Assert.Equal(-3000m, rows[1].CumulativeBalance);
        Assert.Equal(6000m, rows[0].StAccruedLiability);
        Assert.Equal(3000m, rows[0].LtAccruedLiability);
    }

    // E5 (AC2.2.3): sign change, later balance has the opposite sign
    [Fact]
    public void E5_SignChange_AllShortTerm()
    {
        var rows = Build(
            [P(D(2027, 1, 10), 3000m), P(D(2028, 3, 10), 1000m)],
            [Day(D(2027, 1, 10), 1000m), Day(D(2027, 6, 10), 3000m)]);
        Assert.Equal(2000m, rows[0].CumulativeBalance);
        Assert.Equal(-1000m, rows[1].CumulativeBalance);
        Assert.Equal(2000m, rows[0].StPrepaidAsset);
        Assert.Equal(0m, rows[0].LtPrepaidAsset);
    }

    // E6: later balance of the same sign and larger size
    [Fact]
    public void E6_LargerSameSignLaterBalance_AllLongTerm()
    {
        var rows = Build(
            [P(D(2027, 1, 10), 3000m), P(D(2027, 6, 10), 3000m)],
            [Day(D(2027, 1, 10), 1000m), Day(D(2028, 3, 10), 5000m)]);
        Assert.Equal(2000m, rows[0].CumulativeBalance);
        Assert.Equal(5000m, rows[1].CumulativeBalance);
        Assert.Equal(0m, rows[0].StPrepaidAsset);
        Assert.Equal(2000m, rows[0].LtPrepaidAsset);
    }

    // E7: the look-ahead month has no row, so the latest earlier row is used
    [Fact]
    public void E7_LookAheadMonthWithoutRow_UsesLatestRowOnOrBefore()
    {
        var rows = Build(
            [P(D(2027, 3, 10), 1200m)],
            [Day(D(2027, 3, 20), 200m), Day(D(2028, 1, 20), 400m), Day(D(2028, 9, 20), 600m)]);
        Assert.Equal(1000m, rows[0].CumulativeBalance);
        Assert.Equal(600m, rows[1].CumulativeBalance);
        Assert.Equal(400m, rows[0].StPrepaidAsset);
        Assert.Equal(600m, rows[0].LtPrepaidAsset);
    }

    // E8: leap year, February 2027 looks at the February 2028 row ending on the 29th
    [Fact]
    public void E8_LeapYear_FebruaryLooksAtFebruaryNextYear()
    {
        var expenses = new List<ExpenseEntry>
        {
            new(D(2027, 2, 1), D(2027, 2, 28), 1000m),
            new(D(2028, 2, 1), D(2028, 2, 29), 1000m),
            new(D(2028, 3, 1), D(2028, 3, 31), 1000m),
        };
        var rows = Build([P(D(2027, 2, 10), 3000m)], expenses);
        Assert.Equal(D(2028, 2, 29), rows[1].PeriodEnd);
        Assert.Equal(2000m, rows[0].CumulativeBalance);
        Assert.Equal(1000m, rows[1].CumulativeBalance);
        Assert.Equal(1000m, rows[0].StPrepaidAsset);
        Assert.Equal(1000m, rows[0].LtPrepaidAsset);
    }

    // E9 (AC1.1.1): payment-only month before an Overtime range
    [Fact]
    public void E9_PaymentOnlyMonthBeforeOvertimeRange()
    {
        var rows = Build([P(D(2027, 1, 15), 3000m)], Monthly(2027, 3, 3, 1000m));
        Assert.Equal(4, rows.Count);
        Assert.Equal(D(2027, 1, 1), rows[0].PeriodStart);
        Assert.Equal(D(2027, 1, 31), rows[0].PeriodEnd);
        Assert.Equal(0m, rows[0].Expense);
        Assert.Equal(3000m, rows[0].AccrualPrepaid);
        Assert.Equal(new[] { 3, 4, 5 }, rows.Skip(1).Select(r => r.PeriodStart.Month).ToArray());
        Assert.Equal(D(2027, 5, 31), rows[3].PeriodEnd);
    }

    // E10 (AC1.1.4): method None
    [Fact]
    public void E10_NoExpenseRows_ReturnsNoRows()
    {
        Assert.Empty(Build([P(D(2027, 1, 15), 100m)], []));
    }

    // BR2.4: B is 0 in the middle of a schedule
    [Fact]
    public void Split_BalanceZero_AllColumnsZero()
    {
        var rows = Build(
            [P(D(2027, 1, 5), 1000m), P(D(2027, 3, 5), 500m)],
            [Day(D(2027, 1, 20), 1000m), Day(D(2027, 3, 20), 500m)]);
        Assert.All(rows, r =>
        {
            Assert.Equal(0m, r.CumulativeBalance);
            Assert.Equal(0m, r.StAccruedLiability + r.LtAccruedLiability + r.StPrepaidAsset + r.LtPrepaidAsset);
        });
    }

    // BR2.4: L is 0
    [Fact]
    public void Split_LookAheadZero_AllShortTerm()
    {
        var rows = Build([P(D(2027, 1, 5), 1000m)], [Day(D(2027, 3, 5), 1000m)]);
        Assert.Equal(1000m, rows[0].CumulativeBalance);
        Assert.Equal(1000m, rows[0].StPrepaidAsset);
        Assert.Equal(0m, rows[0].LtPrepaidAsset);
    }

    // BR2.4: size of L equals size of B
    [Fact]
    public void Split_LookAheadSameSize_AllLongTerm()
    {
        var rows = Build(
            [P(D(2027, 1, 5), 2000m), P(D(2027, 6, 5), 1000m)],
            [Day(D(2027, 6, 20), 1000m), Day(D(2028, 6, 20), 2000m)]);
        Assert.Equal(2000m, rows[0].CumulativeBalance);
        Assert.Equal(2000m, rows[1].CumulativeBalance);
        Assert.Equal(0m, rows[0].StPrepaidAsset);
        Assert.Equal(2000m, rows[0].LtPrepaidAsset);
    }

    // BR2.5: a negative balance fills only the accrued columns
    [Fact]
    public void Sign_Negative_FillsOnlyAccruedColumns()
    {
        var rows = Build([P(D(2027, 3, 5), 500m)], [Day(D(2027, 1, 5), 500m)]);
        Assert.Equal(-500m, rows[0].CumulativeBalance);
        Assert.Equal(500m, rows[0].StAccruedLiability);
        Assert.Equal(0m, rows[0].StPrepaidAsset);
        Assert.Equal(0m, rows[0].LtPrepaidAsset);
    }

    // BR2.5: a positive balance fills only the prepaid columns
    [Fact]
    public void Sign_Positive_FillsOnlyPrepaidColumns()
    {
        var rows = Build([P(D(2027, 1, 5), 500m)], [Day(D(2027, 3, 5), 500m)]);
        Assert.Equal(500m, rows[0].CumulativeBalance);
        Assert.Equal(500m, rows[0].StPrepaidAsset);
        Assert.Equal(0m, rows[0].StAccruedLiability);
        Assert.Equal(0m, rows[0].LtAccruedLiability);
    }

    // SEC1
    [Fact]
    public void NullPayments_Throws() =>
        Assert.Throws<ArgumentNullException>(() => Builder.Build(null!, []));

    // SEC1
    [Fact]
    public void NullExpenses_Throws() =>
        Assert.Throws<ArgumentNullException>(() => Builder.Build([], null!));

    // BR3.2
    [Fact]
    public void UnequalTotals_ThrowsWithBothTotals()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Builder.Build([P(D(2027, 1, 5), 100m)], [Day(D(2027, 1, 5), 90m)]));
        Assert.Contains("100", ex.Message);
        Assert.Contains("90", ex.Message);
    }

    // BR1.1, BR3.2: empty expenses return empty even with payments
    [Fact]
    public void EmptyExpenses_WithPayments_ReturnsEmptyWithoutThrowing()
    {
        Assert.Empty(Builder.Build([P(D(2027, 1, 5), 100m), P(D(2027, 2, 5), 50m)], []));
    }

    // NFR3.3
    [Fact]
    public void SameInputTwice_GivesEqualRows()
    {
        var payments = new List<PaymentEntry> { P(D(2027, 1, 15), 24000m) };
        var expenses = Monthly(2027, 1, 48, 500m);
        Assert.Equal(Builder.Build(payments, expenses), Builder.Build(payments, expenses));
    }

    // NFR1.1: 10 streams over 10 years, best of 3 under 50 ms after a warm-up
    [Fact]
    public void LargestInput_BuildsUnder50Ms()
    {
        var streams = Enumerable.Range(0, 10)
            .Select(_ => new StreamInput(Guid.NewGuid(), 1000m, Frequency.Monthly, D(2027, 1, 15), D(2036, 12, 15)))
            .ToList();
        var result = new ScheduleGenerator().Generate(new ScheduleRequest(
            streams, new ExpenseInput(ExpenseMethod.Overtime, D(2027, 1, 1), D(2036, 12, 31), null)));
        Builder.Build(result.Payments, result.Expenses);
        var best = TimeSpan.MaxValue;
        for (var i = 0; i < 3; i++)
        {
            var sw = Stopwatch.StartNew();
            Builder.Build(result.Payments, result.Expenses);
            sw.Stop();
            if (sw.Elapsed < best) best = sw.Elapsed;
        }
        Assert.True(best < TimeSpan.FromMilliseconds(50), $"best run took {best.TotalMilliseconds} ms");
    }

    // AC1.1.2: payments and expenses in the same months give one row per month
    [Fact]
    public void AC112_PaymentsAndExpenseSameMonths_OneRowPerMonth()
    {
        var payments = Enumerable.Range(0, 3).Select(i => P(D(2027, 1, 15).AddMonths(i), 1000m)).ToList();
        var rows = Build(payments, Monthly(2027, 1, 3, 1000m));
        Assert.Equal(new[] { 1, 2, 3 }, rows.Select(r => r.PeriodStart.Month).ToArray());
        Assert.All(rows, r => Assert.Equal(1000m, r.Expense));
        Assert.All(rows, r => Assert.Equal(0m, r.CumulativeBalance));
    }

    // AC1.1.3: All at once in June with quarterly payments gives rows for Jan, Apr, Jun, Jul, Oct
    [Fact]
    public void AC113_AllAtOnceWithQuarterlyPayments_RowsAndSingleExpenseMonth()
    {
        var payments = new[] { 1, 4, 7, 10 }.Select(m => P(D(2027, m, 1), 1000m)).ToList();
        var rows = Build(payments, [Day(D(2027, 6, 30), 4000m)]);
        Assert.Equal(new[] { 1, 4, 6, 7, 10 }, rows.Select(r => r.PeriodStart.Month).ToArray());
        Assert.Equal(new[] { 0m, 0m, 4000m, 0m, 0m }, rows.Select(r => r.Expense).ToArray());
        Assert.Equal(D(2027, 6, 1), rows[2].PeriodStart);
        Assert.Equal(D(2027, 6, 30), rows[2].PeriodEnd);
    }
}
