# Code Generation Plan: u3-web-balance

## Summary

- Builds: the bracket money format with thousands separators, and the 8-column expense balance table (no total row) on the Expense schedule tab and in the preview popup, which opens up to 80% wide.
- Touches: web/src/api/types.ts, web/src/contracts/display.ts, new web/src/contracts/expenseBalance.ts, web/src/components/ScheduleTable.tsx, web/src/contracts/ScheduleTabs.tsx, web/src/contracts/SchedulePreview.tsx, web/src/test/fakeApi.ts and the existing web tests.
- Tests: about 30 new Vitest tests (formatter, period, table, tab, popup, axe, timing), plus updates to 4 existing test files for the new format and table.

Methodology: test-after (team.md). This Unit has only the frontend layer: no data model, repository or endpoint change (those are U1 and U2). Tests use the fake API, so U3 builds and passes without U2.

Inputs: `construction/u3-web-balance/functional-design/` (functional-spec.md, frontend-components.md), `construction/u3-web-balance/nfr-design/` (security-design.md, performance-design.md), `construction/u3-web-balance/infrastructure-design/infrastructure-specification.md`, contract C1 v1.1.0 in `inception/contract-design/contract-summary.md`, U3 in `inception/units-generation/unit-of-work.md`, `inception/requirements-analysis/requirements.md`, and the refined mockups.

## Steps

- [x] **Step 1. Verify the test runner.** In `web/`, run `npx vitest --run src/test/contractDetail.test.tsx` on the current code and confirm it passes. (Runner readiness.)
- [x] **Step 2. API types** in `web/src/api/types.ts`: new `BalanceRow` with `periodStart`, `periodEnd`, `expense`, `accrualPrepaid`, `cumulativeBalance`, `stAccruedLiability`, `ltAccruedLiability`, `stPrepaidAsset`, `ltPrepaidAsset` (C1 v1.1.0); new `ExpenseSchedule` = `Schedule<ExpenseEntry>` plus `balanceRows: BalanceRow[]`, used by the contract detail and `SchedulePreview` types (NFR2.1). Stories: US1.1, US2.1, US2.2.
- [x] **Step 3. Display helpers** in `web/src/contracts/display.ts` (US3.1, US1.1):
  - `formatMoney`: non-finite, null or undefined gives an empty string (SEC2); absolute value through `toFixed(10)`; half away from zero on the digit string with carry; `0.00` for any zero result (no sign, AC3.1.3); comma groups of 3 (AC3.1.2); brackets for negatives (AC3.1.1); no locale (NFR3.1). Update the file's top comment: helpers format only, they never add or split money.
  - New `formatPeriod(date)`: `Jan 2027` from `YYYY-MM-DD` with fixed English month names; a malformed value is returned unchanged.
- [x] **Step 4. Shared expense columns and rows** in a new file `web/src/contracts/expenseBalance.ts`: `EXPENSE_BALANCE_COLUMNS` with the exact headers `Period`, `Expense`, `(Accrual)/Prepaid`, `Cumulative Balance`, `ST Accrued Liability`, `LT Accrued Liability`, `ST Prepaid Asset`, `LT Prepaid Asset` (Period left, others right-aligned); pure `expenseBalanceRows(rows)` that formats each row and treats a missing or non-array input as empty (FR2, FR2.6, AC2.1.4, SEC2).
- [x] **Step 5. `ScheduleTable`** in `web/src/components/ScheduleTable.tsx` (US2.3, US4.1):
  - `total` becomes optional; no `tfoot` when it is absent (AC2.3.1, AC2.3.2).
  - New optional props `rowHeader` (first body cell as `th scope="row"`, with the same padding as `td`), `stickyFirstColumn`, `regionLabel` (defaults to the caption) and `maxHeightClass`.
  - Right-align every right-aligned column with tabular figures through table-level classes (all body cells after the first when `rowHeader` is set; the last column otherwise, as today), so rows keep no per-cell style attributes (NFR1.2).
  - Headers `whitespace-nowrap`; sticky header row (`top-0`) and sticky first column (`left-0`) with background colour, a right border on the sticky column, and z-order top-left cell above header above column cells (NFR4.3).
  - The region becomes the scroll container (`overflow-auto` plus the height limit when given) with `role="region"`, `tabIndex={0}` and `aria-label` from `regionLabel`; the pager moves below the region so it stays in view (NFR4.2, NFR4.3).
  - Defaults keep the payment table's look and test ids unchanged.
- [x] **Step 6. Expense tab** in `web/src/contracts/ScheduleTabs.tsx`: the expense table uses `EXPENSE_BALANCE_COLUMNS` and `expenseBalanceRows(expenseSchedule.balanceRows)` in `useMemo`, no total, `rowHeader`, `stickyFirstColumn`, `regionLabel` "Expense schedule, scrolls sideways", height limit `max-h-[70vh]`, and no `emptyMessage` (so headers show for an empty list). Method None message unchanged. Remove the now unused Overtime and All at once column lists (US1.1, US2.1, US2.2, US2.3).
- [x] **Step 7. Expense popup** in `web/src/contracts/SchedulePreview.tsx`: the expense table uses the same columns and row builder from `preview.expenseSchedule.balanceRows`, no total, `regionLabel` "Expense schedule preview, scrolls sideways", height limit `max-h-[60vh]`; `DialogContent` gets `sm:max-w-[80vw]` only for the expense kind; the payment popup is unchanged (US2.1, US2.3, M2).
- [x] **Step 8. Fake API** in `web/src/test/fakeApi.ts`: contract detail and preview responses carry `balanceRows` (the C1 example rows for an Overtime contract; one row for the existing All at once contract; `[]` for method None).
- [x] **Step 9. Frontend tests (after implementation):**
  - New `web/src/test/display.test.ts`: the interaction-spec table (1000, 1234567.5, -1000, -0.004, 0), -0, 1.005, -1.005, 999.995, 999.999, 5e-7, 1e15, NaN, null; `formatPeriod` for January, December, a leap-year February and a malformed value (NFR3.1, AC3.1.1 to AC3.1.3).
  - New `web/src/test/expenseBalance.test.tsx`: tab shows the 8 headers in order; rows and values match the fake API formatted (AC1.1.1 to AC1.1.3, AC2.1.1 to AC2.1.3, AC2.2.1 to AC2.2.6, NFR3.2); no `tfoot` and no total test id (AC2.3.1); Period cells are row headers; region role, focusability and label differ from the caption (NFR4.2); sticky and nowrap classes present (NFR4.3); missing `balanceRows` shows headers and no rows (SEC2); method None message (AC1.1.4); popup shows the same table and values as the tab for the same data (AC2.1.4), no total (AC2.3.2), and has the 80% width class; axe on tab and popup finds no serious or critical issues (NFR4.1); 120 rows render under 200 ms in jsdom, first page shown (NFR1.1).
  - Update existing tests for the new format and table: `contractDetail.test.tsx` (stream amount `1,000.00`, payment row and total `12,000.00`, the expense table now the 8-column balance table with no total), `schedulePreview.test.tsx` (payment total `1,250.50`; expense popup rows from `balanceRows`, no total), `schedule.perf.test.tsx` (totals `52,000.00` and `31,375.00`), and `accessibility.test.tsx` if it asserts expense columns. Form input values (`1000.00`) do not use `formatMoney` and stay as they are (AC3.1.4).
- [x] **Step 10. Run the Unit's tests, lint, typecheck and build** with the commands in `unit-test-instructions.md`, then `npm run lint`, `npm run typecheck`, `npm run build` and `npm run size` in `web/`. All pass with zero warnings.
- [x] **Step 11. Documentation and traceability.** TSDoc on the new exports; `code-summary.md` with a line for the person's by-eye browser check (sticky column, sticky header, keyboard scrolling, popup width), `traceability.json` (one existing file per target) and `source-manifest.json`.

Test configuration: no change. The existing Vitest setup (`web/src/test/setup.ts`, jsdom, vitest-axe) is used.

## Story traceability

| Story | Steps |
|---|---|
| US1.1 (monthly rows, Period format, method None) | 2, 3, 4, 6, 9 |
| US2.1 ((Accrual)/Prepaid and balance on tab and popup) | 2, 4, 6, 7, 9 |
| US2.2 (ST and LT columns) | 2, 4, 6, 9 |
| US2.3 (no total row) | 5, 6, 7, 9 |
| US3.1 (brackets, commas, 0.00, payment total kept) | 3, 9 |
| US4.1 (scrolling, sticky, axe, speed) | 5, 6, 7, 9 |

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
