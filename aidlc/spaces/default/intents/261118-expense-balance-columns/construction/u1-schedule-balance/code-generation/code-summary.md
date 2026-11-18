# Code Summary: u1-schedule-balance

## What was built

- `BalanceRow` record, `IBalanceScheduleBuilder` interface and a fifth `ScheduleResult.Balance` field (`Models.cs`, contract C2).
- `BalanceScheduleBuilder` (new file): pure, `decimal` only. Null lists throw `ArgumentNullException`; empty expenses return an empty list with no totals check; unequal totals throw `ArgumentException` naming both totals. Months come from a sorted map of payment due months and expense period-start months. The look-ahead target is the month start plus 12 calendar months; a binary search finds the latest row on or before it (BR2.3). ST/LT split per BR2.4, columns by sign per BR2.5.
- `ScheduleGenerator.Generate` now fills `Balance` through a static builder instance (BR3.1); the constructor is unchanged.

## Files changed

- Created: `src/ContractManagement.Scheduling/BalanceScheduleBuilder.cs`, `tests/ContractManagement.Scheduling.Tests/BalanceScheduleBuilderTests.cs`
- Modified: `src/ContractManagement.Scheduling/Models.cs`, `src/ContractManagement.Scheduling/ScheduleGenerator.cs`, `tests/ContractManagement.Scheduling.Tests/ScheduleGeneratorTests.cs`, `tests/ContractManagement.Api.Tests/Fakes.cs`, `tests/ContractManagement.Api.Tests/ContractServiceTests.cs`
- Deleted: none

## Tests and results

- 23 new builder tests (E1 to E10, AC1.1.2 and AC1.1.3 month rows, BR2.4 branches, BR2.5 signs, null and mismatch checks, empty expenses, determinism, 50 ms timing) and 2 new generator tests. The AC1.1.2 and AC1.1.3 tests were added after the first build when traceability showed them uncovered.
- `dotnet build ContractManagement.slnx -warnaserror`: succeeded, 0 warnings, 0 errors.
- `dotnet test tests/ContractManagement.Scheduling.Tests --filter "FullyQualifiedName~BalanceScheduleBuilderTests|FullyQualifiedName~ScheduleGeneratorTests"`: 44 passed, 0 failed.
- `dotnet build tests/ContractManagement.Api.Tests -warnaserror`: succeeded.
- `dotnet test tests/ContractManagement.Api.Tests`: 50 passed, 0 failed.
- Step 1 baseline: `dotnet test tests/ContractManagement.Scheduling.Tests`: 19 passed before changes.

## Deviations

- The plan said only `Fakes.cs` constructs `ScheduleResult`. `ContractServiceTests.cs` (the `BrokenTotalsGenerator` test double) also does, so it received an empty `Balance` argument (`[]`) to compile. Its test is about broken totals and is otherwise unchanged.
- E4, E5, E6, E7 and E8 give partial figures; each test uses a small input built to produce the stated balances. No worked example disagreed with rules.md.
- Contract C2 prose "period end plus 12 months" is superseded by BR2.3 (calendar months), as instructed.
- `traceability.json` and `source-manifest.json` are not written here (conductor writes them).
