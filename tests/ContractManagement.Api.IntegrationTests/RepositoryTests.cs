using ContractManagement.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>Repository layer against the real database (AC6.1.2, AC6.2.1, AC6.2.2, AC6.3.1, AC6.3.2).</summary>
[Collection(DatabaseCollection.Name)]
public sealed class RepositoryTests : IAsyncLifetime
{
    private readonly ContractRepository _repository = new(new FixedFactory());

    public Task InitializeAsync() => TestDatabase.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Save_then_get_returns_every_part()
    {
        var contract = ContractFactory.Create();
        await _repository.SaveAsync(contract, CancellationToken.None);

        var loaded = await _repository.GetAsync(contract.ContractId, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal("Office lease", loaded.Name);
        Assert.Equal(contract.StartDate, loaded.StartDate);
        Assert.Equal(contract.ExpenseDate, loaded.ExpenseDate);
        Assert.Single(loaded.PaymentStreams);
        Assert.Equal(2, loaded.PaymentEntries.Count);
        Assert.Equal(contract.StartDate, loaded.PaymentEntries[0].DueDate);
        Assert.Single(loaded.ExpenseEntries);
        Assert.Equal(2469.00m, loaded.ExpenseEntries[0].Amount);
    }

    [Fact]
    public async Task A_failure_on_a_write_leaves_no_rows()
    {
        var contract = ContractFactory.Create();
        // Too large for numeric(18,2): the database rejects this row after the others were written in the transaction.
        contract.ExpenseEntries[0].Amount = 99999999999999999999m;

        await Assert.ThrowsAnyAsync<Exception>(() => _repository.SaveAsync(contract, CancellationToken.None));

        await using var db = TestDatabase.CreateContext();
        Assert.Equal(0, await db.Contracts.CountAsync());
        Assert.Equal(0, await db.PaymentStreams.CountAsync());
        Assert.Equal(0, await db.PaymentScheduleEntries.CountAsync());
        Assert.Equal(0, await db.ExpenseScheduleEntries.CountAsync());
    }

    [Fact]
    public async Task List_orders_by_start_date_descending_then_name_and_pages()
    {
        await _repository.SaveAsync(ContractFactory.Create("B", new DateOnly(2027, 1, 1)), CancellationToken.None);
        await _repository.SaveAsync(ContractFactory.Create("A", new DateOnly(2027, 1, 1)), CancellationToken.None);
        await _repository.SaveAsync(ContractFactory.Create("Newest", new DateOnly(2028, 1, 1)), CancellationToken.None);

        var first = await _repository.ListAsync(1, 2, CancellationToken.None);
        var second = await _repository.ListAsync(2, 2, CancellationToken.None);

        Assert.Equal(3, first.TotalCount);
        Assert.Equal(["Newest", "A"], first.Items.Select(i => i.Name));
        Assert.Equal(["B"], second.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task A_page_past_the_last_is_empty_and_an_empty_database_gives_an_empty_list()
    {
        var empty = await _repository.ListAsync(1, 10, CancellationToken.None);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.TotalCount);

        await _repository.SaveAsync(ContractFactory.Create(), CancellationToken.None);
        var past = await _repository.ListAsync(5, 10, CancellationToken.None);
        Assert.Empty(past.Items);
        Assert.Equal(1, past.TotalCount);
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        Assert.Null(await _repository.GetAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
