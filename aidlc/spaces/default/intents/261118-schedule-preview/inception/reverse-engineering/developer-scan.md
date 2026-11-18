## Developer Code Scan Results

### Scan Coverage
- **Analyzed deeply**:
  - `src/ContractManagement.Scheduling/` (Models.cs, ScheduleGenerator.cs, InputChecker.cs, csproj; builders read as summaries only)
  - `src/ContractManagement.Api/` (Program.cs, Endpoints, Services, Validation, Contracts, Domain, Hosting, Persistence/IContractRepository, ContractDbContext, DatabaseSettings, csproj)
  - `web/src/api/`, `web/src/pages/NewContractPage.tsx`, `web/src/pages/ContractDetailPage.tsx`, `web/src/contracts/` (ContractForm, validation, ScheduleTabs, ExpenseSection, display), `web/src/components/ScheduleTable.tsx`, `web/src/App.tsx`, `web/eslint.config.js`, `web/vite.config.ts`, `web/package.json`, `scripts/verify.ps1`
- **Skimmed only**:
  - `src/ContractManagement.Api/Persistence/Migrations/`, `ContractRepository.cs`, `DesignTimeFactory.cs`
  - `src/ContractManagement.Scheduling/` PaymentDates.cs, PaymentScheduleBuilder.cs, ExpenseScheduleBuilder.cs, BannedSymbols.txt
  - `tests/` (file names and test counts only)
  - `web/src/components/ui/` (shadcn primitives, listing only), `web/src/test/`, remaining `web/src/contracts/*` and `web/src/components/*` (StreamCard, StreamsSummary, DateField, MoneyField, ErrorSummary, FieldError, ContractsTable, Pager), `web/src/pages/ContractsListPage.tsx`
- **Left out**: .NET `bin/` and `obj/` beside each csproj; `web/node_modules/`, `web/dist/` (not opened); AI-DLC install (`.kiro/`, `aidlc/`, its marked sections of `AGENTS.md` and `.gitignore`)

### Packages Found
- ContractManagement.Scheduling — .NET class library (net10.0) — C# — pure schedule engine, contract C2 (`IScheduleGenerator`)
- ContractManagement.Api — ASP.NET Core minimal API (net10.0) — C# — contract C1 (`/api/v1`), validation, EF Core persistence on PostgreSQL
- ContractManagement.Scheduling.Tests, ContractManagement.Api.Tests, ContractManagement.Api.IntegrationTests — xUnit test projects — C#
- web — React 19 + Vite SPA — TypeScript — contract list, new contract form, contract detail

### Build System
- **Type**: .NET SDK solution (`ContractManagement.slnx`) plus npm/Vite for `web/`
- **Config Files**: `ContractManagement.slnx`, three src/test csproj groups, `web/package.json`, `web/vite.config.ts`, `web/tsconfig*.json`, `web/eslint.config.js`, `web/components.json`, `scripts/verify.ps1`
- **Build Dependencies**: Api references Scheduling; test projects reference their subject; web is independent and talks to the Api over HTTP

### APIs Discovered
- REST — `src/ContractManagement.Api/Endpoints/ContractEndpoints.cs` — 3 endpoints under `/api/v1`: `POST /contracts` (createContract, 201 or 400 validation problem), `GET /contracts` (list, page and pageSize), `GET /contracts/{id}` (404 problem when missing); plus `GET /health` and OpenAPI in Development
- Library — `IScheduleGenerator.Generate(ScheduleRequest) : ScheduleResult` (C2), registered as a singleton in `ApiHosting.AddContractApi`

### Frameworks & Libraries
- .NET 10, ASP.NET Core minimal API, EF Core 10.0.4 with Npgsql provider 10.0.3, Microsoft.AspNetCore.OpenApi 10.0.4, Microsoft.OpenApi 2.13.1 (advisory pin), BannedApiAnalyzers 3.3.4
- React ^19.3.1, react-router-dom ^7, react-hook-form ^7 with zod ^4 and @hookform/resolvers, shadcn/ui on radix-ui, Tailwind CSS 4, lucide-react, react-day-picker, date-fns, Vite ^8, TypeScript ~6.0, Vitest ^5 with Testing Library and vitest-axe

### Test Coverage
- **Test Directories**: `tests/ContractManagement.Scheduling.Tests` (19 facts), `tests/ContractManagement.Api.Tests` (35 facts), `tests/ContractManagement.Api.IntegrationTests` (24 facts, needs local PostgreSQL), `web/src/test/` (9 files)
- **Test Frameworks**: xUnit 2.9.3; Vitest with jsdom, Testing Library, vitest-axe; web tests mock the API client with `web/src/test/fakeApi.ts`
- **Coverage Config**: absent (no coverage tool configured)

### Code Quality Indicators
- **Linting**: ESLint in `web/eslint.config.js` (bans `dangerouslySetInnerHTML`, `innerHTML`, browser storage); .NET `TreatWarningsAsErrors` and a banned-API analyzer in Scheduling
- **CI/CD**: no pipeline files; `scripts/verify.ps1` runs restore, build, test, then npm audit, lint, typecheck, test, build, size check
- **Documentation**: `web/README.md`; XML doc comments on nearly all public C# types; no root README

### Technical Debt Signals
- No schedule preview exists: schedules are only generated inside `ContractService.CreateAsync` and persisted; the API has no generate-only endpoint (`ContractEndpoints.cs`)
- Validation rules duplicated by design in `ContractValidator.cs` (server) and `web/src/contracts/validation.ts` (client); the server is authoritative
- `web/src/components/ui/` has `alert-dialog.tsx` but no `dialog.tsx`; no generic popup primitive is installed
- `ContractDetail` TypeScript type reuses `ExpenseInput` for the stored expense (`web/src/api/types.ts`)
- No coverage tooling and no CI pipeline

## Handoff Summary
- **Intent-relevant finding**: Preview needs a new no-save path. `NewContractPage.save` calls `createContract(toCreateRequest(values))` in `web/src/api/contracts.ts` (POST `/api/v1/contracts`); `ContractService.CreateAsync` (`src/ContractManagement.Api/Services/ContractService.cs`) runs `ContractValidator.Validate`, `SizeEstimator`, then `IScheduleGenerator.Generate` (C2, pure and deterministic, `Models.cs`), then saves. Reusable pieces: `toCreateRequest` and `checkContract`/`contractFormSchema` (`validation.ts`), the `CreateContractRequest` JSON shape, `ScheduleTable` (paged, memoized, takes pre-formatted rows and a total string) and `paymentRows` plus the expense row mapping inside `ScheduleTabs.tsx` (`web/src/contracts/ScheduleTabs.tsx`, expense mapping is inline in a `useMemo`, not exported). The form is react-hook-form with `mode: 'onBlur'` and exposes `form.watch()` and `getValues()` in `ContractForm.tsx`; the Section header has an `action` slot. `ContractMapper.ToDetail` (`Responses.cs`) already shapes `PaymentScheduleResponse` and `ExpenseScheduleResponse` (entries plus total) from domain entries, but takes a `Contract`, so a preview needs its own mapping from `ScheduleResult`.
- **Risks / follow-up**: CORS allows only GET and POST with Content-Type header (`ApiHosting.cs`), so a POST preview endpoint needs no CORS change. Request body limit is 256 KB and `SizeEstimator.MaxRows` is 10,000; a preview must apply the same validation and size guard (reuse `ContractValidator.Validate` and `SizeEstimator`) or run the risk of the engine's 100,000 entry limit. Stream ids are generated server-side (`Guid.NewGuid()` in `BuildStreams`), so preview payment rows need a stable stream label (the detail page labels streams by frequency and amount via `paymentRows`, which requires stream ids that match entries). The API client never retries and never sends credentials; keep that for any new call. Contract detail screens and `fakeApi.ts` mock the whole `@/api/contracts` module, so a new client function must be added to `fakeApi.ts` too. The `size` check limits only the list screen bundle (300 KB gzip); the new form code loads lazily with `NewContractPage`.
