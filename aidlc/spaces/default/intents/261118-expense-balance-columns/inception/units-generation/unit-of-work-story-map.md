# Story to Unit Map: expense balance columns

| Story | Unit IDs | Directories | What each unit does for it |
|---|---|---|---|
| US1.1 | U1; U2; U3 | u1-schedule-balance; u2-api-balance; u3-web-balance | U1 builds the monthly rows; U2 returns them; U3 shows them and the method None state |
| US2.1 | U1; U2; U3 | u1-schedule-balance; u2-api-balance; u3-web-balance | U1 calculates (Accrual)/Prepaid and the balance; U2 returns the same rows for preview and saved contract; U3 shows them on the tab and in the popup |
| US2.2 | U1; U3 | u1-schedule-balance; u3-web-balance | U1 calculates the ST and LT split; U3 shows the four columns |
| US2.3 | U3 | u3-web-balance | U3 drops the expense total row on the tab and in the popup |
| US3.1 | U3 | u3-web-balance | U3 changes the shared money format and keeps the payment total row |
| US4.1 | U2; U3 | u2-api-balance; u3-web-balance | U2 keeps read and preview within 2 seconds and needs no migration; U3 makes the wide table scroll and pass axe |

## Cross-cutting rows

- US2.1: the preview and the saved contract must show the same values, so U1's one builder serves both paths through U2.
- US3.1: every amount on every screen goes through the one money formatter in U3.

## Implementation order within each unit

- **U1:** monthly rows (US1.1), then (Accrual)/Prepaid and balance (US2.1), then the ST and LT split (US2.2).
- **U2:** saved-contract read (US1.1, US2.1, US4.1), then preview (US2.1).
- **U3:** money format (US3.1), then the table component and the tab (US1.1, US2.1, US2.2, US2.3), then the popup (US2.1, US2.3), then accessibility and scrolling (US4.1).

## Coverage verification

- All 6 stories are assigned to at least one unit.
- Every unit has rows: U1 has 3, U2 has 3, U3 has 6.
