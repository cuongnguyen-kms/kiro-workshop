namespace ContractManagement.Scheduling;

/// <summary>Stateless implementation of <see cref="IBalanceScheduleBuilder"/>.</summary>
public sealed class BalanceScheduleBuilder : IBalanceScheduleBuilder
{
    /// <inheritdoc />
    public IReadOnlyList<BalanceRow> Build(IReadOnlyList<PaymentEntry> payments, IReadOnlyList<ExpenseEntry> expenses)
    {
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(expenses);
        if (expenses.Count == 0) return [];

        var paymentTotal = payments.Sum(p => p.Amount);
        var expenseTotal = expenses.Sum(e => e.Amount);
        if (paymentTotal != expenseTotal)
            throw new ArgumentException(
                $"Payment total {paymentTotal} differs from expense total {expenseTotal}.", nameof(payments));

        var months = new SortedDictionary<DateOnly, MonthAcc>();
        foreach (var p in payments) Get(months, p.DueDate).Payments += p.Amount;
        foreach (var e in expenses)
        {
            var acc = Get(months, e.PeriodStart);
            acc.Expense += e.Amount;
            if (e.PeriodStart != e.PeriodEnd)
            {
                acc.OvertimeStart = acc.OvertimeStart is { } s && s < e.PeriodStart ? s : e.PeriodStart;
                acc.OvertimeEnd = acc.OvertimeEnd is { } t && t > e.PeriodEnd ? t : e.PeriodEnd;
            }
        }

        var keys = months.Keys.ToArray();
        var accs = months.Values.ToArray();
        var balances = new decimal[keys.Length];
        var running = 0m;
        for (var i = 0; i < keys.Length; i++)
        {
            running += accs[i].Payments - accs[i].Expense;
            balances[i] = running;
        }

        var rows = new List<BalanceRow>(keys.Length);
        for (var i = 0; i < keys.Length; i++)
        {
            var month = keys[i];
            var acc = accs[i];
            var start = acc.OvertimeStart ?? month;
            var end = acc.OvertimeEnd ?? month.AddMonths(1).AddDays(-1);

            var idx = Array.BinarySearch(keys, month.AddMonths(12));
            var floor = idx >= 0 ? idx : ~idx - 1;
            var (st, lt) = Split(balances[i], balances[floor]);
            var negative = balances[i] < 0;
            rows.Add(new BalanceRow(
                start, end, acc.Expense, acc.Payments - acc.Expense, balances[i],
                negative ? st : 0m, negative ? lt : 0m,
                negative ? 0m : st, negative ? 0m : lt));
        }
        return rows.AsReadOnly();
    }

    private static MonthAcc Get(SortedDictionary<DateOnly, MonthAcc> months, DateOnly date)
    {
        var key = new DateOnly(date.Year, date.Month, 1);
        if (!months.TryGetValue(key, out var acc)) months[key] = acc = new MonthAcc();
        return acc;
    }

    private static (decimal St, decimal Lt) Split(decimal b, decimal l)
    {
        if (b == 0m) return (0m, 0m);
        var absB = Math.Abs(b);
        var absL = Math.Abs(l);
        if (l == 0m || Math.Sign(l) != Math.Sign(b)) return (absB, 0m);
        if (absL >= absB) return (0m, absB);
        return (absB - absL, absL);
    }

    private sealed class MonthAcc
    {
        public decimal Payments;
        public decimal Expense;
        public DateOnly? OvertimeStart;
        public DateOnly? OvertimeEnd;
    }
}
