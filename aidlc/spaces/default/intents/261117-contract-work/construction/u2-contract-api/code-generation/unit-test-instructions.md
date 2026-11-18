# Unit Test Instructions: u2-contract-api

## Framework

xUnit 2.9.3 with `Microsoft.NET.Test.Sdk` 17.14.1 and `xunit.runner.visualstudio` 3.1.4, the same versions as `ContractManagement.Scheduling.Tests`. The integration project adds `Microsoft.AspNetCore.Mvc.Testing`.

| Project | What it holds | Needs |
|---|---|---|
| `tests/ContractManagement.Api.Tests` | validation, size estimate, service (fake generator and fake repository) | nothing else |
| `tests/ContractManagement.Api.IntegrationTests` | repository, data model, endpoints | local PostgreSQL and the password |

## Commands that run only this Unit's tests

Build first, then run (both are exact and scoped to this Unit's projects):

```powershell
dotnet build tests/ContractManagement.Api.Tests tests/ContractManagement.Api.IntegrationTests -warnaserror
dotnet test tests/ContractManagement.Api.Tests --no-build
dotnet test tests/ContractManagement.Api.IntegrationTests --no-build
```

## Database for the integration tests

- Host `localhost`, port `5432`, user `postgres`, database `contract_management_test`. The tests create it if missing and empty its tables before each test class.
- The password comes from the user secret `ConnectionStrings:ContractDbPassword` (set once with `dotnet user-secrets set` on the integration project) or from the environment variable `ConnectionStrings__ContractDbPassword`. It is never written to a file. When it is missing, the tests fail with a message that names this setting; they are not skipped.

## Expected coverage

No project-wide coverage floor applies to the `classic` scope. Minimal strategy: at least one test per requirement and one happy-path test per component (about 25 tests in total). Every validation message in the Functional Design rules has a test.

## Mocking and stubbing

- `IScheduleGenerator` and `IContractRepository` are replaced by hand-written fakes in the unit tests. No mocking library is used.
- The integration tests use the real generator, the real repository and the real database.
- A failing repository for the atomic-save test is a fake that throws on the last write, plus an integration test that forces the last write to fail in the real database.

## Test data

- Dates are fixed (for example 2027-01-01 to 2027-12-31). No test reads the clock.
- Each integration test class empties the four tables before it runs, so tests do not depend on each other.
