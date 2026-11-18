# Code Generation Plan: u2-contract-api

## Summary

- Builds: the .NET 10 contract API (create, list and read contracts) on PostgreSQL, calling the existing schedule library
- Touches: new `src/ContractManagement.Api/`, `tests/ContractManagement.Api.Tests/`, `tests/ContractManagement.Api.IntegrationTests/`, `ContractManagement.slnx`, `.config/dotnet-tools.json`, `scripts/verify.ps1`
- Tests: about 25 tests (unit tests for validation and the service, integration tests against local PostgreSQL for the repository and the endpoints)

Testing Contract: methodology test-after. Each layer is implemented, then its tests are written and run. Test strategy Minimal. Existing suite (`ContractManagement.Scheduling.Tests` and the frontend steps in `scripts/verify.ps1`) stays green.

Stack (from NFR design): ASP.NET Core minimal APIs, EF Core 10 with Npgsql, xUnit, `Microsoft.AspNetCore.Mvc.Testing`. The only tool this plan installs is `dotnet-ef`, as a local tool in `.config/dotnet-tools.json`, to create the first migration.

Source layout inside `src/ContractManagement.Api/`: `Contracts/` (request and response shapes), `Domain/` (the four entities), `Validation/`, `Services/`, `Persistence/` (DbContext, repository, migrations), `Endpoints/`, `Program.cs`.

## Steps

- [x] **Step 1. Project structure and production configuration skeleton.** Create `src/ContractManagement.Api` (web project, net10.0, nullable on, warnings as errors, reference to `ContractManagement.Scheduling`), packages `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design` (private), `Microsoft.AspNetCore.OpenApi`. Add the project to `ContractManagement.slnx`. `appsettings.json` with host, port, database, user (no password) and `Cors:AllowedOrigin`; `appsettings.Development.json` with `http://localhost:5173`. Add the tool manifest with `dotnet-ef`. Implements NFR1.1, NFR1.2, NFR1.3, NFR2.3, NFR2.5, BR6.2.
- [x] **Step 2. Bootstrap the test runner and record the unit-scoped command.** Create `tests/ContractManagement.Api.Tests` (unit tests, xUnit, same settings as the Scheduling tests) and `tests/ContractManagement.Api.IntegrationTests` (xUnit, `Microsoft.AspNetCore.Mvc.Testing`, user secrets id). Add both to the solution. Record the commands in `unit-test-instructions.md`. Implements NFR7.1.
- [x] **Step 3. Data model - implement.** `Domain/` entities `Contract` (contractId, name, startDate, endDate, expenseMethod, expenseStartDate, expenseEndDate, expenseDate, createdAt), `PaymentStream` (streamId, contractId, position, amount, frequency, startDate, endDate), `PaymentScheduleEntry` (paymentEntryId, contractId, streamId, dueDate, amount), `ExpenseScheduleEntry` (expenseEntryId, contractId, periodStart, periodEnd, amount), exactly as in `entities.md`. `Persistence/ContractDbContext` maps them to snake_case tables with `numeric(18,2)` money, text enums, cascade delete from contract, and the indexes `contracts(start_date desc, name, id)`, `payment_streams(contract_id)`, `payment_schedule_entries(contract_id, due_date)`, `expense_schedule_entries(contract_id, period_start)`. Create the first migration `InitialCreate`. Implements ENT Contract, PaymentStream, PaymentScheduleEntry, ExpenseScheduleEntry; NFR4.3, NFR5.9.
- [x] **Step 4. Data model - write and run its tests.** Integration tests: the migration creates the four tables; a saved contract keeps `numeric(18,2)` amounts exactly (for example 1234.50); deleting a contract deletes its children. Covers NFR4.3.
- [x] **Step 5. Repository - implement.** `Persistence/IContractRepository` and `ContractRepository`: `SaveAsync` (one explicit transaction, batched `SaveChanges`), `GetAsync` (split query, no tracking, rows in stored order, not found returns null), `ListAsync` (count plus one page ordered by start date descending, name, id; summary columns only). Implements BR5.1, BR5.2, BR5.3, NFR5.1, NFR5.3, NFR5.4, NFR4.1.
- [x] **Step 6. Repository - write and run its tests.** Integration tests against `contract_management_test`: save then get returns every part (AC6.2.1); a failure on the last write leaves no contract or child rows (AC6.1.2); list ordering and paging, a page past the last, empty database (AC6.3.1, AC6.3.2); unknown id returns null (AC6.2.2).
- [x] **Step 7. Business logic - implement.** `Validation/ContractValidator` (BR1.1, BR1.2, BR2.1 to BR2.4, BR2.6, BR3.1 to BR3.4; collects every message with its field path; skips the date-range checks when the contract dates are invalid), `Validation/SizeEstimator` (BR5.4), `Services/ContractService` (validate, apply BR2.5 and BR3.5, size check, build the C2 `ScheduleRequest`, call `IScheduleGenerator`, check totals BR4.2, save through the repository, return the contract). Register the real `ScheduleGenerator` in dependency injection. Implements BR1.1 to BR5.4 except the HTTP-only rules, BR1.3 (no duplicate check), NFR6.1, NFR6.2.
- [x] **Step 8. Business logic - write and run its tests.** Unit tests with a fake generator and a fake repository: one valid contract is saved (AC6.1.1); each validation rule gives its exact message and path (AC1.1.2, AC1.1.3, AC2.1.3, AC2.1.4, AC3.1.4, AC3.1.5); One-time stream stores end date equal to start date (AC2.1.5); expense fields are ignored for the wrong method (AC3.1.1 to AC3.1.3); two contracts with the same name both save (BR1.3); the 10,000 row cap rejects (BR5.4); a generator or repository failure saves nothing and surfaces as a failure (AC6.1.2). One test with the real generator checks AC4.1.1 and AC5.1.2 through the service.
- [x] **Step 9. API endpoint layer - implement.** `Endpoints/ContractEndpoints` mapping POST, GET list and GET by id under `/api/v1/contracts`; `Contracts/` request and response records matching C1 (camelCase JSON, dates as `YYYY-MM-DD`, enums as strings, decimals); 201 with `Location`; ValidationProblem 400; 404 Problem Details 'Contract not found.'; paging validation (BR5.3); CORS policy from `Cors:AllowedOrigin` (BR6.1); generic exception handler with `traceId` (BR6.3, NFR6.4, NFR6.9); 256 KB body limit (NFR6.3); security headers; HTTPS redirection and HSTS outside Development; request logging (NFR6.7 to NFR6.10); `GET /health` (NFR4.7); development-only OpenAPI document; startup that applies migrations in Development and creates the database if missing (BR6.2, NFR4.5); connection string built from configuration with the password from user secrets or environment (NFR2.1).
- [x] **Step 10. API - write and run its tests.** Integration tests with `WebApplicationFactory` against `contract_management_test`: create returns 201 with both schedules and a `Location` header (AC4.1.1, AC5.1.1, AC6.1.1); invalid body returns 400 with messages keyed by field path and nothing saved; get unknown id returns 404 (AC6.2.2); list returns the page shape with an empty first page (AC6.3.2); a disallowed origin gets no CORS header; `/health` returns 200; create with 10 streams over 10 years responds within 2 seconds (NFR5.1); the generated OpenAPI paths match the Contract Summary (NFR4.9).
- [x] **Step 11. Frontend behavior.** Not applicable to this Unit; delivered by `u3-contract-web`.
- [x] **Step 12. Environment and build configuration.** Set the user secret for the local database password (`dotnet user-secrets set` on the API project, value supplied by you, never written to a file). Update `scripts/verify.ps1` only to run the new projects through the existing `dotnet build` and `dotnet test` steps (they already run solution-wide); no new step is needed unless the solution file does not pick them up. Ensure the integration project creates and empties `contract_management_test` itself.
- [x] **Step 13. Documentation and traceability.** XML summaries on public types; write `code-summary.md` (what was built, how to run it, configuration keys, how to set the password) and `traceability.json` (acceptance criteria and business rules to code and tests); write `source-manifest.json`.

## Story and rule traceability

| Story | Plan steps |
|---|---|
| US1.1 | 7, 8, 9, 10 |
| US2.1 | 7, 8, 9, 10 |
| US3.1 | 7, 8, 9, 10 |
| US4.1 | 7, 8 (through U1), 10 |
| US5.1 | 7, 8 (through U1), 10 |
| US6.1 | 5, 6, 7, 8, 9, 10 |
| US6.2 | 5, 6, 9, 10 |
| US6.3 | 5, 6, 9, 10 |

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
