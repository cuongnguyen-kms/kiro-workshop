using ContractManagement.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>Data model layer: tables, money precision, cascade delete (NFR4.3).</summary>
[Collection(DatabaseCollection.Name)]
public sealed class DataModelTests : IAsyncLifetime
{
    public Task InitializeAsync() => TestDatabase.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migration_creates_the_four_tables()
    {
        await using var db = TestDatabase.CreateContext();
        var tables = await db.Database
            .SqlQuery<string>($"SELECT table_name AS \"Value\" FROM information_schema.tables WHERE table_schema = 'public' AND table_name IN ('contracts','payment_streams','payment_schedule_entries','expense_schedule_entries')")
            .ToListAsync();
        Assert.Equal(4, tables.Count);
    }

    [Fact]
    public async Task Money_columns_are_numeric_18_2_and_keep_amounts_exactly()
    {
        var contract = ContractFactory.Create(amount: 1234.50m);
        await new ContractRepository(new FixedFactory()).SaveAsync(contract, CancellationToken.None);

        await using var db = TestDatabase.CreateContext();
        var scale = await db.Database
            .SqlQuery<int>($"SELECT numeric_scale AS \"Value\" FROM information_schema.columns WHERE table_name = 'payment_streams' AND column_name = 'amount'")
            .SingleAsync();
        var precision = await db.Database
            .SqlQuery<int>($"SELECT numeric_precision AS \"Value\" FROM information_schema.columns WHERE table_name = 'payment_streams' AND column_name = 'amount'")
            .SingleAsync();
        var stored = await db.PaymentStreams.AsNoTracking().Select(s => s.Amount).SingleAsync();

        Assert.Equal(18, precision);
        Assert.Equal(2, scale);
        Assert.Equal(1234.50m, stored);
    }

    [Fact]
    public async Task Deleting_a_contract_deletes_its_children()
    {
        var contract = ContractFactory.Create();
        await new ContractRepository(new FixedFactory()).SaveAsync(contract, CancellationToken.None);

        await using var db = TestDatabase.CreateContext();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM contracts");

        Assert.Equal(0, await db.PaymentStreams.CountAsync());
        Assert.Equal(0, await db.PaymentScheduleEntries.CountAsync());
        Assert.Equal(0, await db.ExpenseScheduleEntries.CountAsync());
    }
}

/// <summary>Gives the repository a fresh context on the test database.</summary>
internal sealed class FixedFactory : IDbContextFactory<ContractDbContext>
{
    public ContractDbContext CreateDbContext() => TestDatabase.CreateContext();
}
