# Security, Compatibility, Correctness and Accessibility Requirements: u3-web-balance

Sources: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/requirements-analysis/requirements.md` (NFR2 to NFR4), contract C1 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`, and `aidlc/spaces/default/codekb/Kiro-Workshop/technology-stack.md`.

## Threat considerations

U3 changes how existing API data is shown. It adds no input field, no new API call, no storage and no authentication change.

| Concern | Applies? | Requirement |
|---|---|---|
| Injection into the page | Low | Amounts and dates are rendered as React text only; no `dangerouslySetInnerHTML` or `innerHTML` (ESLint rule already bans them) (SEC1). |
| Unexpected API data | Low | A missing `balanceRows`, a non-number amount or a malformed date must not crash the screen (SEC2). |
| Data exposure | No | Only figures the API already returns to this user are shown. No browser storage (ESLint rule). |

| ID | Requirement | Pass/fail check |
|---|---|---|
| SEC1 | All new values are rendered as text nodes. | ESLint with `--max-warnings 0` passes. |
| SEC2 | A response without `balanceRows` is shown as an empty expense table with the eight headers and no rows; a non-finite amount shows as an empty cell rather than throwing. | Component test with a response missing `balanceRows`; formatter test with `NaN`. |

## NFR rows for this Unit

| ID | Derived from | Requirement | Pass/fail check |
|---|---|---|---|
| NFR2.1 | NFR2 | The client accepts C1 v1.1.0 additively: it reads `balanceRows` and ignores `entries` and `total` for the expense table; the payment schedule and the other fields are read as before. | Existing tests for detail, list and preview pass after updating only the expected amount strings and the expense table. |
| NFR3.1 | NFR3 | The money formatter is deterministic and exact to the cent: it rounds by decimal string arithmetic, not floating-point multiplication, so 1.005 shows 1.01 and -1.005 shows (1.01); results never depend on the browser language. | Formatter unit tests cover the interaction-spec table, 1.005, -1.005, 999.999, -0.004, 0, -0 and NaN. |
| NFR3.2 | NFR3 | The client never adds, splits or recalculates money; it shows the API values (FR4.1). | Code review; the tab test asserts the cells equal the fake API values formatted. |
| NFR4.1 | NFR4 | The expense table on the tab and in the popup has no serious or critical axe issues; every column has a `th scope="col"` header with the exact label; Period cells are `th scope="row"`. | vitest-axe in jsdom on the tab and the popup; header and row-header assertions. |
| NFR4.2 | NFR4 | The table's scroll region is keyboard focusable (`tabindex="0"`), has `role="region"` and an accessible name that differs from the hidden caption ("Expense schedule, scrolls sideways" versus the caption "Expense schedule"), and shows a visible focus ring. | Component test on the region's attributes. |
| NFR4.3 | NFR4 | On a narrow screen the table scrolls sideways inside its container; headers stay on one line; the Period column stays in view; the header row stays in view when the region scrolls down. | Class assertions in tests; the person checks by eye in a browser (team.md: real-browser checks are not automated). |
