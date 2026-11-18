# Functional Specification: u3-web-balance

Stories this Unit implements (see `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/user-stories/stories.md`): US1.1, US2.1, US2.2, US2.3, US3.1, US4.1. Unit definition: U3 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/units-generation/unit-of-work.md`, with its rows in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/units-generation/unit-of-work-story-map.md`. Component: the web client in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/domain-design/components.md`. Contract: C1 v1.1.0 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`. Requirements: `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/requirements-analysis/requirements.md`. Layout and wording come from the approved refined mockups (M1 to M4, interaction spec and accessibility checklist).

U3 is a `ui` Unit: it writes no entities or rules. The browser never calculates money (FR4.1, ADR-003); it formats and shows what the API returns. This file is self-contained for the Unit's workflows and screen states; `frontend-components.md` holds the component design.

## Data the Unit reads (contract C1 v1.1.0)

`expenseSchedule` in both the contract detail response and the preview response gains `balanceRows`: one row per month with `periodStart`, `periodEnd`, `expense`, `accrualPrepaid`, `cumulativeBalance`, `stAccruedLiability`, `ltAccruedLiability`, `stPrepaidAsset`, `ltPrepaidAsset`. `entries` and `total` stay in the response but the expense table no longer shows them. `balanceRows` is empty for method None.

## Workflow 1. Format an amount (every screen) - US3.1

Input: a number from the API.

1. Round to 2 decimals (half away from zero).
2. IF the rounded value is 0 (including -0 and values such as -0.004) THEN show `0.00` (AC3.1.3).
3. Write the absolute value with exactly 2 decimals, a full stop as the decimal point and a comma every 3 digits to the left of it (AC3.1.2). The result does not depend on the browser's language.
4. IF the value is negative THEN wrap it in brackets, with no minus sign: `(1,000.00)` (AC3.1.1). Same text colour as other amounts.

| Input | Output |
|---|---|
| 1000 | 1,000.00 |
| 1234567.5 | 1,234,567.50 |
| -1000 | (1,000.00) |
| -0.004 | 0.00 |
| 0 | 0.00 |
| 999.999 | 1,000.00 |

Every amount in the app goes through this one formatter: payment rows and total, expense table amounts, stream amounts in the form summary and the detail screen's stream list (FR3.1, M4).

## Workflow 2. Format a period - US1.1

1. Take the year and month of `periodStart`.
2. Show the English three-letter month and the year, for example `Jan 2027`, for every row: expense months, payment-only months and the All at once month (mockups Q4). The result does not depend on the browser's language.

## Workflow 3. Show the Expense schedule tab (M1) - US1.1, US2.1, US2.2, US2.3

1. IF the contract's expense method is None THEN show "This contract has no expense schedule." and no table (AC1.1.4, unchanged).
2. ELSE build one display row per `balanceRows` item, in the order the API sent them (AC1.1.1 to AC1.1.3): Period (Workflow 2), then the seven amounts through Workflow 1.
3. Show the table with the eight columns in this order and with these exact headers: `Period`, `Expense`, `(Accrual)/Prepaid`, `Cumulative Balance`, `ST Accrued Liability`, `LT Accrued Liability`, `ST Prepaid Asset`, `LT Prepaid Asset` (FR2).
4. Show no total row (AC2.3.1).

The values shown are exactly the API values (AC2.1.1 to AC2.1.3, AC2.2.1 to AC2.2.6); U3 adds no check of its own on them.

## Workflow 4. Show the Expense schedule preview popup (M2) - US2.1, US2.3

1. Opening, loading, validation errors, failure and retry stay as today.
2. On success, build the rows from `preview.expenseSchedule.balanceRows` with the same row builder as Workflow 3, and show the same table with the same columns (AC2.1.4, FR2.6).
3. Show no total row (AC2.3.2).
4. The Expense schedule popup is up to 80% of the viewport width. The Payment schedule popup keeps its current width.

## Workflow 5. Payment schedule (tab and popup) - US3.1

Unchanged columns (Due date, Stream, Amount) and total row; the amounts and the total use Workflow 1 (AC3.1.4).

## Screen states

| State | Expense tab (M1) | Expense popup (M2) |
|---|---|---|
| Loading | unchanged detail skeleton | unchanged "Loading the preview…" |
| Method None | unchanged message, no table | button disabled with its hint, unchanged |
| Error | unchanged | unchanged message and Retry |
| Rows present | 8-column table, no total row | same table, no total row, popup up to 80% wide |
| More than 100 rows | the existing pager (100 rows a page) | the same pager |

## Layout and accessibility - US4.1

- Headers stay on one line. The table sits in a container that scrolls sideways when it is wider than the space (AC4.1.2).
- The Period column stays fixed at the left with its own background; the header row stays at the top when the container scrolls down.
- Period cells are row headers, so each amount is announced with its month; every column has a column header (AC4.1.3).
- The scroll container is focusable, has `role="region"` and the accessible name "Expense schedule, scrolls sideways" (tab) or "Expense schedule preview, scrolls sideways" (popup), and shows the existing focus ring.
- No serious or critical axe issues on the tab and in the popup, checked with vitest-axe in jsdom (team.md accepts that real-browser checks are not run).
- Rendering stays quick: rows are built once per data change, with no per-row components, and long schedules use the existing pager (AC4.1.1, together with U2's 2-second API target).

AC4.1.4 (saved contracts need no migration) is delivered by U2: U3 only reads `balanceRows`.

## Tests (for Code Generation)

- Formatter: the table above, plus large and small positives and negatives.
- Period formatter: January, December, a leap-year February.
- Tab: 8 headers in order; rows and values from the fake API; no total row; method None message; Period cells are row headers; region name; axe.
- Popup: the same table and values as the tab for the same data; no total row; the 80% width class; axe.
- Payment schedule: total row kept, amounts in the new format.
- Existing tests that expect `toFixed` strings, the two-column expense table or the expense total row are updated.
