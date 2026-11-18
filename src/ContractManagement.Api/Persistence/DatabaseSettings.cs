using Npgsql;

namespace ContractManagement.Api.Persistence;

/// <summary>Builds the PostgreSQL connection string from configuration (BR6.2, NFR2.1). The password only comes from configuration.</summary>
public static class DatabaseSettings
{
    /// <summary>Configuration key of the password (user secrets or environment variable ConnectionStrings__ContractDbPassword).</summary>
    public const string PasswordKey = "ConnectionStrings:ContractDbPassword";

    /// <summary>Builds the connection string. Throws a message that names the missing password key and never prints a value.</summary>
    public static string BuildConnectionString(IConfiguration configuration, string? databaseOverride = null)
    {
        var section = configuration.GetSection("ConnectionStrings");
        var password = configuration[PasswordKey];
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                $"The database password is not set. Provide it with user secrets or the environment variable ConnectionStrings__ContractDbPassword (key {PasswordKey}).");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = section["ContractDbHost"] ?? "localhost",
            Port = int.TryParse(section["ContractDbPort"], out var port) ? port : 5432,
            Database = databaseOverride ?? section["ContractDbDatabase"] ?? "contract_management",
            Username = section["ContractDbUser"] ?? "postgres",
            Password = password,
            MaxPoolSize = 20,
        };
        return builder.ConnectionString;
    }

    /// <summary>Host and database for log lines. Never includes the password.</summary>
    public static string Describe(string connectionString)
    {
        var b = new NpgsqlConnectionStringBuilder(connectionString);
        return $"{b.Host}:{b.Port}/{b.Database}";
    }
}
