namespace ContractManagement.Scheduling;

/// <summary>Builds the expense rows (BR2.1 to BR2.6).</summary>
internal static class ExpenseScheduleBuilder
{
    public static (List<ExpenseEntry> Expenses, decimal Total) Build(ExpenseInput expense, decimal baseAmount)
    {
        switch (expense.Method)
        {
            case ExpenseMethod.None:
                return (new List<ExpenseEntry>(), 0m);

            case ExpenseMethod.AllAtOnce:
                var date = expense.Date!.Value;
                return (new List<ExpenseEntry> { new(date, date, baseAmount) }, baseAmount);

            case ExpenseMethod.Overtime:
                return (Overtime(expense.StartDate!.Value, expense.EndDate!.Value, baseAmount), baseAmount);

            default:
                throw new ArgumentException("Unknown expense method.", nameof(expense));
        }
    }

    private static List<ExpenseEntry> Overtime(DateOnly start, DateOnly end, decimal baseAmount)
    {
        var totalDays = end.DayNumber - start.DayNumber + 1;
        var rows = new List<ExpenseEntry>();
        var allocated = 0m;
        var periodStart = start;

        while (periodStart <= end)
        {
            var lastOfMonth = new DateOnly(periodStart.Year, periodStart.Month,
                DateTime.DaysInMonth(periodStart.Year, periodStart.Month));
            var periodEnd = lastOfMonth < end ? lastOfMonth : end;
            var isLast = periodEnd == end;

            decimal amount;
            if (isLast)
            {
                amount = baseAmount - allocated;
            }
            else
            {
                var monthDays = periodEnd.DayNumber - periodStart.DayNumber + 1;
                amount = Math.Round(baseAmount * monthDays / totalDays, 2, MidpointRounding.AwayFromZero);
                allocated += amount;
            }
            rows.Add(new ExpenseEntry(periodStart, periodEnd, amount));

            if (isLast)
            {
                break;
            }
            periodStart = periodEnd.AddDays(1);
        }
        return rows;
    }
}
