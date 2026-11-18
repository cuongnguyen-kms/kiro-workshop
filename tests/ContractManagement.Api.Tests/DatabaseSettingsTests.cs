using ContractManagement.Api.Persistence;
using Microsoft.Extensions.Configuration;

namespace ContractManagement.Api.Tests;

/// <summary>The connection string comes from configuration and never prints the password (BR6.2).</summary>
public sealed class DatabaseSettingsTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value)).Build();

    [Fact]
    public void A_missing_password_fails_with_a_message_that_names_the_setting()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => DatabaseSettings.BuildConnectionString(Config()));

        Assert.Contains("ConnectionStrings__ContractDbPassword", ex.Message);
    }

    [Fact]
    public void The_connection_uses_the_configured_parts_and_the_description_hides_the_password()
    {
        var connection = DatabaseSettings.BuildConnectionString(Config(
            ("ConnectionStrings:ContractDbHost", "db.example"),
            ("ConnectionStrings:ContractDbPort", "5433"),
            ("ConnectionStrings:ContractDbDatabase", "demo"),
            ("ConnectionStrings:ContractDbUser", "app"),
            (DatabaseSettings.PasswordKey, "unit-test-value")));

        Assert.Equal("db.example:5433/demo", DatabaseSettings.Describe(connection));
        Assert.DoesNotContain("unit-test-value", DatabaseSettings.Describe(connection));
    }
}
