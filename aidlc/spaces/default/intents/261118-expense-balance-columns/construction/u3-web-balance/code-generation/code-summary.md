# Code Summary: u3-web-balance

## What was built

- **Money format.** `formatMoney` now gives comma thousands separators, brackets for negatives (`(1,000.00)`), `0.00` for any zero result (including -0 and -0.004), and an empty string for null, undefined, NaN or infinite input. It rounds half away from zero on the `toFixed(10)` digit string, so no float is multiplied, and it does not depend on the browser's language. Every amount in the app goes through it (payment rows and total, expense table, stream list and form summary).
- **Period format.** New `formatPeriod` shows `Jan 2027` from `YYYY-MM-DD` with fixed English month names; a malformed value comes back unchanged.
- **Balance types.** New `BalanceRow` and `ExpenseSchedule` (`Schedule<ExpenseEntry>` plus `balanceRows`) in the API types, used by the contract detail and the preview.
- **Expense balance table.** New `web/src/contracts/expenseBalance.ts` holds `EXPENSE_BALANCE_COLUMNS` (the 8 exact headers) and `expenseBalanceRows`, shared by the Expense schedule tab and the preview popup. A missing or non-array `balanceRows` gives no rows.
- **`ScheduleTable`.** `total` is optional (no `tfoot` without it). New props `rowHeader`, `stickyFirstColumn`, `regionLabel` and `maxHeightClass`. The scroll region is now the `overflow-auto` focusable element with `role="region"`; the pager sits below it. Header cells are `whitespace-nowrap` and sticky at the top; the first column is sticky at the left with a background and a right border. Alignment and sticky styling come from table-level classes, so rows carry no per-cell attributes. Payment table look, columns, total row and test ids are unchanged.
- **Tab and popup.** The Expense tab and popup show the 8-column table with no total row, region names "Expense schedule, scrolls sideways" and "Expense schedule preview, scrolls sideways", and height limits `max-h-[70vh]` and `max-h-[60vh]`. The expense popup is `sm:max-w-[80vw]`; the payment popup keeps its width. The unused Overtime and All at once column lists were removed.
- **Fake API.** Contract detail and preview responses carry `balanceRows` (C1 example rows for an Overtime contract, one row for the All at once sample, `[]` for None and for a new saved contract).

## Files changed

Created:
- `web/src/contracts/expenseBalance.ts`
- `web/src/test/display.test.ts`
- `web/src/test/expenseBalance.test.tsx`

Modified:
- `web/src/api/types.ts`
- `web/src/contracts/display.ts`
- `web/src/components/ScheduleTable.tsx`
- `web/src/contracts/ScheduleTabs.tsx`
- `web/src/contracts/SchedulePreview.tsx`
- `web/src/test/fakeApi.ts`
- `web/src/test/contractDetail.test.tsx`
- `web/src/test/schedulePreview.test.tsx`
- `web/src/test/schedule.perf.test.tsx`

Deleted: none. `accessibility.test.tsx` needed no change (it does not assert expense columns).

## Tests

- New: `display.test.ts` (19 tests for the formatter and period), `expenseBalance.test.tsx` (15 tests: 11 for the tab, 4 for the popup, covering headers, values, no total row, row headers, region, sticky and nowrap classes, missing `balanceRows`, method None, popup parity with the tab, 80% width, axe on both, 120 rows under 200 ms).
- Updated for the new format and table: `contractDetail.test.tsx`, `schedulePreview.test.tsx`, `schedule.perf.test.tsx`. No check was weakened; the expense total assertions were replaced by no-total-row assertions because the design removes that row.

## Results

| Command (in `web/`) | Result |
|---|---|
| `npm.cmd exec -- vitest --run` on the 6 Unit test files | 6 files, 49 tests passed |
| `npm.cmd test -- --run` (full suite) | 9 files, 68 tests passed |
| `npm.cmd run lint` (max-warnings 0) | passed, no warnings |
| `npm.cmd run typecheck` | passed |
| `npm.cmd run build` | passed |
| `npm.cmd run size` | passed, 131.1 KB gzip of 300 KB |

## Deviations

- The Unit test command with `npm.cmd --prefix web exec -- vitest ...` run from the project root could not resolve the `@/` alias (all 6 files failed to load, no test ran). The same vitest command was run from `web/` instead and passed.
- `SchedulePreview` no longer needs the expense method in `PreviewBody`, so its `method` prop was removed.
- The popup width class is chosen from the kind of the last opened preview (kept in the existing request ref) so the width does not flip during the close animation.

## Browser check

Browser check by the person: pending (sticky Period column, sticky header row, sideways and keyboard scrolling, popup width). jsdom cannot show these.
