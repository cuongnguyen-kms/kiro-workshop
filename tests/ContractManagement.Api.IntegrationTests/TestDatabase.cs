using ContractManagement.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>Shared helpers for the local PostgreSQL test database <c>contract_management_test</c>.</summary>
internal static class TestDatabase
{
    public const string Name = "contract_management_test";

    /// <summary>Settings for the test run: user secrets, then environment variables. The password is never in a file.</summary>
    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddUserSecrets(typeof(TestDatabase).Assembly)
        .AddEnvironmentVariables()
        .Build();

    /// <summary>Connection string for the test database. Throws a message that names the password setting when it is missing.</summary>
    public static string ConnectionString => DatabaseSettings.BuildConnectionString(Configuration, Name);

    public static DbContextOptions<ContractDbContext> Options =>
        new DbContextOptionsBuilder<ContractDbContext>().UseNpgsql(ConnectionString).Options;

    public static ContractDbContext CreateContext() => new(Options);

    /// <summary>Creates the database and tables when missing, then empties the tables.</summary>
    public static async Task ResetAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE expense_schedule_entries, payment_schedule_entries, payment_streams, contracts CASCADE");
    }
}

/// <summary>All database tests share one database, so they run one after another.</summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection
{
    public const string Name = "database";
}
