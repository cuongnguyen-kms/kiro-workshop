# Code Summary: u1-schedule-engine

## What was built

A .NET 10 class library, `ContractManagement.Scheduling`, that implements contract C2: `IScheduleGenerator.Generate(ScheduleRequest)` returns the payment schedule and expense schedule.

- `Models.cs`: enums, immutable records and the interface from C2, with XML docs.
- `PaymentDates.cs`: date stepping per frequency, always counted from the original start date (BR1.1 to BR1.5).
- `InputChecker.cs`: BR1.8 checks and the 100,000-entry limit (NFR6.1, NFR6.2). Throws `ArgumentException`.
- `PaymentScheduleBuilder.cs`: combines and orders the streams (BR1.6).
- `ExpenseScheduleBuilder.cs`: AllAtOnce, Overtime (per day, per calendar month, rounding on the last row) and None (BR2.1 to BR2.6).
- `ScheduleGenerator.cs`: stateless entry point.

Built with: .NET 10 SDK 10.0.404, C# nullable on, warnings as errors, `decimal` money and `DateOnly` dates. The only package is the build-time analyzer `Microsoft.CodeAnalysis.BannedApiAnalyzers` (not shipped); `BannedSymbols.txt` bans file, network, environment, console, clock and random APIs.

## How to run

- Unit tests: `dotnet test tests/ContractManagement.Scheduling.Tests/ContractManagement.Scheduling.Tests.csproj`
- Whole solution: `scripts/verify.ps1` (restore, build with `-warnaserror`, test).

## Test results

- Unit command: 19 tests, 19 passed, 0 failed, no warnings.
- `scripts/verify.ps1`: build succeeded, 19 of 19 passed, exit code 0.
- Performance (NFR5.1): 10 Weekly streams over 10 years plus a 10-year Overtime expense passed the 200 ms limit (the whole test took about 17 ms).

## Notes

- Tests are in one file, `ScheduleGeneratorTests.cs`, and map to the table in `unit-test-instructions.md`; the "Bad input" and "Overtime inside one month" cases cover several rules each.
- `.gitignore` gained `bin/` and `obj/`.
- `traceability.json` was not produced: the `aidlc engine sensor-traceability` command was blocked by a permission rule in this session.
