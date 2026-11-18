# Security and Quality Requirements: u1-schedule-balance

U1 is a library change inside `src/ContractManagement.Scheduling`. It adds the balance builder (contract C2) and the `Balance` list on `ScheduleResult`. It has no endpoint, no storage, no user input of its own and no network or file access. Its callers (U2, the API) already validate requests and authorise nothing beyond what they do today. Performance, scalability, reliability and observability files are not produced for this Unit's kind (library); the targets that touch this Unit are carried in the table below.

Inputs read: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/functional-spec.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/rules.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/requirements-analysis/requirements.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`, `aidlc/spaces/default/codekb/Kiro-Workshop/technology-stack.md`.

## Threat considerations

| Concern | Applies? | Requirement |
|---|---|---|
| Authentication and authorisation | No | The library has no entry point a user reaches. Access control stays in the API (unchanged). |
| Untrusted input | Indirect | The rows come from the generator or from stored rows the API already validated. The builder still checks its own preconditions and fails fast (SEC1). |
| Data exposure | No | Balance rows are derived from amounts the response already returns. No new personal or secret data. |
| Resource exhaustion | Low | Row counts are bounded by the existing caps (10 streams, 10 years, 10,000 rows). The builder is linear in the row count after sorting (SEC2). |
| I/O and side effects | No | The Scheduling project's banned-symbols analyzer forbids I/O. The builder is a pure function (SEC3). |

## Requirements

| ID | Requirement | Pass/fail check |
|---|---|---|
| SEC1 | `Build` throws `ArgumentNullException` for a null list and `ArgumentException` when the payment total and the expense total differ (BR in `rules.md`). It never returns partial rows. | Unit tests for null input and for unequal totals. |
| SEC2 | Work is O(n log n) in the number of payment and expense rows; no nested scans over all rows per month. | Code review of the builder; NFR1.1 timing test. |
| SEC3 | No file, network, clock or random access in the builder; same input, same output. | Build passes with the banned-symbols analyzer and `-warnaserror`. |

## NFR rows for this Unit

| ID | Derived from | Requirement | Pass/fail check |
|---|---|---|---|
| NFR1.1 | NFR1 | Building balance rows for 10 streams over 10 years (120 months, up to 10,000 input rows) takes under 50 ms on the developer machine, leaving the 2-second read and preview limits to U2. | A unit test builds the largest allowed input and asserts it completes under 50 ms. |
| NFR2.1 | NFR2 | The C2 change is additive: `ScheduleResult` gains `Balance`; the existing payment and expense lists keep their rows and meaning. No storage or schema is touched. | Existing Scheduling tests pass unchanged apart from the new constructor argument. |
| NFR3.1 | NFR3 | All money is `decimal`; no `double` or `float`. Amounts are exact to 2 decimals and ST plus LT equals the absolute balance on every row. | Unit tests assert exact `decimal` values and the ST plus LT equality on every row. |
| NFR3.2 | NFR3 | Unit tests cover the worked examples E1 to E10 in `functional-spec.md`: the FR2.3 split example, a sign change, a schedule ending within 12 months, payment-only months, All at once, and the look-ahead to a month without a row (BR2.3 calendar months, including the leap-year case). | Each example is one xUnit test; all pass. |
| NFR3.3 | NFR3 | Repeated calls with the same input return equal rows. | A test calls `Build` twice and compares. |

NFR4 (accessibility) does not apply to this Unit; it belongs to U3.
