# Unit Test Instructions: u1-schedule-balance

## Framework

xUnit in the existing project `tests/ContractManagement.Scheduling.Tests` (references `src/ContractManagement.Scheduling`). No new packages and no configuration change. The tests need no database, network or environment variables.

Before running, stop the API if it is running (it locks the build output): `Get-Process -Name ContractManagement.Api` and stop it.

## Run this Unit's tests

Builder tests and the generator's balance tests only:

```powershell
dotnet test tests/ContractManagement.Scheduling.Tests --filter "FullyQualifiedName~BalanceScheduleBuilderTests|FullyQualifiedName~ScheduleGeneratorTests"
```

The fake in `tests/ContractManagement.Api.Tests/Fakes.cs` changes too, so check that project still compiles and passes its own fake-based tests:

```powershell
dotnet build tests/ContractManagement.Api.Tests -warnaserror
```

## Expected coverage targets

No coverage floor (team.md). Every rule BR1.1 to BR3.2 and every NFR row NFR1.1, NFR2.1, NFR3.1 to NFR3.3 has at least one test. Build and Test runs the full suite.

## Mocking and stubbing

None. The builder is a pure function, so tests call it directly with hand-built `PaymentEntry` and `ExpenseEntry` lists. Generator tests use the real `ScheduleGenerator`.

## Test data

Inline in each test, taken from the worked examples E1 to E10 in `construction/u1-schedule-balance/functional-design/functional-spec.md`. The timing test builds 10 monthly streams over 10 years through the real generator.
