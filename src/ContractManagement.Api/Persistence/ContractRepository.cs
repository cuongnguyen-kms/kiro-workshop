using ContractManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Api.Persistence;

/// <summary>EF Core and PostgreSQL implementation of <see cref="IContractRepository"/>.</summary>
public sealed class ContractRepository(IDbContextFactory<ContractDbContext> contextFactory) : IContractRepository
{
    /// <inheritdoc />
    public async Task SaveAsync(Contract contract, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Contract?> GetAsync(Guid contractId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var contract = await db.Contracts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.PaymentStreams)
            .Include(c => c.PaymentEntries)
            .Include(c => c.ExpenseEntries)
            .SingleOrDefaultAsync(c => c.ContractId == contractId, cancellationToken);
        if (contract is null)
        {
            return null;
        }

        var position = contract.PaymentStreams.ToDictionary(s => s.StreamId, s => s.Position);
        contract.PaymentStreams = contract.PaymentStreams.OrderBy(s => s.Position).ToList();
        contract.PaymentEntries = contract.PaymentEntries
            .OrderBy(e => e.DueDate)
            .ThenBy(e => position[e.StreamId])
            .ToList();
        contract.ExpenseEntries = contract.ExpenseEntries.OrderBy(e => e.PeriodStart).ThenBy(e => e.PeriodEnd).ToList();
        return contract;
    }

    /// <inheritdoc />
    public async Task<ContractListResult> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var totalCount = await db.Contracts.CountAsync(cancellationToken);
        var items = await db.Contracts
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ThenBy(c => c.Name)
            .ThenBy(c => c.ContractId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ContractSummaryRow(c.ContractId, c.Name, c.StartDate, c.EndDate))
            .ToListAsync(cancellationToken);
        return new ContractListResult(items, totalCount);
    }
}
