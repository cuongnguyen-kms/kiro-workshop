# Dependencies

## Internal
- Contract API references Scheduling Library (project reference).
- Api.Tests and Api.IntegrationTests reference Contract API (internals visible to both); Scheduling.Tests references Scheduling Library.
- Contract Web App depends on the Contract API only through HTTP (`/api/v1`), configured by `VITE_API_BASE_URL`. No code is shared.
- Inside the web app: pages use `api/contracts.ts`, `contracts/*` and `components/*`; `ContractForm` uses `validation.ts` and `SchedulePreview`; `SchedulePreview` and `ScheduleTabs` both use `ScheduleTable` and `display.ts`; `SchedulePreview` also uses `components/ui/dialog` and `previewSchedule`. No circular imports seen.
- Inside the API: `ContractService` uses `IScheduleGenerator`, `IContractRepository`, `ContractValidator`, `SizeEstimator`; response records in `Contracts/` depend on `Domain` entities and Scheduling enums.

## External
- Runtime services: PostgreSQL (password via user secrets or `ConnectionStrings__ContractDbPassword`).
- NuGet and npm packages: versions in `technology-stack.md`. No package manifests lock .NET versions centrally; `web/package-lock.json` locks npm.
- `npm audit --omit=dev --audit-level=high` runs in `scripts/verify.ps1`; one NuGet advisory is handled by pinning Microsoft.OpenApi.
