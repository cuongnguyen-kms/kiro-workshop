# Code Generation Plan: schedule preview

## Summary

- Builds: a server preview call (`POST /api/v1/contracts/preview`, stores nothing) and two buttons with schedule popups on the New contract form
- Touches: `src/ContractManagement.Api/` (service, endpoint), `web/src/` (API client, form, new preview dialog, new `components/ui/dialog.tsx`), tests in `tests/ContractManagement.Api.Tests`, `tests/ContractManagement.Api.IntegrationTests`, `web/src/test`
- Tests: about 15 (6 backend unit, 3 API integration, 7 frontend component)

Testing Contract: test-after, Minimal strategy, existing suites stay green. No new packages: the popup uses the already installed `radix-ui` Dialog.

No data-model change: the preview stores nothing, so no table, column or migration is added.

## Steps

- [x] **Step 1. Structure.** No new projects. New files: `src/ContractManagement.Api/Contracts/Preview.cs`, `web/src/components/ui/dialog.tsx`, `web/src/contracts/SchedulePreview.tsx`. Requirements FR1 to FR3.
- [x] **Step 2. Verify test runners.** Confirm the commands in `unit-test-instructions.md` run on the unchanged tree (existing tests pass).
- [x] **Step 3. Data model.** Not applicable: nothing is stored (FR2.2).
- [x] **Step 4. Data model tests.** Not applicable.
- [x] **Step 5. Repository.** Not applicable: the preview never calls the repository.
- [x] **Step 6. Repository tests.** Not applicable.
- [x] **Step 7. Business logic - implement.** In `ContractService`, add `Preview(CreateContractRequest? request, PreviewKind kind)` that reuses the create path without saving: the same `ContractValidator` checks except the name (FR2.4); for `kind = payment` it validates contract dates and streams only and calls the generator with expense method None (FR2.3); for `kind = expense` it validates dates, streams and expense. Then it applies the BR2.5 and BR3.5 rules, the `SizeEstimator` cap (FR2.7) and one `IScheduleGenerator.Generate` call (FR2.1). It returns payment rows with the stream's 1-based form position instead of a saved id (FR3.3), the expense rows, and both totals. Refactor `CreateAsync` only as far as needed to share these steps, keeping its behaviour unchanged.
- [x] **Step 8. Business logic - tests.** Add `tests/ContractManagement.Api.Tests/ContractPreviewServiceTests.cs`: preview equals create for the same input (NFR2), no repository call (FR2.2), invalid expense ignored for payment preview (FR2.3), blank name allowed (FR2.4), same message and path as create (FR2.5), row cap (FR2.7). Run them, then the whole unit test project.
- [x] **Step 9. API - implement.** In `ContractEndpoints`, map `POST /api/v1/contracts/preview?kind=payment|expense` (default `expense`), named `previewContractSchedule`, in the same route group so CORS, body limit, error handling and the future sign-in apply (NFR3). It returns 200 with `{ paymentSchedule: { entries: [{ streamNumber, dueDate, amount }], total }, expenseSchedule: { entries: [{ periodStart, periodEnd, amount }], total } }` or 400 ValidationProblem. Add the shapes in `Contracts/Preview.cs`. Existing endpoints do not change (NFR5).
- [x] **Step 10. API - tests.** Add `tests/ContractManagement.Api.IntegrationTests/PreviewApiTests.cs`: 200 with both schedules and no stored contract (FR2.1, FR2.2), 400 for invalid input (FR2.5), 10 streams over 10 years within 2 s after a warm-up (NFR1). Update the existing OpenAPI test so it expects the new path. Run them, then the whole integration project.
- [x] **Step 11. Frontend - implement.**
  - `web/src/api/contracts.ts`: add `previewSchedule(request, kind)` and its response types.
  - `web/src/components/ui/dialog.tsx`: the shadcn Dialog on the installed `radix-ui` package.
  - `web/src/contracts/validation.ts`: add `canPreviewPayments(values)` and `canPreviewExpense(values)` built from the existing client rules (FR1.2, FR1.3), plus the reason text for a disabled button (FR1.5).
  - `web/src/contracts/SchedulePreview.tsx`: the two buttons with `data-testid` (`preview-payments`, `preview-expense`), disabled reasons linked by `aria-describedby`, the None hint "This contract has no expense schedule." (FR1.4). Each opens a dialog that calls `previewSchedule` with the form values at that moment (FR3.6), shows loading (FR3.4), then the existing `ScheduleTable` with the same columns and total as the detail tabs (FR3.1, FR3.2), "Stream N" labels (FR3.3), server messages (FR2.5) or the retry message with a Retry button (FR2.6). Close button and Escape, focus handling from Radix (FR3.5).
  - `web/src/contracts/ContractForm.tsx`: place `SchedulePreview` beside Save, fed by `watch()` so the buttons update as the user types (FR1.1, FR1.6). The form state is never changed by the preview (FR3.7).
  - `web/src/test/fakeApi.ts`: add `previewSchedule`.
- [x] **Step 12. Frontend - tests.** Add `web/src/test/schedulePreview.test.tsx` with the seven tests listed in `unit-test-instructions.md`, including axe with the dialog open (NFR4). Run it, then the whole frontend suite.
- [x] **Step 13. Build configuration.** No change. `scripts/verify.ps1` already runs every project and the frontend steps; run it once at the end.
- [x] **Step 14. Documentation and traceability.** XML summaries on the new public types, `code-summary.md`, `traceability.json` (one existing file per OK target) and the C1 contract note in `code-summary.md` for the new endpoint.

## Story and requirement traceability

| Requirement | Steps |
|---|---|
| FR1.1 to FR1.6 | 11, 12 |
| FR2.1 to FR2.4, FR2.7 | 7, 8, 9, 10 |
| FR2.5, FR2.6 | 7 to 12 |
| FR3.1 to FR3.7 | 11, 12 |
| NFR1 | 10 |
| NFR2 | 8 |
| NFR3, NFR5 | 9, 10 |
| NFR4 | 12 |

## Testing Contract

```json
{
  "version": 1,
  "methodology": "test-after",
  "source": "org",
  "ordering": "implement each applicable testable layer, then write and run that layer's tests.",
  "scope": "express",
  "test_strategy": "minimal",
  "project_type": "brownfield",
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
  "input_sha256": "sha256:93cc30bfac56d027325cddc54558e948a7b3c5795d8b95fdf54be9cef18eb687",
  "contract_sha256": "sha256:55ed9576f1182de0615d887392d7d201fa1b6dc3599767f7260b15939c91944b"
}
```
