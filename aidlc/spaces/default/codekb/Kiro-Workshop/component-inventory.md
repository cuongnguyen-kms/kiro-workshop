# Component Inventory

## Scheduling Library
`src/ContractManagement.Scheduling/`. Stateless schedule engine: `ScheduleGenerator` (implements `IScheduleGenerator`), `InputChecker`, `PaymentDates`, `PaymentScheduleBuilder` (combined payments sorted by due date then stream position, total), `ExpenseScheduleBuilder` (Overtime monthly pro-rata with remainder on the last row, AllAtOnce single row, None empty; base amount is the payment total), and the C2 types in `Models.cs`. Depends on nothing; no I/O.

## Contract API
`src/ContractManagement.Api/` except Persistence. `ContractEndpoints` (routes, including preview), `ContractService` (validate, size-check, generate, save; `Preview` generates without saving), `ContractValidator` and `SizeEstimator`, request and response records with `ContractMapper` and the preview records in `Contracts/Preview.cs`, the `Contract` aggregate (`Domain/`), and `ApiHosting` (DI, error handler, CORS, health, startup database check). Depends on Scheduling Library and Contract Persistence.

## Contract Persistence
`src/ContractManagement.Api/Persistence/`. `IContractRepository` and `ContractRepository`, `ContractDbContext` (snake_case tables `contracts`, `payment_streams`, `payment_schedule_entries`, `expense_schedule_entries`; numeric(18,2) money; text enums), `DatabaseSettings` (connection string, password only from configuration), `DesignTimeFactory`, and the InitialCreate migration. Depends on PostgreSQL.

## Contract Web App
`web/src/`. Pages `ContractsListPage`, `NewContractPage` (hosts the form, one save at a time), `ContractDetailPage`; feature components `ContractForm`, `StreamCard`, `ExpenseSection`, `StreamsSummary`, `SchedulePreview` (two buttons and a dialog showing previewed schedules), `ScheduleTabs` (saved schedules, exports `paymentRows`), `ContractsTable`, `Pager`; shared `ScheduleTable` (paged native table, rows pre-formatted strings, one pre-formatted total string, right-aligns the last column only), `DateField`, `MoneyField`, `ErrorSummary`, `FieldError`; `validation.ts` (form model, rules, `toCreateRequest`, `toPreviewRequest`, preview reasons), `display.ts` (`formatMoney` uses `toFixed(2)`, `formatDate`, `formatMonth`, labels); shadcn primitives in `components/ui/` (now including `dialog`); API client in `api/`. Depends on the Contract API over HTTP.

## Test Suites
`tests/ContractManagement.Scheduling.Tests`, `tests/ContractManagement.Api.Tests` (validator, service, settings, fakes), `tests/ContractManagement.Api.IntegrationTests` (API, repository, data model, target checks against a local PostgreSQL test database), and `web/src/test/` (Vitest). Counts: `code-quality-assessment.md`.
