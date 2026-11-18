using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContractManagement.Api.Persistence;

/// <summary>Lets <c>dotnet ef migrations add</c> build the model without a database or a password.</summary>
internal sealed class DesignTimeFactory : IDesignTimeDbContextFactory<ContractDbContext>
{
    public ContractDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ContractDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_only;Username=design")
            .Options;
        return new ContractDbContext(options);
    }
}
