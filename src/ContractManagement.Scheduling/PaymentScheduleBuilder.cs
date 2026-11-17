namespace ContractManagement.Scheduling;

/// <summary>Builds the combined, ordered payment list (BR1.1 to BR1.7).</summary>
internal static class PaymentScheduleBuilder
{
    public static (List<PaymentEntry> Payments, decimal Total) Build(IReadOnlyList<StreamInput> streams)
    {
        var rows = new List<(PaymentEntry Entry, int Position)>();
        for (var i = 0; i < streams.Count; i++)
        {
            var s = streams[i];
            foreach (var date in PaymentDates.Enumerate(s))
            {
                rows.Add((new PaymentEntry(s.StreamId, date, s.Amount), i));
            }
        }

        // Dates within one stream never repeat, so (date, position) is a total order.
        rows.Sort((a, b) =>
        {
            var byDate = a.Entry.DueDate.CompareTo(b.Entry.DueDate);
            return byDate != 0 ? byDate : a.Position.CompareTo(b.Position);
        });

        var payments = new List<PaymentEntry>(rows.Count);
        var total = 0m;
        foreach (var (entry, _) in rows)
        {
            payments.Add(entry);
            total += entry.Amount;
        }
        return (payments, total);
    }
}
