# Security, Correctness and Accessibility Design: u3-web-balance

Designs how U3 meets `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-requirements/security-requirements.md` and `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-requirements/performance-requirements.md`, with the stack in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-requirements/tech-stack-decisions.md`, the workflows in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md`, and contract C1 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

## Rendering safety (SEC1)

All values are rendered as React text. No HTML strings, no `dangerouslySetInnerHTML`; the existing ESLint rules enforce this.

## Unexpected data (SEC2)

- `balanceRows` missing or not an array: treated as an empty list. The table shows its eight headers and no rows. This only happens with an API older than C1 v1.1.0; with U2 built, the field is always present. The method None case never reaches the table (the tab shows its message first).
- An amount that is `null`, `undefined`, `NaN` or infinite: the formatter returns an empty string, so the cell is blank and nothing throws.
- A malformed `periodStart`: the period formatter returns the raw value unchanged.

## Exact money format (NFR3.1, NFR3.2)

The formatter never multiplies floats. Design:

1. Reject non-finite input (empty string, as above).
2. Turn the absolute value into a plain decimal string with 10 fraction digits (`toFixed(10)`). This never uses exponent form below 1e21 (amounts are far below that) and removes float noise: 1.005, stored as 1.00499999…, becomes `1.0050000000`.
3. Round half away from zero on the digit string: look at the third fraction digit and carry into the integer part when needed (so 999.995 becomes 1000.00).
4. If the rounded result is all zeros, return `0.00` with no sign.
5. Group the integer digits with commas every 3 digits and join with a full stop and the 2 fraction digits.
6. Wrap a negative result in brackets.

So 1.005 shows 1.01 and -1.005 shows (1.01). The API sends at most 2 decimals, so rounding only matters for values with float noise; it is tested so the rule is fixed.

The client still never adds, splits or recalculates money (FR4.1): the formatter changes only how a value reads.

Tests: the interaction-spec table, 1.005, -1.005, 999.995, 999.999, -0.004, 0, -0, 5e-7, 1e15, `NaN`, `null`.

## Accessibility (NFR4.1 to NFR4.3)

- `ScheduleTable` renders the first cell of each body row as `th scope="row"` when `rowHeader` is set (expense tables); column headers stay `th scope="col"`.
- Scroll region: `role="region"`, `tabIndex={0}`, `aria-label` "Expense schedule, scrolls sideways" on the tab and "Expense schedule preview, scrolls sideways" in the popup; the hidden caption stays "Expense schedule" / "Expense schedule preview". The existing focus ring classes stay.
- Sticky header and column: the region gets a height limit (`max-h-[70vh]` on the tab, `max-h-[60vh]` in the popup) with `overflow: auto`, so it scrolls both ways and `position: sticky` works for the header row (`top: 0`) and the first column (`left: 0`). Sticky cells get the muted or background colour so scrolled cells do not show through; the top-left header cell sits above both. The pager sits below the region, outside the scrolled area, so Previous and Next stay in view.
- Headers use `whitespace-nowrap`.
- Popup: `DialogContent` for the expense kind gets `sm:max-w-[80vw]`; the payment kind keeps the default.
- axe runs on the tab and on the popup in jsdom. The person checks the sticky header, the sticky column and keyboard scrolling by eye in a browser at the end of U3, as team.md accepts.
