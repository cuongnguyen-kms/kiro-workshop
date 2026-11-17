namespace ContractManagement.Scheduling;

/// <summary>Checks the caller's promises (BR1.8) and the size limit (NFR6.2).</summary>
internal static class InputChecker
{
    public const int MaxEntries = 100_000;

    public static void Check(ScheduleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Streams is null || request.Streams.Count == 0)
        {
            throw new ArgumentException("At least one payment stream is required.", nameof(request));
        }
        if (request.Expense is null)
        {
            throw new ArgumentException("Expense settings are required.", nameof(request));
        }

        for (var i = 0; i < request.Streams.Count; i++)
        {
            var s = request.Streams[i];
            if (s is null)
            {
                throw new ArgumentException($"Stream {i} is missing.", nameof(request));
            }
            if (s.Amount <= 0m)
            {
                throw new ArgumentException($"Stream {i}: amount must be greater than zero.", nameof(request));
            }
            if (!Enum.IsDefined(s.Frequency))
            {
                throw new ArgumentException($"Stream {i}: unknown frequency.", nameof(request));
            }
            if (s.EndDate < s.StartDate)
            {
                throw new ArgumentException($"Stream {i}: end date is before start date.", nameof(request));
            }
            if (s.Frequency == Frequency.OneTime && s.EndDate != s.StartDate)
            {
                throw new ArgumentException($"Stream {i}: a OneTime stream must have end date equal to start date.", nameof(request));
            }
        }

        var e = request.Expense;
        switch (e.Method)
        {
            case ExpenseMethod.Overtime:
                if (e.StartDate is null || e.EndDate is null)
                {
                    throw new ArgumentException("Overtime expense needs a start date and an end date.", nameof(request));
                }
                if (e.EndDate < e.StartDate)
                {
                    throw new ArgumentException("Overtime expense end date is before its start date.", nameof(request));
                }
                break;
            case ExpenseMethod.AllAtOnce:
                if (e.Date is null)
                {
                    throw new ArgumentException("AllAtOnce expense needs a date.", nameof(request));
                }
                break;
            case ExpenseMethod.None:
                break;
            default:
                throw new ArgumentException("Unknown expense method.", nameof(request));
        }

        var count = 0L;
        foreach (var s in request.Streams)
        {
            foreach (var _ in PaymentDates.Enumerate(s))
            {
                if (++count > MaxEntries)
                {
                    throw new ArgumentException($"The request would produce more than {MaxEntries} entries.", nameof(request));
                }
            }
        }
        if (e.Method == ExpenseMethod.Overtime)
        {
            var s = e.StartDate!.Value;
            var en = e.EndDate!.Value;
            count += (en.Year - s.Year) * 12L + en.Month - s.Month + 1;
            if (count > MaxEntries)
            {
                throw new ArgumentException($"The request would produce more than {MaxEntries} entries.", nameof(request));
            }
        }
    }
}
