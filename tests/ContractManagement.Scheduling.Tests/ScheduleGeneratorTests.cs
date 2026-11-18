using System.Diagnostics;
using ContractManagement.Scheduling;

namespace ContractManagement.Scheduling.Tests;

public class ScheduleGeneratorTests
{
    private static readonly ScheduleGenerator Generator = new();
    private static readonly ExpenseInput NoExpense = new(ExpenseMethod.None, null, null, null);

    private static DateOnly D(int y, int m, int d) => new(y, m, d);

    private static StreamInput Stream(Frequency f, DateOnly start, DateOnly end, decimal amount = 1000m, Guid? id = null)
        => new(id ?? Guid.NewGuid(), amount, f, start, end);

    private static ScheduleResult Run(ExpenseInput expense, params StreamInput[] streams)
        => Generator.Generate(new ScheduleRequest(streams, expense));

    private static DateOnly[] Dates(ScheduleResult r) => r.Payments.Select(p => p.DueDate).ToArray();

    // BR1.5 (smoke test for the runner, Step 3)
    [Fact]
    public void OneTime_PaysOnceOnStartDate_EndDateIgnoredByRule()
    {
        var s = Stream(Frequency.OneTime, D(2027, 6, 1), D(2027, 6, 1), 500m);
        var r = Run(NoExpense, s);
        var p = Assert.Single(r.Payments);
        Assert.Equal(D(2027, 6, 1), p.DueDate);
        Assert.Equal(500m, p.Amount);
        Assert.Equal(500m, r.PaymentTotal);
    }

    // AC4.1.1, BR1.1, BR1.4
    [Fact]
    public void Monthly_MidMonth_FourPayments_LinkedToStream()
    {
        var s = Stream(Frequency.Monthly, D(2027, 1, 15), D(2027, 4, 15));
        var r = Run(NoExpense, s);
        Assert.Equal(new[] { D(2027, 1, 15), D(2027, 2, 15), D(2027, 3, 15), D(2027, 4, 15) }, Dates(r));
        Assert.All(r.Payments, p => { Assert.Equal(1000m, p.Amount); Assert.Equal(s.StreamId, p.StreamId); });
        Assert.Equal(4000m, r.PaymentTotal);
    }

    // AC4.1.2, BR1.3
    [Fact]
    public void Monthly_From31st_CutBackThenRecovers()
    {
        var r = Run(NoExpense, Stream(Frequency.Monthly, D(2027, 1, 31), D(2027, 4, 30)));
        Assert.Equal(new[] { D(2027, 1, 31), D(2027, 2, 28), D(2027, 3, 31), D(2027, 4, 30) }, Dates(r));
        Assert.Equal(4000m, r.PaymentTotal);
    }

    // BR1.3
    [Fact]
    public void Monthly_From31st_LeapYear_FebruaryIs29th()
    {
        var r = Run(NoExpense, Stream(Frequency.Monthly, D(2028, 1, 31), D(2028, 3, 31)));
        Assert.Equal(new[] { D(2028, 1, 31), D(2028, 2, 29), D(2028, 3, 31) }, Dates(r));
    }

    // BR1.3
    [Fact]
    public void Annually_From29February_UsesLastDayInNonLeapYears()
    {
        var r = Run(NoExpense, Stream(Frequency.Annually, D(2028, 2, 29), D(2032, 2, 29)));
        Assert.Equal(new[] { D(2028, 2, 29), D(2029, 2, 28), D(2030, 2, 28), D(2031, 2, 28), D(2032, 2, 29) }, Dates(r));
    }

    // BR1.3
    [Fact]
    public void Quarterly_StepsOfThreeMonths()
    {
        var r = Run(NoExpense, Stream(Frequency.Quarterly, D(2027, 1, 10), D(2027, 12, 31)));
        Assert.Equal(new[] { D(2027, 1, 10), D(2027, 4, 10), D(2027, 7, 10), D(2027, 10, 10) }, Dates(r));
    }

    // AC4.1.3, BR1.5
    [Fact]
    public void OneTime_Valid_ProducesSinglePayment()
    {
        var r = Run(NoExpense, Stream(Frequency.OneTime, D(2027, 6, 1), D(2027, 6, 1), 500m));
        Assert.Single(r.Payments);
        Assert.Equal(D(2027, 6, 1), r.Payments[0].DueDate);
    }

    // AC4.1.4, BR1.2
    [Fact]
    public void Weekly_AcrossYearEnd()
    {
        var r = Run(NoExpense, Stream(Frequency.Weekly, D(2027, 12, 20), D(2028, 1, 10), 100m));
        Assert.Equal(new[] { D(2027, 12, 20), D(2027, 12, 27), D(2028, 1, 3), D(2028, 1, 10) }, Dates(r));
    }

    // BR1.4
    [Fact]
    public void EndDate_IsInclusive_AndLaterDateExcluded()
    {
        var onEnd = Run(NoExpense, Stream(Frequency.Weekly, D(2027, 1, 1), D(2027, 1, 15)));
        Assert.Equal(3, onEnd.Payments.Count);
        var dayBefore = Run(NoExpense, Stream(Frequency.Weekly, D(2027, 1, 1), D(2027, 1, 14)));
        Assert.Equal(2, dayBefore.Payments.Count);
        var sameDay = Run(NoExpense, Stream(Frequency.Monthly, D(2027, 1, 1), D(2027, 1, 1)));
        Assert.Single(sameDay.Payments);
    }

    // AC4.1.5, BR1.6
    [Fact]
    public void TwoStreams_OrderedByDateThenStreamPosition()
    {
        var a = Stream(Frequency.Monthly, D(2027, 1, 1), D(2027, 2, 1), 100m);
        var b = Stream(Frequency.Weekly, D(2027, 1, 1), D(2027, 1, 8), 10m);
        var r = Run(NoExpense, a, b);
        Assert.Equal(4, r.Payments.Count);
        Assert.Equal((a.StreamId, D(2027, 1, 1)), (r.Payments[0].StreamId, r.Payments[0].DueDate));
        Assert.Equal((b.StreamId, D(2027, 1, 1)), (r.Payments[1].StreamId, r.Payments[1].DueDate));
        Assert.Equal((b.StreamId, D(2027, 1, 8)), (r.Payments[2].StreamId, r.Payments[2].DueDate));
        Assert.Equal((a.StreamId, D(2027, 2, 1)), (r.Payments[3].StreamId, r.Payments[3].DueDate));
        Assert.Equal(220m, r.PaymentTotal);
    }

    // AC4.1.6, BR1.7
    [Fact]
    public void SameInputTwice_SameResult()
    {
        var request = new ScheduleRequest(
            new[] { Stream(Frequency.Monthly, D(2027, 1, 31), D(2027, 12, 31)) },
            new ExpenseInput(ExpenseMethod.Overtime, D(2027, 1, 1), D(2027, 3, 31), null));
        var r1 = Generator.Generate(request);
        var r2 = Generator.Generate(request);
        Assert.Equal(r1.Payments, r2.Payments);
        Assert.Equal(r1.Expenses, r2.Expenses);
        Assert.Equal(r1.PaymentTotal, r2.PaymentTotal);
        Assert.Equal(r1.ExpenseTotal, r2.ExpenseTotal);
    }

    // AC5.1.1, BR2.1, BR2.2
    [Fact]
    public void AllAtOnce_OneRowForPaymentTotal()
    {
        var r = Run(new ExpenseInput(ExpenseMethod.AllAtOnce, null, null, D(2027, 3, 1)),
            Stream(Frequency.Monthly, D(2027, 1, 1), D(2027, 12, 1)));
        var e = Assert.Single(r.Expenses);
        Assert.Equal(new ExpenseEntry(D(2027, 3, 1), D(2027, 3, 1), 12000m), e);
        Assert.Equal(12000m, r.ExpenseTotal);
    }

    // AC5.1.2, AC5.1.3, BR2.3, BR2.4, BR2.6
    [Fact]
    public void Overtime_ThreeMonths_RoundsAndLastRowTakesDifference()
    {
        var r = Run(new ExpenseInput(ExpenseMethod.Overtime, D(2027, 1, 1), D(2027, 3, 31), null),
            Stream(Frequency.OneTime, D(2027, 1, 1), D(2027, 1, 1), 3100m));
        Assert.Equal(new[]
        {
            new ExpenseEntry(D(2027, 1, 1), D(2027, 1, 31), 1067.78m),
            new ExpenseEntry(D(2027, 2, 1), D(2027, 2, 28), 964.44m),
            new ExpenseEntry(D(2027, 3, 1), D(2027, 3, 31), 1067.78m)
        }, r.Expenses);
        Assert.Equal(3100.00m, r.Expenses.Sum(x => x.Amount));
        Assert.Equal(3100m, r.ExpenseTotal);
    }

    // AC5.1.4, BR2.3
    [Fact]
    public void Overtime_StartingMidMonth_ShortFirstRow()
    {
        var r = Run(new ExpenseInput(ExpenseMethod.Overtime, D(2027, 1, 20), D(2027, 2, 28), null),
            Stream(Frequency.OneTime, D(2027, 1, 1), D(2027, 1, 1), 900m));
        Assert.Equal(new[]
        {
            new ExpenseEntry(D(2027, 1, 20), D(2027, 1, 31), 270.00m),
            new ExpenseEntry(D(2027, 2, 1), D(2027, 2, 28), 630.00m)
        }, r.Expenses);
    }

    // BR2.4
    [Fact]
    public void Overtime_InsideOneMonth_OneRowWholeAmount()
    {
        var r = Run(new ExpenseInput(ExpenseMethod.Overtime, D(2027, 5, 10), D(2027, 5, 10), null),
            Stream(Frequency.OneTime, D(2027, 1, 1), D(2027, 1, 1), 777.77m));
        Assert.Equal(new[] { new ExpenseEntry(D(2027, 5, 10), D(2027, 5, 10), 777.77m) }, r.Expenses);
    }

    // AC5.1.5, BR2.5
    [Fact]
    public void None_NoExpenseRows_PaymentsStillProduced()
    {
        var r = Run(NoExpense, Stream(Frequency.Monthly, D(2027, 1, 1), D(2027, 3, 1)));
        Assert.Empty(r.Expenses);
        Assert.Equal(0m, r.ExpenseTotal);
        Assert.Equal(3, r.Payments.Count);
        Assert.Equal(3000m, r.PaymentTotal);
    }

    // BR1.8, NFR6.1
    [Fact]
    public void BadInput_ThrowsArgumentException()
    {
        var ok = Stream(Frequency.Monthly, D(2027, 1, 1), D(2027, 3, 1));
        var cases = new ScheduleRequest[]
        {
            new(Array.Empty<StreamInput>(), NoExpense),
            new(new[] { ok with { Amount = 0m } }, NoExpense),
            new(new[] { ok with { Amount = -5m } }, NoExpense),
            new(new[] { ok with { EndDate = D(2026, 12, 31) } }, NoExpense),
            new(new[] { Stream(Frequency.OneTime, D(2027, 1, 1), D(2027, 2, 1)) }, NoExpense),
            new(new[] { ok }, new ExpenseInput(ExpenseMethod.Overtime, null, null, null)),
            new(new[] { ok }, new ExpenseInput(ExpenseMethod.Overtime, D(2027, 3, 1), D(2027, 1, 1), null)),
            new(new[] { ok }, new ExpenseInput(ExpenseMethod.AllAtOnce, null, null, null)),
        };
        foreach (var c in cases)
        {
            Assert.ThrowsAny<ArgumentException>(() => Generator.Generate(c));
        }
    }

    // NFR6.2
    [Fact]
    public void TooManyEntries_Throws()
    {
        var s = Stream(Frequency.Weekly, D(2000, 1, 1), D(9999, 12, 31));
        Assert.ThrowsAny<ArgumentException>(() => Run(NoExpense, s));
    }

    // NFR5.1
    [Fact]
    public void Performance_TenWeeklyStreamsTenYears_Under200ms()
    {
        var streams = Enumerable.Range(0, 10)
            .Select(_ => Stream(Frequency.Weekly, D(2027, 1, 1), D(2036, 12, 31)))
            .ToArray();
        var request = new ScheduleRequest(streams,
            new ExpenseInput(ExpenseMethod.Overtime, D(2027, 1, 1), D(2036, 12, 31), null));
        Generator.Generate(request); // warm-up: JIT cost is not the engine's cost
        var sw = Stopwatch.StartNew();
        var r = Generator.Generate(request);
        sw.Stop();
        Assert.True(r.Payments.Count > 5000);
        Assert.True(sw.ElapsedMilliseconds < 200, $"Took {sw.ElapsedMilliseconds} ms");
    }

    // BR3.1: Generate returns the balance rows the builder makes from its own rows
    [Fact]
    public void Generate_Overtime_BalanceEqualsBuilderOutput()
    {
        var r = Run(new ExpenseInput(ExpenseMethod.Overtime, D(2027, 1, 1), D(2027, 12, 31), null),
            Stream(Frequency.OneTime, D(2027, 1, 15), D(2027, 1, 15), 12000m));
        Assert.Equal(12, r.Balance.Count);
        Assert.Equal(new BalanceScheduleBuilder().Build(r.Payments, r.Expenses), r.Balance);
    }

    // FR1.4: method None gives no balance rows
    [Fact]
    public void Generate_MethodNone_BalanceIsEmpty()
    {
        var r = Run(NoExpense, Stream(Frequency.OneTime, D(2027, 1, 15), D(2027, 1, 15), 500m));
        Assert.Empty(r.Balance);
    }
}
