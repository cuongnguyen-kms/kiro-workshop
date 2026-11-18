# Tech Stack Decisions: u1-schedule-balance

No new technology. U1 stays on the stack recorded in `aidlc/spaces/default/codekb/Kiro-Workshop/technology-stack.md`, and the design in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/functional-spec.md` and `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/rules.md` needs nothing beyond it. Requirements: `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/requirements-analysis/requirements.md`; contract C2: `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

| Area | Choice | Reason |
|---|---|---|
| Language and runtime | C# on .NET 10, nullable on, warnings as errors | Existing `ContractManagement.Scheduling` project. |
| Money type | `decimal` | Exact cents; matches `numeric(18,2)` storage (NFR3.1). |
| Date type | `DateOnly` | Already used by the payment and expense rows; calendar-month arithmetic for the look-ahead uses `AddMonths` on the month start (BR2.3). |
| New types | `BalanceRow` record, `IBalanceScheduleBuilder` interface and its implementation in the Scheduling project | Contract C2; ADR-001 keeps the rules in this library. |
| Tests | xUnit in `tests/ContractManagement.Scheduling.Tests` | Existing test project; test-after per `team.md`. |
| Packages | None added | No need; keeps the 300 KB web limit and the dependency surface unchanged. |

## Alternatives considered

- **Put the builder in the API (`ContractService`)**: rejected in ADR-001; the preview and saved-contract paths would duplicate it.
- **A new class library for balances**: rejected; one small type set does not justify a new project.
