using ContractManagement.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddContractApi();

var app = builder.Build();
app.UseContractApi();
await app.InitializeDatabaseAsync();
app.Run();

/// <summary>Entry point marker for integration tests.</summary>
public partial class Program;
