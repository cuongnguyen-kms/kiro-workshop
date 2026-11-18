# Risk and Sequencing Rationale: expense balance columns

A Bolt is one build pass over a piece of the work, ending in something that runs.

**Heuristic:** risk-first (Cohn's ordering by risk), chosen in Q1. No weighted scoring model (such as WSJF, which ranks work by value and urgency against size) was used: with three Units and one valid order through the rules, ranking adds nothing.

## Order

1. **U1 first.** It carries the most risk: the 12-month look-ahead, sign changes, payment-only months and exact 2-decimal splits. Three review findings in the earlier stages were about exactly these rules (the larger same-sign later balance, month-end look-ahead dates, and periods for All at once and payment-only months). Building it first settles them in tests before anything depends on them.
2. **U2 second.** It depends on U1's builder (the only code edge in the DAG). Its own risk is small: mapping and a read-time rebuild.
3. **U3 last.** It has no code dependency, so it could go anywhere, but building it after U2 lets its fake API match the real responses U2 now returns, and lets the person see the finished feature in the running app at its checkpoint.

## Check against the dependency DAG

The order U1, U2, U3 is a valid topological order of `inception/units-generation/unit-of-work-dependency.md` (U2 after U1; U3 free). No deviation to justify.

## Carried-forward risks

| Risk | Where it is handled |
|---|---|
| Month-end and leap-year look-ahead dates (Contract Design R-01) | U1 Functional Design and tests |
| Period dates for All at once and payment-only rows (Contract Design R-02) | U1 Functional Design |
| Positional C# records break existing callers at compile time (Contract Design R-03) | U1 and U2 code plans update callers in the same change |
| Sticky column and popup width cannot be checked in jsdom (Refined Mockups R-04) | U3 checkpoint: checked by eye in a browser |
| Existing tests assert the old number strings (team.md) | U3 code plan |
