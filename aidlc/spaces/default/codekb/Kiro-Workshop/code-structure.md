# Code Structure

## Layout
```
ContractManagement.slnx
scripts/verify.ps1                     full verification (dotnet then npm)
src/ContractManagement.Scheduling/     library: Models.cs (C2 types), ScheduleGenerator, InputChecker, PaymentDates, PaymentScheduleBuilder, ExpenseScheduleBuilder
src/ContractManagement.Api/            Program.cs; Endpoints/; Services/; Validation/; Contracts/ (Requests, Responses, Preview); Domain/; Hosting/; Persistence/ (+ Migrations/)
tests/ContractManagement.*.Tests       xUnit (Scheduling, Api unit, Api integration)
web/src/                               api/, pages/, contracts/, components/ (+ ui/ shadcn), lib/, test/
```

## File Classification
- Controller/Handler: `Endpoints/ContractEndpoints.cs` (create, preview, list, get)
- Service: `Services/ContractService.cs` (`CreateAsync`, `Preview`, `GetAsync`, `ListAsync`); schedule engine `ScheduleGenerator.cs` and builders
- Utility/Validator: `Validation/ContractValidator.cs`, `SizeEstimator.cs`, `InputChecker.cs`
- Model/DTO: `Contracts/Requests.cs`, `Contracts/Responses.cs` (includes `ContractMapper`), `Contracts/Preview.cs` (`PreviewKind`, preview response records), `Domain/Contract.cs`, `Scheduling/Models.cs`
- Repository: `Persistence/ContractRepository.cs` behind `IContractRepository`; `ContractDbContext.cs`
- Configuration: `Hosting/ApiHosting.cs`, `DatabaseSettings.cs`, `appsettings*.json`
- Migration: `Persistence/Migrations/` (one: 20261117145419_InitialCreate, plus the model snapshot)
- Frontend pages: `web/src/pages/` (List, New, Detail), each lazy-loaded in `App.tsx`
- Frontend feature code: `web/src/contracts/` (ContractForm, StreamCard, ExpenseSection, SchedulePreview, ScheduleTabs, StreamsSummary, ContractsTable, Pager, validation.ts, display.ts)
- Frontend shared: `web/src/components/` (ScheduleTable, DateField, MoneyField, ErrorSummary, FieldError) and `ui/` (shadcn: alert-dialog, alert, button, calendar, card, dialog, form, input, label, popover, radio-group, select, skeleton, table, tabs)
- Frontend API: `web/src/api/contracts.ts` (createContract, previewSchedule, listContracts, getContract), `types.ts`
- Tests: `tests/*`, `web/src/test/*.test.tsx` with `fakeApi.ts` mocking `@/api/contracts`

## Patterns
- Primary-constructor DI in C#; records for requests, responses and engine types.
- Lenient JSON converters turn bad dates or numbers into null so the validator reports them.
- Frontend: react-hook-form with a Zod resolver; typed result unions (`kind: 'ok' | 'validation' | 'failure'`) from the API client; `@/` path alias to `web/src`.
- Schedule tables: rows are `Record<string, string>` pre-formatted by `display.ts` helpers; columns are `{key, label, align}` lists defined per file (duplicated in ScheduleTabs and SchedulePreview).
