using ContractManagement.Api.Contracts;
using ContractManagement.Api.Domain;
using ContractManagement.Api.Persistence;
using ContractManagement.Scheduling;

namespace ContractManagement.Api.Tests;

/// <summary>Hand-written fake of the schedule engine: one payment per stream on its start date and consistent expense rows.</summary>
internal sealed class FakeGenerator : IScheduleGenerator
{
    public int Calls { get; private set; }
    public ScheduleRequest? LastRequest { get; private set; }
    public bool Throw { get; set; }

    public ScheduleResult Generate(ScheduleRequest request)
    {
        Calls++;
        LastRequest = request;
        if (Throw) throw new InvalidOperationException("engine failed");
        var payments = request.Streams.Select(s => new PaymentEntry(s.StreamId, s.StartDate, s.Amount)).ToList();
        var total = payments.Sum(p => p.Amount);
        var expenses = request.Expense.Method switch
        {
            ExpenseMethod.Overtime => new List<ExpenseEntry> { new(request.Expense.StartDate!.Value, request.Expense.EndDate!.Value, total) },
            ExpenseMethod.AllAtOnce => [new ExpenseEntry(request.Expense.Date!.Value, request.Expense.Date.Value, total)],
            _ => [],
        };
        return new ScheduleResult(payments, total, expenses, expenses.Sum(e => e.Amount),
            new BalanceScheduleBuilder().Build(payments, expenses));
    }
}

/// <summary>Hand-written fake repository that keeps saved contracts in memory.</summary>
internal sealed class FakeRepository : IContractRepository
{
    public List<Contract> Saved { get; } = [];
    public bool Throw { get; set; }

    public Task SaveAsync(Contract contract, CancellationToken cancellationToken)
    {
        if (Throw) throw new InvalidOperationException("database failed");
        Saved.Add(contract);
        return Task.CompletedTask;
    }

    public Task<Contract?> GetAsync(Guid contractId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(c => c.ContractId == contractId));

    public Task<ContractListResult> ListAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        Task.FromResult(new ContractListResult([], Saved.Count));
}

/// <summary>Builds valid requests with fixed dates (2027) for the tests.</summary>
internal static class Requests
{
    public static readonly DateOnly Start = new(2027, 1, 1);
    public static readonly DateOnly End = new(2027, 12, 31);

    public static CreateContractRequest Valid(
        string name = "Office lease",
        List<PaymentStreamRequest?>? streams = null,
        ExpenseRequest? expense = null) =>
        new(name, Start, End,
            streams ?? [new PaymentStreamRequest(100.00m, "Monthly", Start, new DateOnly(2027, 3, 31))],
            expense ?? new ExpenseRequest("AllAtOnce", null, null, new DateOnly(2027, 3, 31)));
}
