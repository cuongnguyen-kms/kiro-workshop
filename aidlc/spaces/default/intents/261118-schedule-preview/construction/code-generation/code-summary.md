# Code Summary: schedule preview

## Created

- `src/ContractManagement.Api/Contracts/Preview.cs`: `PreviewKind`, the preview response records.
- `web/src/components/ui/dialog.tsx`: shadcn Dialog on the installed `radix-ui` package.
- `web/src/contracts/SchedulePreview.tsx`: the two buttons ("Payment schedule", "Expense schedule"), disabled reasons, and the popup with loading, table, server messages and Retry.
- `tests/ContractManagement.Api.Tests/ContractPreviewServiceTests.cs` (6 tests).
- `tests/ContractManagement.Api.IntegrationTests/PreviewApiTests.cs` (3 tests).
- `web/src/test/schedulePreview.test.tsx` (7 tests).

## Modified

- `src/ContractManagement.Api/Services/ContractService.cs`: `Preview(request, kind)` and `PreviewResult`. `CreateAsync` is unchanged.
- `src/ContractManagement.Api/Endpoints/ContractEndpoints.cs`: `POST /api/v1/contracts/preview?kind=payment|expense`, named `previewContractSchedule`, in the existing route group.
- `web/src/api/contracts.ts`, `web/src/api/types.ts`: `previewSchedule` and its types.
- `web/src/contracts/validation.ts`: `toPreviewRequest`, `canPreviewPayments`, `canPreviewExpense`, reason text.
- `web/src/contracts/ContractForm.tsx`: `SchedulePreview` beside Cancel and Save.
- `web/src/test/fakeApi.ts`: `previewSchedule` fake.
- `tests/ContractManagement.Api.IntegrationTests/ApiTests.cs`: OpenAPI test expects the new path.

## Contract note (C1)

New endpoint only (NFR5). `POST /api/v1/contracts/preview`, optional query `kind` (`payment` or `expense`, default `expense`). Body: the create body; `name` is ignored. 200: `{ paymentSchedule: { entries: [{ streamNumber, dueDate, amount }], total }, expenseSchedule: { entries: [{ periodStart, periodEnd, amount }], total } }`. 400: ValidationProblem with the Save messages and paths. A payment preview ignores the expense fields. Nothing is stored. Existing endpoints are unchanged.

## Test results

- `dotnet test` Api.Tests: 50 passed. Api.IntegrationTests: 27 passed. Scheduling.Tests: 19 passed.
- Frontend: lint, typecheck, 34 tests, build and size check (130.8 KB of 300 KB) pass. The new file has 7 tests, including axe with the popup open.

## Gaps and deviations

- `scripts/verify.ps1` stops at `npm audit --omit=dev --audit-level=high`: 4 high findings in picomatch, fdir, tinyglobby and vite, an existing dependency issue. No `package.json` or lock file was changed. The audit threshold was not lowered. Every other verify step was run separately and passes.
- `aidlc engine sensor-traceability` was blocked by this agent's permission rule, so `traceability.json` was written by hand in the format of earlier units (no `--write-upstream` run).
- The plan listed `getContract`-style types in `contracts.ts`; the preview types live in `types.ts` with the other API types.
- Expense popup uses the same columns as the detail tab (Month for Overtime, Date for All at once).
