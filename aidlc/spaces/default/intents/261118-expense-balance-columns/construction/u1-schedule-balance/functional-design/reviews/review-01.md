## Review

**Verdict:** READY
**Reviewer:** aidlc-architecture-reviewer-agent
**Date:** 2026-11-18T04:43:47Z
**Iteration:** 1
**Verdict Dictated:** no

### Findings

| ID | Severity | Location | Finding | Required action | Status |
|---|---|---|---|---|---|
| R-01 | Major | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/rules.md > BR2.3 and functional-spec.md > E8; contract-summary.md > C2 "Rules owned by U1" | The look-ahead rule and contract C2 give different answers for February. C2 says the target is the row's period end plus 12 calendar months and the row used is the latest one whose period end is on or before it. For the February 2027 row, that target is 28 Feb 2028, and the February 2028 row ends on 29 Feb 2028, so C2 excludes it and uses January 2028. BR2.3 and E8 compare calendar months and use the February 2028 row. I recomputed E8 under both readings and they give different balances, so ST and LT differ for every February row in a year before a leap year. | Pick the calendar-month reading, which matches the requirements assumption (end of January looks at end of January) and is the one E8 shows. Then state in the design that it refines C2's period-end wording, and have the contract summary say so too. The unit tests and U2 must then follow one reading. | New |
| R-02 | Major | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/rules.md > BR2.3 (last sentence) and BR2.4 | The case where the next row is more than 12 months after the row has no rule text and no example. BR2.3 selects the latest row on or before the target, which is then the row itself, so L equals B and BR2.4 gives ST 0 and LT equal to B. For example, a payment in Jan 2027 and an All at once expense in Jun 2029 gives that result for the January row. The final sentence of BR2.3 ("uses its own later rows only... L is the last row's balance, which is 0") does not describe this case. C2 says "when no later row reaches that date the whole balance is ST", which a developer could read the opposite way. | Rewrite the last sentence of BR2.3 to say that the row itself counts as a candidate, and add a worked example for a gap longer than 12 months. State the expected ST and LT for that example, and keep C2's wording consistent. | New |
| R-03 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/rules.md > BR1.2 | An Overtime row is recognised by period start differing from period end. An Overtime range of a single day (start equals end) is therefore treated like an All at once month, and its period becomes the whole month instead of that day. In `ExpenseScheduleBuilder.cs`, All at once rows also have equal start and end, so the two cases cannot be told apart from the rows alone. | Say explicitly that a one-day Overtime range gets a whole-month period, and that this is accepted. Alternatively, define the period from the row's own dates in both cases. Add a unit-test example either way. | New |
| R-04 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/functional-spec.md > Worked examples; traceability.json > AC1.1.2, AC1.1.3 | Several rules and acceptance criteria that traceability.json marks OK have no worked example, although the examples are the unit tests. The missing ones are: BR1.2 narrowing for an Overtime range that starts or ends mid-month (E1 and E9 use whole months only); several payments summed in one month alongside an Overtime row (AC1.1.2); All at once with Quarterly payments (AC1.1.3, rows Jan, Apr, Jun, Jul, Oct); and the exact AC2.2.1 figures (12,000.00 and 3,000.00 give ST 9,000.00 and LT 3,000.00; E2 and E4 use other numbers). E1 to E7 and E9 recompute correctly against BR1 and BR2. E10 and AC1.1.4 also agree. | Add the missing examples so each acceptance criterion marked OK has a test case. | New |

### Validation Tool Results

| Tool | Result | Interpretation |
|---|---|---|
| none listed for this stage | not run | Examples E1 to E10 were recomputed by hand against BR1.1 to BR3.2. E1 to E7, E9 and E10 are correct. E8 is correct under BR2.3 but conflicts with C2 (first finding). |

### Summary

The rules agree with FR1 to FR4 and the stories, and the arithmetic in the examples holds. Two points need the person's attention before build: the calendar-month look-ahead differs from C2's period-end wording for leap-year Februaries, and the gap-longer-than-12-months case is undefined in the rule text.
