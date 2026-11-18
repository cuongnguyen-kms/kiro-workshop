# Code Generation Plan: u1-schedule-balance

## Summary

- Builds: the monthly balance builder (contract C2) and the new Balance list on the schedule result, filled for previews.
- Touches: src/ContractManagement.Scheduling (Models.cs, new BalanceScheduleBuilder.cs, ScheduleGenerator.cs), tests/ContractManagement.Scheduling.Tests (new BalanceScheduleBuilderTests.cs, ScheduleGeneratorTests.cs), tests/ContractManagement.Api.Tests/Fakes.cs.
- Tests: about 22 new xUnit unit tests for the builder and 2 for the generator; the existing .NET suite stays green.

Methodology: test-after (team.md). Each layer is implemented, then its tests are written and run. This Unit has only the business-logic layer: no database, repository, endpoint or frontend change (those are U2 and U3).

Inputs: `construction/u1-schedule-balance/functional-design/` (functional-spec.md, rules.md, entities.md), `construction/u1-schedule-balance/nfr-design/security-design.md`, contract C2 in `inception/contract-design/contract-summary.md`, U1 in `inception/units-generation/unit-of-work.md`, and `inception/requirements-analysis/requirements.md`.

## Steps

- [x] **Step 1. Verify the test runner.** Run `dotnet test tests/ContractManagement.Scheduling.Tests` on the current code and confirm it passes. Stop the running API (`ContractManagement.Api` process) first if it locks the build output. (Runner readiness.)
- [x] **Step 2. Data model: `BalanceRow` and `ScheduleResult.Balance`** in `src/ContractManagement.Scheduling/Models.cs`.
  - New record `BalanceRow(DateOnly PeriodStart, DateOnly PeriodEnd, decimal Expense, decimal AccrualPrepaid, decimal CumulativeBalance, decimal StAccruedLiability, decimal LtAccruedLiability, decimal StPrepaidAsset, decimal LtPrepaidAsset)` with XML summaries for every field (contract C2; entities.md; FR1.1, FR2.1 to FR2.4).
  - New interface `IBalanceScheduleBuilder` with `IReadOnlyList<BalanceRow> Build(IReadOnlyList<PaymentEntry> payments, IReadOnlyList<ExpenseEntry> expenses)` (contract C2).
  - `ScheduleResult` gains a fifth positional field `IReadOnlyList<BalanceRow> Balance` (contract C2, BR3.1). Stories: US1.1, US2.1, US2.2.
- [x] **Step 3. Business logic: `BalanceScheduleBuilder`** in a new file `src/ContractManagement.Scheduling/BalanceScheduleBuilder.cs`, implementing `IBalanceScheduleBuilder` (US1.1, US2.1, US2.2).
  - Input checks: null lists throw `ArgumentNullException`; empty expenses return an empty list with no totals check (BR1.1, FR1.4); otherwise unequal totals throw `ArgumentException` giving both totals (BR3.2, SEC1).
  - Months: payment due dates and expense period starts grouped by year and month into a sorted map (BR1.1).
  - Period: the Overtime row's period when the month's expense row has a start different from its end, else the first to last day of the month (BR1.2).
  - Expense, (Accrual)/Prepaid and Cumulative Balance per month (BR1.3, BR2.1, BR2.2).
  - Look-ahead: target month = month start plus 12 calendar months (`AddMonths(12)`); a binary search over the sorted months returns the latest month on or before the target, never an exact-match lookup (BR2.3; leap year E8; month without a row E7).
  - ST and LT split from absolute values (BR2.4) and columns by sign (BR2.5). `decimal` only, no rounding (NFR3.1).
- [x] **Step 4. Business logic: generator wiring** in `src/ContractManagement.Scheduling/ScheduleGenerator.cs`. After the payment and expense rows are built, call a static `BalanceScheduleBuilder` instance and pass its rows as `Balance` (BR3.1, FR2.6). The generator keeps its parameterless constructor, so the API's DI registration does not change.
- [x] **Step 5. Keep existing callers compiling.** Update `tests/ContractManagement.Api.Tests/Fakes.cs` (the fake generator) to pass `Balance`, built with `BalanceScheduleBuilder` from its own rows (NFR2.1). No other code constructs `ScheduleResult`.
- [x] **Step 6. Business logic tests (after implementation)** in a new file `tests/ContractManagement.Scheduling.Tests/BalanceScheduleBuilderTests.cs`:
  - One test per worked example E1 to E10 in functional-spec.md (NFR3.2).
  - One test per BR2.4 branch: B = 0; L = 0; L opposite sign; abs L at least abs B; abs L below abs B.
  - One test per BR2.5 sign: a negative balance fills only the accrued columns; a positive one fills only the prepaid columns.
  - Checks: null payments, null expenses, unequal totals (message has both totals), empty expenses returns empty with no throw even when payments exist (SEC1, BR3.2).
  - Every row in every test: ST plus LT equals the absolute balance, all four split columns are never negative, and the last row's balance is 0 (NFR3.1, BR2.2).
  - Same input twice gives equal rows (NFR3.3).
  - Timing: 10 streams over 10 years: one warm-up call, then the best of 3 timed calls under 50 ms (NFR1.1).
- [x] **Step 7. Generator tests (after implementation)** in `tests/ContractManagement.Scheduling.Tests/ScheduleGeneratorTests.cs`: one test that `Generate` returns `Balance` equal to `Build(Payments, Expenses)` for an Overtime contract, and one that `Balance` is empty for method None (BR3.1, FR1.4). Existing tests stay unchanged (NFR2.1).
- [x] **Step 8. Run the Unit's tests and the build.** `dotnet build ContractManagement.slnx -warnaserror`, then the commands in `unit-test-instructions.md`. All pass with zero warnings.
- [x] **Step 9. Documentation and traceability.** XML docs on all new public types; `code-summary.md`, `traceability.json` (one existing file per target) and `source-manifest.json`.

Test configuration: no change. The existing xUnit project `tests/ContractManagement.Scheduling.Tests` already references the Scheduling project.

## Story traceability

| Story | Steps |
|---|---|
| US1.1 (monthly rows, periods, payment-only months, method None) | 2, 3, 6 |
| US2.1 ((Accrual)/Prepaid and Cumulative Balance) | 2, 3, 6 |
| US2.2 (ST/LT split, accrued or prepaid columns) | 2, 3, 6 |
| FR2.6 / FR4.3 (preview and saved contract give the same rows) | 4, 7 |

## Testing Contract

```json
{
  "version": 1,
  "methodology": "test-after",
  "source": "team",
  "ordering": "We implement each layer first, then write and run that layer's tests, then run `scripts/verify.ps1`.",
  "scope": "classic",
  "test_strategy": "standard",
  "project_type": "brownfield",
  "applicable_notes": [
    {
      "layer": "org",
      "text": "We treat tests as a first-class deliverable in every Bolt. The specific\nmethodology (TDD, BDD, ATDD, or classic test-after) is affirmed at\npractices-discovery and recorded in `team.md` under this heading with explicit\n`Methodology` and `Ordering` fields; Code Generation resolves those fields\nindependently from coverage, tooling, and scope notes.\n\nWhen no posture has been affirmed, our default per scope is:\n- **Methodology**: test-after\n- **Ordering**: implement each applicable testable layer, then write and run\n  that layer's tests.\n- `mvp`, `enterprise`, `feature`, `infra` add an 80% line-coverage\n  floor and CI execution before merge.\n- `bugfix`, `security-patch` add a targeted regression for the specific\n  bug/vulnerability and require the existing suite to remain green.\n- `express` uses the Minimal strategy: requirement-driven unit tests (one per\n  requirement, with a happy-path floor per component); existing tests remain\n  green.\n- `poc`, `refactor`, `workshop`, `classic` add no extra new-test floor and require the\n  existing suite to remain green.\n\nThe active `Test Strategy` still applies in every scope and determines test\nvolume/types. Scope floors are additive; they never reduce or replace the\nselected strategy.\n\nBuild and Test verifies defined coverage floors and affirmed quality targets;\nthey may not be weakened to make a step pass.\n\nAffirm a stricter posture in `team.md` if the team commits to one."
    },
    {
      "layer": "team",
      "text": "- **Methodology**: test-after\n- **Methodology evidence**: Earlier build-and-test records show tests written during Code Generation with traceability checked afterwards (one test per requirement). No feature files, scenario specs or test-first history exist, and the team confirmed test-after in the interview.\n- **Ordering**: We implement each layer first, then write and run that layer's tests, then run `scripts/verify.ps1`.\n- We set no coverage minimum. Every requirement must have a test, and all tests must pass.\n- Test stack: xUnit for .NET (unit and integration against a local PostgreSQL), Vitest with Testing Library, vitest-axe and jsdom for the web.\n- Existing tests assert the current schedule table output (two-column expense rows, `toFixed` strings), so we update them with this change.\n- Checks that need a real browser (timing, axe) run only in jsdom, and we accept that."
    }
  ],
  "obligations": {
    "strategy": "standard",
    "strategy_volume": [
      "Five to eight tests per component.",
      "Unit tests plus integration tests for key boundaries.",
      "Add E2E, performance, or security tests when requirements demand them."
    ],
    "scope_floor": [
      "Keep the existing test suite green.",
      "This scope adds no extra new-test floor beyond the selected test strategy."
    ],
    "combination_rule": "Apply every selected-strategy obligation and every scope-floor obligation; neither replaces the other, and a targeted scope regression may add the narrowest necessary test type beyond the strategy default."
  },
  "plan_profile": {
    "methodology": "test-after",
    "runner_step": "Verify the existing test runner/configuration and record the exact unit-scoped command.",
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
      "Verify the existing test runner/configuration and record the exact unit-scoped command.",
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
  "input_sha256": "sha256:436e600840bd38d5a7ef142e1ee8b2c18c181ac6cc4a4b0426a00b213092c4c9",
  "contract_sha256": "sha256:5a264bc2c9e1a54d8cad4422ec3c2718c6155aaa7f2f9ddcd4dbfc011d155fb7"
}
```
