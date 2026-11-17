namespace ContractManagement.Scheduling;

/// <summary>Date stepping for one stream (BR1.1 to BR1.5).</summary>
internal static class PaymentDates
{
    /// <summary>Lists the due dates of a stream, from the start date up to and including the end date.</summary>
    public static IEnumerable<DateOnly> Enumerate(StreamInput stream)
    {
        if (stream.Frequency == Frequency.OneTime)
        {
            yield return stream.StartDate;
            yield break;
        }

        for (var n = 0; ; n++)
        {
            if (!TryStep(stream, n, out var date) || date > stream.EndDate)
            {
                yield break;
            }
            yield return date;
        }
    }

    // Payment n is always counted from the original start date, so a 31st start recovers after a short month.
    private static bool TryStep(StreamInput stream, int n, out DateOnly date)
    {
        try
        {
            date = stream.Frequency switch
            {
                Frequency.Weekly => stream.StartDate.AddDays(7 * n),
                Frequency.Monthly => stream.StartDate.AddMonths(n),
                Frequency.Quarterly => stream.StartDate.AddMonths(3 * n),
                Frequency.Annually => stream.StartDate.AddYears(n),
                _ => throw new InvalidOperationException("Unknown frequency.")
            };
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Stepped past the last date a DateOnly can hold: nothing further can be due.
            date = default;
            return false;
        }
    }
}
