# Story to Unit Map

| Story | Unit IDs | Directories | What each unit does for it |
|---|---|---|---|
| US1.1 | U2; U3 | u2-contract-api; u3-contract-web | U2 validates name and dates; U3 shows the Details section and messages |
| US2.1 | U2; U3 | u2-contract-api; u3-contract-web | U2 validates streams; U3 shows stream cards with add and remove |
| US3.1 | U2; U3 | u2-contract-api; u3-contract-web | U2 validates expense dates; U3 shows the method-specific date fields |
| US4.1 | U1; U2 | u1-schedule-engine; u2-contract-api | U1 generates payment dates; U2 calls it on save and stores the rows |
| US5.1 | U1; U2 | u1-schedule-engine; u2-contract-api | U1 generates the expense schedule; U2 calls it on save and stores the rows |
| US6.1 | U2; U3 | u2-contract-api; u3-contract-web | U2 saves in one transaction; U3 shows the saved schedules or the failure alert |
| US6.2 | U2; U3 | u2-contract-api; u3-contract-web | U2 returns the contract with schedules or not found; U3 shows the detail screen |
| US6.3 | U2; U3 | u2-contract-api; u3-contract-web | U2 lists contracts; U3 shows the list and empty state |

## Cross-cutting Rows

- US1.1, US2.1, US3.1: validation messages must be identical in U2 and U3 (Refined Mockups interaction spec).
- US4.1, US5.1, US6.1: totals must match exactly from U1 through U2's storage (NFR4).

## Implementation Order Within Each Unit

- **U1:** US4.1 (payment dates), then US5.1 (expense schedule, which needs the payment total).
- **U2:** US1.1, US2.1 and US3.1 validation, then US4.1 and US5.1 integration with U1, then US6.1 save, then US6.2 and US6.3 reads.
- **U3:** US1.1, US2.1 and US3.1 form sections, then US6.1 save flow, then US6.2 detail and US6.3 list.

## Coverage Verification

- All 8 stories are assigned to at least one unit.
- Every unit has rows: U1 has 2, U2 has 8, U3 has 6.
