using ContractManagement.Api.Domain;

namespace ContractManagement.Api.Persistence;

/// <summary>One row of the contract list: summary columns only.</summary>
public sealed record ContractSummaryRow(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate);

/// <summary>One page of the contract list and the total number of contracts.</summary>
public sealed record ContractListResult(IReadOnlyList<ContractSummaryRow> Items, int TotalCount);

/// <summary>Stores and reads the Contract aggregate.</summary>
public interface IContractRepository
{
    /// <summary>Saves the contract, its streams and both schedules in one transaction, or nothing (BR5.1).</summary>
    Task SaveAsync(Contract contract, CancellationToken cancellationToken);

    /// <summary>Reads a contract with every part in stored order, or null when no contract has the id (BR5.2).</summary>
    Task<Contract?> GetAsync(Guid contractId, CancellationToken cancellationToken);

    /// <summary>Reads one page ordered by start date descending, name, id (BR5.3). <paramref name="page"/> starts at 1.</summary>
    Task<ContractListResult> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
}
