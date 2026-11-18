# Security and Quality Design: u1-schedule-balance

Designs how U1 meets the requirements in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/nfr-requirements/security-requirements.md`, using the stack in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/nfr-requirements/tech-stack-decisions.md`, the workflow in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/functional-spec.md`, and contract C2 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

U1 has no authentication, authorisation, encryption, secrets or network surface. The design is about input checks, determinism and bounded work.

## Input checks (SEC1)

`BalanceScheduleBuilder.Build(payments, expenses)` checks its input in this order and fails fast, before building any row:

1. A null list throws `ArgumentNullException` naming the parameter.
2. An empty expense list returns an empty list and does not compare totals (BR1.1, BR3.2). This covers method None and a payment-only preview.
3. When expense rows exist, the payment total must equal the expense total exactly (`decimal` equality). Otherwise `ArgumentException` with a message that gives both totals.

Decision for saved contracts (review finding on stored rows): stored rows were written by the generator with matching totals, so a mismatch on read means the stored data is damaged. U2 does not catch the exception. The API's existing error handler logs it and returns a 500 problem response, so damaged data is visible rather than shown with wrong balances. U2's functional design records this; U1 only guarantees the exception type and message.

## Determinism and exactness (NFR3.1, NFR3.3, SEC3)

- All arithmetic is `decimal`; no `double`, `float` or `Math.Round` with a midpoint other than the inputs' 2 decimals, so no rounding happens.
- Months come from `DateOnly` (year and month); the look-ahead target is the month start plus 12 calendar months (`AddMonths(12)`), never a day count (BR2.3).
- The builder reads no clock, random source, culture or environment. The Scheduling project's banned-symbols analyzer and `-warnaserror` enforce no I/O.
- ST and LT are computed from absolute values by BR2.4, so both are never negative and add up to the absolute balance.

## Bounded work (SEC2, NFR1.1)

- Group payments and expenses by month into a sorted dictionary: O(n log n).
- Walk the months once, carrying the cumulative balance.
- The look-ahead for each month uses a binary search over the sorted month keys: O(m log m) for m months (at most 120 for 10 years).

## Tests that prove it

| Check | Test in `tests/ContractManagement.Scheduling.Tests` |
|---|---|
| SEC1 | Null payments, null expenses, unequal totals (message has both totals), empty expenses returns empty with no throw. |
| NFR3.1 | Every example asserts exact `decimal` values and that ST plus LT equals the absolute balance on every row. |
| NFR3.2 | One test per worked example E1 to E10, plus one per ST/LT branch of BR2.4 (B = 0; L = 0; opposite sign; abs L at least abs B; abs L below abs B) and one per sign of BR2.5 (asset and liability). |
| NFR3.3 | Two calls with the same input give equal lists. |
| NFR1.1 | Largest allowed input (10 streams, 10 years): one warm-up call, then the best of 3 timed calls must be under 50 ms, so a busy machine does not fail `scripts/verify.ps1`. |
| NFR2.1 | Existing generator tests pass with only the new `Balance` argument added. |
