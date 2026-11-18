## Review

**Verdict:** READY
**Reviewer:** aidlc-architecture-reviewer-agent
**Date:** 2026-11-18T04:16:51Z
**Iteration:** 1
**Verdict Dictated:** no

### Findings

| ID | Severity | Location | Finding | Required action | Status |
|---|---|---|---|---|---|
| R-01 | Major | aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md > C2 rules owned by U1 (look-ahead) | The look-ahead target is "period end plus 12 calendar months", but the rule does not say what happens when the period end is the last day of a month and the target month is shorter or longer. The row for Feb 2027 ends 2027-02-28, so the target is 2028-02-28. Under the leap year the Feb 2028 row ends 2028-02-29 and is therefore after the target, so the Jan 2028 balance is used instead of Feb 2028. This contradicts the requirement's intent (end of January looks at end of January) and changes ST and LT values. | State the date rule: when the period end is the month's last day, the target is the last day of the month 12 months later. Add a leap-year case to the examples or to U1's tests. | New |
| R-02 | Major | aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md > C1 BalanceRow periodStart and periodEnd | The period rules cover only Overtime start and end dates. For All at once, requirement FR1.2 gives the expense date's month, but the contract does not say whether periodStart and periodEnd are the whole month or the expense date. Because periodEnd feeds the 12-month look-ahead, the choice changes ST and LT. The same gap exists for a month with a payment before an Overtime start. | Say explicitly what periodStart and periodEnd are for All at once and for payment-only months, and add an All at once example row. | New |
| R-03 | Minor | src/ContractManagement.Scheduling/Models.cs > ScheduleResult; src/ContractManagement.Api/Contracts/Responses.cs > ExpenseScheduleResponse | Both records are positional. Adding Balance and BalanceRows as new constructor parameters breaks every existing caller, including tests and fakes, at compile time. The wire change is additive (NFR2), but the C# change is source-breaking inside the solution. ContractMapper.ToDetail also takes only a Contract, so it needs a way to get balance rows for a saved contract (recomputing through the builder from stored entries). | Note in C2 that existing constructor calls and tests are updated in the same change, and state that saved contracts get balance rows by calling Build on the stored entries at read time. | New |
| R-04 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md > C1 example JSON | The example numbers match the rules: Jan is 12,000.00 minus 1,000.00 = 11,000.00; Feb is 0 minus 1,000.00 = (1,000.00), cumulative 10,000.00; the schedule ends in Dec 2027, before the 12-month target, so the whole balance is ST. The example therefore never exercises the ST/LT split or the accrual columns, and the headline FR2.3 case is untested at contract level. | Add a second example with an ST/LT split (for example the 12,000 / 3,000 case) and one accrual row. | New |

### Validation Tool Results

| Tool | Result | Interpretation |
|---|---|---|
| none listed | not run | The unit DAG (U2 to U1 is C2, U3 to U2 is C1) matches the two contracts, and NFR2 additive-within-v1 is respected on the wire. |

### Summary

The contracts match the unit DAG and the additive-within-v1 requirement, and the example numbers are correct. Two gaps in the period-end and look-ahead date rules can change ST and LT values and should be settled before U1's design.
