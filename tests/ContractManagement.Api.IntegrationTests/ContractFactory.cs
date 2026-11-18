using ContractManagement.Api.Domain;
using ContractManagement.Scheduling;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>Builds domain contracts with fixed dates for repository tests.</summary>
internal static class ContractFactory
{
    public static Contract Create(string name = "Office lease", DateOnly? start = null, decimal amount = 1234.50m)
    {
        var contractId = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var startDate = start ?? new DateOnly(2027, 1, 1);
        return new Contract
        {
            ContractId = contractId,
            Name = name,
            StartDate = startDate,
            EndDate = startDate.AddMonths(12).AddDays(-1),
            ExpenseMethod = ExpenseMethod.AllAtOnce,
            ExpenseDate = startDate,
            CreatedAt = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero),
            PaymentStreams =
            [
                new PaymentStream
                {
                    StreamId = streamId, ContractId = contractId, Position = 0, Amount = amount,
                    Frequency = Frequency.Monthly, StartDate = startDate, EndDate = startDate.AddMonths(2),
                },
            ],
            PaymentEntries =
            [
                new PaymentScheduleEntry { PaymentEntryId = Guid.NewGuid(), ContractId = contractId, StreamId = streamId, DueDate = startDate, Amount = amount },
                new PaymentScheduleEntry { PaymentEntryId = Guid.NewGuid(), ContractId = contractId, StreamId = streamId, DueDate = startDate.AddMonths(1), Amount = amount },
            ],
            ExpenseEntries =
            [
                new ExpenseScheduleEntry { ExpenseEntryId = Guid.NewGuid(), ContractId = contractId, PeriodStart = startDate, PeriodEnd = startDate, Amount = amount * 2 },
            ],
        };
    }
}
