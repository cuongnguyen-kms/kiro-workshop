# Code Generation Plan: u1-schedule-engine

## Summary

- Builds: the .NET 10 schedule rules library (payment dates, Overtime split, rounding) and the solution it lives in
- Touches: a new solution file, `src/ContractManagement.Scheduling/`, `tests/ContractManagement.Scheduling.Tests/` and `scripts/verify.ps1`
- Tests: about 18 xUnit unit tests (one or more per acceptance criterion and rule), test-after, no mocks

## Context

- Unit: U1 `u1-schedule-engine` (library). Stories: US4.1, US5.1. Design: `functional-design/` (BR1.1 to BR2.6), `nfr-requirements/` (NFR1.1 to NFR7.2), contract C2 in `contract-summary.md`.
- Methodology: test-after. For the one testable layer here (business logic), each step implements it and the next step writes and runs its tests.
- The solution file and `scripts/verify.ps1` are created here, because this is the first unit built. U2 and U3 add their own projects to them.
- Prerequisite: the .NET 10 SDK is installed on this machine (see the external dependency map). Step 1 checks it before anything is written.

## Steps

- [x] **Step 1: Check the build tools.** Run `dotnet --version` and confirm it reports 10.x. If it does not, stop and tell the person; nothing is written. Traces to NFR1.1.
- [x] **Step 2: Project structure and production configuration skeleton.**
  - Create the solution `ContractManagement.slnx` at the workspace root (the .NET 10 default solution format).
  - Create the class library `src/ContractManagement.Scheduling/ContractManagement.Scheduling.csproj` targeting `net10.0`, with nullable reference types on, warnings treated as errors, and no package references (NFR1.1, NFR1.2).
  - Add the analyzer rule that bans file, network, environment, console and clock APIs in this project (`BannedSymbols.txt` with the Microsoft.CodeAnalysis.BannedApiAnalyzers rule): NFR3.2. This analyzer is the only package reference, and it is a build-time analyzer that is not shipped with the library, so NFR1.2 holds for the shipped code.
  - Create `.gitignore` entries for `bin/` and `obj/` only if the root `.gitignore` does not already cover them.
- [x] **Step 3: Bootstrap the test runner and record the exact unit-scoped command.**
  - Create the xUnit test project `tests/ContractManagement.Scheduling.Tests/ContractManagement.Scheduling.Tests.csproj` referencing the library (NFR7.1, NFR7.2).
  - The unit-scoped command, recorded in `unit-test-instructions.md`, is: `dotnet test tests/ContractManagement.Scheduling.Tests/ContractManagement.Scheduling.Tests.csproj`.
  - Add one placeholder-free smoke test (a call to `Generate` with a one-stream OneTime request) to prove the runner works before the real tests are written. It stays as the BR1.5 test.
- [x] **Step 4: Business logic: public interface and records (contract C2).** Create in `src/ContractManagement.Scheduling/`:
  - `Frequency` and `ExpenseMethod` enums.
  - Records `StreamInput`, `ExpenseInput`, `ScheduleRequest`, `PaymentEntry`, `ExpenseEntry`, `ScheduleResult`, with the exact members from contract C2, immutable, read-only lists (NFR4.3).
  - Interface `IScheduleGenerator` with `Generate(ScheduleRequest)`.
  - Money is `decimal` and dates are `DateOnly` (NFR4.1). Entities named: StreamInput, ExpenseInput, ScheduleRequest, PaymentEntry, ExpenseEntry, ScheduleResult, all from `entities.md`.
- [x] **Step 5: Business logic: input checker.** `InputChecker` applies BR1.8 (at least one stream; amounts above zero; `EndDate` not before `StartDate`, equal for OneTime; Overtime has both dates, end not before start; AllAtOnce has a date) and counts the payment entries the streams would produce, rejecting more than 100,000 (NFR6.1, NFR6.2). Throws `ArgumentException` naming the broken promise.
- [x] **Step 6: Business logic: payment schedule builder and date stepping helper.** Implements BR1.1 to BR1.7: first payment on the start date; Weekly +7 days; Monthly, Quarterly and Annually from the original start day with the day cut back to the last day of a short month; stop after the end date; OneTime pays once; combine streams ordered by due date then stream position; total with `decimal`. Traces to AC4.1.1 to AC4.1.6.
- [x] **Step 7: Business logic: expense schedule builder.** Implements BR2.1 to BR2.6: base amount is the payment total; AllAtOnce gives one row; Overtime gives one row per calendar month with amount = base x days in that month's slice / total days, rounded to 2 decimals half away from zero, and the last row = base minus the other rows; None gives no rows and a total of 0. Traces to AC5.1.1 to AC5.1.5.
- [x] **Step 8: Business logic: `ScheduleGenerator` class.** Implements `IScheduleGenerator`: runs the input checker, the payment builder, then the expense builder, and returns the `ScheduleResult`. Stateless.
- [x] **Step 9: Business logic: write and run the unit tests (test-after).** In `tests/ContractManagement.Scheduling.Tests/`, write the tests listed in `unit-test-instructions.md`, then run the unit-scoped command. Every test must pass. The worked examples in `functional-spec.md` are the expected values. Traces to AC4.1.1 to AC5.1.5 and BR1.1 to BR2.6.
- [x] **Step 10: Performance check (NFR5.1).** Add one test that generates 10 Weekly streams over 10 years plus a 10-year Overtime expense and asserts `Generate` finishes in under 200 ms. If the test fails because the machine is slow, the target stays and the gap is reported to the person; it is not loosened.
- [x] **Step 11: Environment and build configuration.** Create `scripts/verify.ps1`: it runs `dotnet restore`, `dotnet build --no-restore -warnaserror`, then `dotnet test --no-build` for the whole solution, and exits with the first non-zero code. Later units add their own checks to this file. Traces to the recorded verification command and NFR7.2.
- [x] **Step 12: Documentation and traceability.** Add XML doc comments on the public interface and records. Write `code-summary.md` (what was built, with what, and how to run it), `unit-test-instructions.md` is already written, `source-manifest.json` and `traceability.json` per the stage steps.

## Test Steps Included

The plan includes test files (Steps 3, 9, 10) and test configuration (Step 3: the xUnit project and its runner), as the test strategy requires.

## Assumptions

- The test framework is xUnit and the solution format is `.slnx`. Neither was chosen by the requester; both are the .NET 10 defaults and are easy to change (see `tech-stack-decisions.md`, NFR7.2).
- The one added package is the banned-API analyzer (`Microsoft.CodeAnalysis.BannedApiAnalyzers`), which only runs at build time. It is the way NFR3.2 is enforced by the build instead of by review alone.

## Story to Step Map

| Story | Steps |
|---|---|
| US4.1 | 4, 5, 6, 8, 9 |
| US5.1 | 4, 5, 7, 8, 9 |
| NFR5.1 | 10 |
| Verification command | 11 |

## Testing Contract

```json
{
  "version": 1,
  "methodology": "test-after",
  "source": "org",
  "ordering": "implement each applicable testable layer, then write and run that layer's tests.",
  "scope": "classic",
  "test_strategy": "minimal",
  "project_type": "greenfield",
  "applicable_notes": [
    {
      "layer": "org",
      "text": "We treat tests as a first-class deliverable in every Bolt. The specific\nmethodology (TDD, BDD, ATDD, or classic test-after) is affirmed at\npractices-discovery and recorded in `team.md` under this heading with explicit\n`Methodology` and `Ordering` fields; Code Generation resolves those fields\nindependently from coverage, tooling, and scope notes.\n\nWhen no posture has been affirmed, our default per scope is:\n- **Methodology**: test-after\n- **Ordering**: implement each applicable testable layer, then write and run\n  that layer's tests.\n- `mvp`, `enterprise`, `feature`, `infra` add an 80% line-coverage\n  floor and CI execution before merge.\n- `bugfix`, `security-patch` add a targeted regression for the specific\n  bug/vulnerability and require the existing suite to remain green.\n- `express` uses the Minimal strategy: requirement-driven unit tests (one per\n  requirement, with a happy-path floor per component); existing tests remain\n  green.\n- `poc`, `refactor`, `workshop`, `classic` add no extra new-test floor and require the\n  existing suite to remain green.\n\nThe active `Test Strategy` still applies in every scope and determines test\nvolume/types. Scope floors are additive; they never reduce or replace the\nselected strategy.\n\nBuild and Test verifies defined coverage floors and affirmed quality targets;\nthey may not be weakened to make a step pass.\n\nAffirm a stricter posture in `team.md` if the team commits to one."
    }
  ],
  "obligations": {
    "strategy": "minimal",
    "strategy_volume": [
      "One verifiable test per requirement at the narrowest effective level.",
      "At least one happy-path unit test per component.",
      "Unit tests are the default; a bugfix/security scope floor may require an integration or E2E regression when that is the narrowest level that reproduces the defect."
    ],
    "scope_floor": [
      "Keep the existing test suite green.",
      "This scope adds no extra new-test floor beyond the selected test strategy."
    ],
    "combination_rule": "Apply every selected-strategy obligation and every scope-floor obligation; neither replaces the other, and a targeted scope regression may add the narrowest necessary test type beyond the strategy default."
  },
  "plan_profile": {
    "methodology": "test-after",
    "runner_step": "Bootstrap the minimal test runner/configuration and record the exact unit-scoped command.",
    "runner_ready_before_first_test": true,
    "testable_layers": [
      "Data model / database behavior",
      "Repository / data access",
      "Business logic",
      "API / endpoint",
      "Frontend behavior"
    ],
    "steps": [
      "Project structure and production configuration skeleton.",
      "Bootstrap the minimal test runner/configuration and record the exact unit-scoped command.",
      "Data model / database behavior - implement.",
      "Data model / database behavior - write and run its tests after implementation.",
      "Repository / data access - implement.",
      "Repository / data access - write and run its tests after implementation.",
      "Business logic - implement.",
      "Business logic - write and run its tests after implementation.",
      "API / endpoint - implement.",
      "API / endpoint - write and run its tests after implementation.",
      "Frontend behavior - implement.",
      "Frontend behavior - write and run its tests after implementation.",
      "Environment/build configuration.",
      "Documentation and traceability."
    ]
  },
  "input_sha256": "sha256:39866c06a848b589e1cecab794bb7df06d0190eb9b72703333ba72e6d8a39031",
  "contract_sha256": "sha256:818cff968cd44820f3eb2e38374d17d8c73f417d0164e1bcc21bcef03e530848"
}
```
