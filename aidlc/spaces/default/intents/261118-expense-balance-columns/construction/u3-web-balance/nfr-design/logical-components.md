# Logical Components: u3-web-balance

All components run in the browser bundle built from `web/`. Sources: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md` and contract C1 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

| Component | File | Responsibility | NFR patterns applied |
|---|---|---|---|
| API types | `web/src/api/types.ts` | `BalanceRow` and `ExpenseSchedule` (C1 v1.1.0) | Additive types (NFR2.1) |
| Display helpers | `web/src/contracts/display.ts` | `formatMoney` (brackets, commas, exact rounding), `formatPeriod` | Exact decimal-string rounding (NFR3.1); no locale |
| Expense columns and rows | `web/src/contracts/expenseBalance.ts` (new) | `EXPENSE_BALANCE_COLUMNS`, `expenseBalanceRows` shared by tab and popup | One definition (AC2.1.4); pure, memoised (NFR1.2) |
| Schedule table | `web/src/components/ScheduleTable.tsx` | Optional total, row headers, sticky header and column, region label, pager outside the scroll area | NFR4.1 to NFR4.3 |
| Schedule tabs | `web/src/contracts/ScheduleTabs.tsx` | Expense tab with the balance table | — |
| Schedule preview | `web/src/contracts/SchedulePreview.tsx` | Expense popup with the balance table, 80% width | — |
| Fake API | `web/src/test/fakeApi.ts` | `balanceRows` in fake responses | U3 builds without U2 |

Failure domain: the browser tab. A defect shows wrong or blank cells in one table; it cannot change stored data because U3 makes no new API call. Shared resources: none.
