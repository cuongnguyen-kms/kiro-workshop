# Logical Components: u1-schedule-balance

All components live in `src/ContractManagement.Scheduling`, one failure domain: the library runs inside the API process and has no state, so a failure is one exception on one request. Sources: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/functional-spec.md` and contract C2 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

| Component | Kind | Responsibility | NFR patterns applied |
|---|---|---|---|
| `BalanceRow` (Models.cs) | record | One month's period, expense, (Accrual)/Prepaid, cumulative balance and the four ST/LT columns. | `decimal` money (NFR3.1); immutable. |
| `IBalanceScheduleBuilder` | interface | C2 entry point `Build(payments, expenses)`. | Lets U2 call the builder for saved contracts. |
| `BalanceScheduleBuilder` | class | Input checks, monthly grouping, cumulative walk, look-ahead and ST/LT split. | Fail fast (SEC1); O(n log n) (SEC2, NFR1.1); pure (SEC3, NFR3.3). |
| `ScheduleGenerator` (existing) | class | After building payments and expenses, calls the builder and sets `ScheduleResult.Balance`. | Additive change (NFR2.1). |
| `ScheduleResult` (existing) | record | Gains `Balance` (list of `BalanceRow`). | Positional record: every caller adds the argument in the same change. |

Shared resources: none. No cache, pool, file, or connection. Blast radius of a defect: wrong or missing balance rows in previews and contract reads; payment and expense rows are not affected because they are built first and never changed by the builder.
