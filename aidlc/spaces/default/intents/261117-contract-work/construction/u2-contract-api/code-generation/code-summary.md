# Code Summary: u2-contract-api

## What was built

A .NET 10 minimal API for contracts on PostgreSQL. It validates a contract, calls the existing schedule library (`IScheduleGenerator`, unit u1) once, and saves the contract, its streams and both schedules in one transaction.

| Endpoint | Behavior |
|---|---|
| `POST /api/v1/contracts` | 201 with `Location` and the saved contract; 400 validation problem with messages keyed by field path; 500 generic problem with `traceId` |
| `GET /api/v1/contracts?page=&pageSize=` | page of summaries (default page 1, size 10; sizes 10, 20, 50, 100); sorted by start date descending, name, id |
| `GET /api/v1/contracts/{id}` | the contract with streams and both schedules; 404 `Contract not found.` |
| `GET /health` | 200, or 503 when `SELECT 1` does not answer within 2 seconds |
| `GET /openapi/v1.json` | generated OpenAPI document, Development only |

Source layout in `src/ContractManagement.Api/`: `Contracts/` (request and response shapes), `Domain/` (four entities), `Validation/` (`ContractValidator`, `SizeEstimator`), `Services/ContractService`, `Persistence/` (DbContext, repository, `InitialCreate` migration, `DatabaseSettings`), `Endpoints/`, `Hosting/ApiHosting.cs`, `Program.cs`.

Also added: tool manifest `.config/dotnet-tools.json` (`dotnet-ef` 10.0.4, local), the two test projects, and the three new projects in `ContractManagement.slnx`. `scripts/verify.ps1` is unchanged: its solution-wide `dotnet build` and `dotnet test` steps pick up the new projects.

## How to run

```powershell
dotnet user-secrets set "ConnectionStrings:ContractDbPassword" "<your local password>" --project src/ContractManagement.Api
dotnet run --project src/ContractManagement.Api
```

In Development the API creates the `contract_management` database if it is missing and applies the migration at start. Outside Development it only checks the connection.

## Configuration keys

| Key | Where | Default |
|---|---|---|
| `ConnectionStrings:ContractDbHost` | appsettings.json | `localhost` |
| `ConnectionStrings:ContractDbPort` | appsettings.json | `5432` |
| `ConnectionStrings:ContractDbDatabase` | appsettings.json | `contract_management` |
| `ConnectionStrings:ContractDbUser` | appsettings.json | `postgres` |
| `ConnectionStrings:ContractDbPassword` | user secrets, or environment variable `ConnectionStrings__ContractDbPassword` | none; never in a file |
| `Cors:AllowedOrigin` | appsettings.json (empty), appsettings.Development.json (`http://localhost:5173`) | empty allows no origin |

## Setting the password

- API: `dotnet user-secrets set "ConnectionStrings:ContractDbPassword" "<value>" --project src/ContractManagement.Api`
- Integration tests: the same command with `--project tests/ContractManagement.Api.IntegrationTests`
- Or set the environment variable `ConnectionStrings__ContractDbPassword` for either.

When the password is missing, the API and the integration tests fail with a message that names the setting.

## Test results

Commands (the unit-test instructions list two projects in one `dotnet build`, which the .NET CLI refuses, so build each project on its own):

```powershell
dotnet build tests/ContractManagement.Api.Tests -warnaserror
dotnet build tests/ContractManagement.Api.IntegrationTests -warnaserror
dotnet test tests/ContractManagement.Api.Tests --no-build
dotnet test tests/ContractManagement.Api.IntegrationTests --no-build
```

| Project | Result |
|---|---|
| `ContractManagement.Api.Tests` (unit) | 44 passed, 0 failed |
| `ContractManagement.Api.IntegrationTests` (local PostgreSQL, `contract_management_test`) | 19 passed, 0 failed |
| `ContractManagement.Scheduling.Tests` (existing) | 19 passed, 0 failed |

Solution-wide `dotnet build -warnaserror` gives 0 warnings and 0 errors. The frontend steps of `scripts/verify.ps1` were not run (they belong to u3 and install packages in `web/`).

## Notes and gaps

- `Microsoft.OpenApi` 2.13.1 is referenced directly because the version pulled in by `Microsoft.AspNetCore.OpenApi` 10.0.4 (2.0.0) has a known high severity advisory, which fails the build under warnings as errors.
- The 256 KB body limit is set on Kestrel; the in-memory test server has no such limit, so it has no automated test. The generic 500 path has no automated test either.
- The 2-second create test measures one request after a warm-up request.
- `aidlc engine sensor-traceability` was refused by a permission rule in this session, so `traceability.json` was written by hand in the same shape as the earlier stages.
- Integration tests share one database and run one after another.
