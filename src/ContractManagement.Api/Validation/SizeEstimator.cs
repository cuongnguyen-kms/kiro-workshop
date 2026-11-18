using ContractManagement.Scheduling;

namespace ContractManagement.Api.Validation;

/// <summary>Estimates how many schedule rows a validated contract would produce, rounding up so it never undercounts (BR5.4).</summary>
public static class SizeEstimator
{
    /// <summary>The most schedule rows one contract may generate.</summary>
    public const int MaxRows = 10_000;

    /// <summary>The message returned when a contract is too large.</summary>
    public const string TooLarge = "This contract would create more than 10,000 schedule rows. Shorten the dates or use a less frequent payment.";

    /// <summary>Estimated payment rows of one stream.</summary>
    public static long EstimateStream(StreamInput stream)
    {
        var days = (long)(stream.EndDate.DayNumber - stream.StartDate.DayNumber) + 1;
        var months = MonthSpan(stream.StartDate, stream.EndDate);
        return stream.Frequency switch
        {
            Frequency.Weekly => CeilDiv(days, 7),
            Frequency.Monthly => months,
            Frequency.Quarterly => CeilDiv(months, 3),
            Frequency.Annually => CeilDiv(months, 12),
            _ => 1,
        };
    }

    /// <summary>Estimated expense rows.</summary>
    public static long EstimateExpense(ExpenseInput expense) => expense.Method switch
    {
        ExpenseMethod.Overtime => MonthSpan(expense.StartDate!.Value, expense.EndDate!.Value),
        ExpenseMethod.AllAtOnce => 1,
        _ => 0,
    };

    /// <summary>Estimated total rows of a request.</summary>
    public static long Estimate(ScheduleRequest request) =>
        request.Streams.Sum(EstimateStream) + EstimateExpense(request.Expense);

    private static long MonthSpan(DateOnly start, DateOnly end) =>
        ((long)end.Year - start.Year) * 12 + (end.Month - start.Month) + 1;

    private static long CeilDiv(long value, long divisor) => (value + divisor - 1) / divisor;
}
