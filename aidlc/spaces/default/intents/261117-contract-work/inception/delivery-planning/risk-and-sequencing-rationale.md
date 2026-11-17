# Risk and Sequencing Rationale

A Bolt is one build pass over one or more units that ends in something that runs and is tested.

## Heuristic Used

Risk first (Cohn's "riskiest assumption first"), chosen in Q1. No formal scoring model such as WSJF (Weighted Shortest Job First: value and urgency divided by size) is used. With two Bolts and fixed dependencies, a score would not change the order.

## Why This Order

1. **The schedule rules carry the most risk.** Date arithmetic (month-end payments, weekly steps across year ends), the daily Overtime split, and exact rounding are where mistakes are likely and costly. Building U1 first proves these rules in isolation before anything depends on them.
2. **The backend comes next because it depends on U1.** Bundling U1 and U2 into one Bolt (Q2) means the first review shows a working API with real data, not a library on its own.
3. **The frontend comes last.** Its risk is lower: the screens are specified in the Refined Mockups and the API in contract C1. Building it against a finished backend means the final check uses the real API, not only a fake one.

## Dependency Check

The unit dependency graph says U2 depends on U1, and U3 has no unit dependency. The order U1, U2, U3 is a valid topological order, so there is no deviation to justify.

## Trade-off Accepted

The screens appear only in Bolt 2, so you will not click through the UI until the backend is done. Q1 option B would have shown the screens earlier, against a fake API.
