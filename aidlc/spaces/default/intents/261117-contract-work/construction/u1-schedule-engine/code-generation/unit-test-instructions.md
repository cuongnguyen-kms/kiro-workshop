# Unit Test Instructions: u1-schedule-engine

## Setup

- Framework: xUnit, in `tests/ContractManagement.Scheduling.Tests/`, referencing `src/ContractManagement.Scheduling/`.
- Prerequisite: the .NET 10 SDK. Nothing else: the library has no database, network or configuration.
- Mocking: none. The library has no dependencies to replace. Tests build `ScheduleRequest` records and compare the returned `ScheduleResult` with expected values.
- Test data: values are written in the tests themselves, taken from the worked examples in `functional-design/functional-spec.md`. No files or fixtures.

## Command (this unit only)

```
dotnet test tests/ContractManagement.Scheduling.Tests/ContractManagement.Scheduling.Tests.csproj
```

This command runs before the first real test (Step 3 of the plan) with one smoke test, to prove the runner works.

## Tests (Minimal strategy: at least one test per acceptance criterion and per rule, about 18)

| Test | Checks | Traces to |
|---|---|---|
| Monthly stream mid-month, four payments | Dates 2027-01-15, 02-15, 03-15, 04-15, each 1000, linked to the stream | AC4.1.1, BR1.1, BR1.4 |
| Monthly from the 31st | 2027-01-31, 02-28, 03-31, 04-30 | AC4.1.2, BR1.3 |
| Monthly from the 31st in a leap year | February payment on the 29th | BR1.3 |
| Annually from 29 February | 28 February in non-leap years, 29 in leap years | BR1.3 |
| Quarterly | Steps of 3 months | BR1.3 |
| OneTime | One payment on the start date, end date ignored | AC4.1.3, BR1.5 |
| Weekly across a year end | 2027-12-20, 12-27, 2028-01-03, 01-10 | AC4.1.4, BR1.2 |
| End date is a payment date, and a date after it is excluded | Boundary inclusive | BR1.4 |
| Two streams | Entries of both, each with its own stream id, ordered by due date then stream position | AC4.1.5, BR1.6 |
| Same input twice | Identical results | AC4.1.6, BR1.7 |
| AllAtOnce | One row on the date for the payment total (12000) | AC5.1.1, BR2.1, BR2.2 |
| Overtime three months, 3100 | 1067.78, 964.44, 1067.78; sum exactly 3100.00 | AC5.1.2, AC5.1.3, BR2.3, BR2.4, BR2.6 |
| Overtime starting mid-month, 900 | January 270.00, February 630.00 | AC5.1.4, BR2.3 |
| Overtime inside one month | One row with the whole amount | BR2.4 |
| None | No rows, expense total 0, payments still produced | AC5.1.5, BR2.5 |
| Bad input | Each broken promise throws `ArgumentException` (no streams, zero amount, end before start, Overtime without dates, AllAtOnce without a date) | BR1.8, NFR6.1 |
| Too many entries | A request that would produce over 100,000 entries throws | NFR6.2 |
| Performance | 10 Weekly streams over 10 years plus a 10-year Overtime expense finishes in under 200 ms | NFR5.1 |

## Expected Coverage

- Every acceptance criterion of US4.1 and US5.1 and every rule BR1.1 to BR2.6 has at least one test.
- No project-wide line-coverage floor applies to the classic plan. If one is added later, Build and Test checks it on the full suite, not on this unit's run.

## Results

A run is good when every test passes with no warnings. The performance test (NFR5.1) must pass as written; if it fails, the gap is reported and the target is not loosened.
