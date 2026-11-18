# Business Rules: u1-schedule-balance

These rules settle the two date gaps the Contract Design review left open (R-01 month-end look-ahead, R-02 periods for All at once and payment-only months): the look-ahead counts calendar months, not days, and every row belongs to exactly one calendar month.

```yaml
rules:
  - id: BR1.1
    statement: The balance schedule has one row per calendar month that has a payment due or an expense row.
    category: calculation
    applies_to: BalanceRow
    trigger: build balance rows
    logic: Collect the months of every payment due date and every expense row's period start. IF the expense rows are empty THEN return no rows. ELSE return one row per collected month, ordered by month.
    violation: none
    source: FR1.1, FR1.3, FR1.4
  - id: BR1.2
    statement: A row's period is its calendar month, narrowed to the Overtime expense range when the month has an Overtime expense row.
    category: calculation
    applies_to: BalanceRow.periodStart, BalanceRow.periodEnd
    trigger: build balance rows
    logic: IF the month has an expense row whose period start differs from its period end (Overtime) THEN the row's period is that expense row's period. ELSE the row's period is the first to the last day of the month (payment-only months and the All at once month).
    violation: none
    source: FR1.2
  - id: BR1.3
    statement: A row's expense is the expense of its month, and 0 for a payment-only month.
    category: calculation
    applies_to: BalanceRow.expense
    trigger: build balance rows
    logic: expense = sum of the amounts of the expense rows whose period start falls in the month (0 when none).
    violation: none
    source: FR1.1
  - id: BR2.1
    statement: (Accrual)/Prepaid is the month's payments minus the month's expense.
    category: calculation
    applies_to: BalanceRow.accrualPrepaid
    trigger: build balance rows
    logic: accrualPrepaid = sum of the amounts of payments whose due date falls in the month, minus expense. Positive means prepaid, negative means accrued.
    violation: none
    source: FR2.1
  - id: BR2.2
    statement: Cumulative Balance is the running total of (Accrual)/Prepaid and ends at 0.
    category: calculation
    applies_to: BalanceRow.cumulativeBalance
    trigger: build balance rows
    logic: cumulativeBalance of a row = cumulativeBalance of the previous row plus accrualPrepaid (the first row starts from 0). Because total payments equal total expense, the last row is 0.
    violation: none
    source: FR2.2
  - id: BR2.3
    statement: The 12-month look-ahead balance is the balance of the latest row in or before the same month one year later.
    category: calculation
    applies_to: BalanceRow
    trigger: build balance rows
    logic: For a row in month M, the target month is M plus 12 calendar months (January 2027 looks at January 2028; February 2027 looks at February 2028, whose last day is the 29th). The look-ahead balance L is the cumulativeBalance of the latest row whose month is on or before the target month. A row with no later row up to the target month uses its own later rows only, so when the schedule ends before the target month, L is the last row's balance, which is 0.
    violation: none
    source: FR2.3
  - id: BR2.4
    statement: The balance is split into the part that reverses within 12 months (ST) and the rest (LT).
    category: calculation
    applies_to: BalanceRow
    trigger: build balance rows
    logic: Let B be the row's cumulativeBalance and L the look-ahead balance. IF B is 0 THEN ST and LT are 0. ELSE IF L is 0 or L has the opposite sign to B THEN ST is the size of B and LT is 0. ELSE IF the size of L is at least the size of B THEN ST is 0 and LT is the size of B. ELSE ST is the size of B minus the size of L, and LT is the size of L.
    violation: none
    source: FR2.3, FR4.2
  - id: BR2.5
    statement: The sign of the balance chooses the columns; the amounts are always positive.
    category: calculation
    applies_to: BalanceRow.stAccruedLiability, BalanceRow.ltAccruedLiability, BalanceRow.stPrepaidAsset, BalanceRow.ltPrepaidAsset
    trigger: build balance rows
    logic: IF B is negative THEN stAccruedLiability = ST, ltAccruedLiability = LT and both prepaid columns are 0. IF B is positive THEN stPrepaidAsset = ST, ltPrepaidAsset = LT and both accrued columns are 0. IF B is 0 THEN all four are 0.
    violation: none
    source: FR2.4
  - id: BR3.1
    statement: The same builder serves previews and saved contracts.
    category: policy
    applies_to: BalanceRow
    trigger: generate schedules, or read a saved contract
    logic: Generate builds the balance rows from the payment and expense rows it just produced, with the same builder U2 calls for stored rows. The builder depends only on its inputs, so the same rows give the same balance rows.
    violation: none
    source: FR2.6, FR4.1, FR4.3
  - id: BR3.2
    statement: The builder refuses inputs whose totals differ.
    category: validation
    applies_to: builder input
    trigger: build balance rows with expense rows present
    logic: IF the expense rows are not empty AND the sum of payment amounts differs from the sum of expense amounts THEN throw an argument error. Amounts are exact decimals; no rounding happens in the builder.
    violation: ArgumentException naming the broken precondition (contract C2)
    source: FR4.2, NFR3
```

## Rules summary

| ID | Rule | Category |
|---|---|---|
| BR1.1 | One row per month with a payment or an expense; none for method None | calculation |
| BR1.2 | Row period: whole month, narrowed to the Overtime range in Overtime months | calculation |
| BR1.3 | Row expense; 0 for payment-only months | calculation |
| BR2.1 | (Accrual)/Prepaid = month's payments minus expense | calculation |
| BR2.2 | Cumulative Balance = running total, ends at 0 | calculation |
| BR2.3 | Look-ahead: latest row in or before the same month next year | calculation |
| BR2.4 | ST and LT split, never negative, adding up to the balance | calculation |
| BR2.5 | Sign chooses accrued or prepaid columns | calculation |
| BR3.1 | One builder for previews and saved contracts | policy |
| BR3.2 | Totals must match | validation |
