# Business Rules: u1-schedule-engine

```yaml
rules:
  - id: BR1.1
    statement: The first payment of a stream is on the stream's start date.
    category: calculation
    applies_to: PaymentEntry
    trigger: Generating payments for a stream
    logic: The first PaymentEntry has dueDate equal to the stream's startDate.
    violation: Not applicable (always holds)
    source: FR4.2

  - id: BR1.2
    statement: A Weekly stream pays every 7 days.
    category: calculation
    applies_to: PaymentEntry
    trigger: Stream frequency is Weekly
    logic: Payment number n (starting at 0) is due on startDate plus 7 times n days.
    violation: Not applicable (always holds)
    source: FR4.2

  - id: BR1.3
    statement: A Monthly, Quarterly or Annually stream pays on the same day of the month as its start date, or on the last day of the month when that month is shorter.
    category: calculation
    applies_to: PaymentEntry
    trigger: Stream frequency is Monthly, Quarterly or Annually
    logic: >
      Payment number n (starting at 0) is due n steps after the start, where a step is 1 month for Monthly,
      3 months for Quarterly and 12 months for Annually. The day is the start date's day, counted from the original
      start date every time (not from the previous payment), and is cut back to the last day of the month when the month
      has fewer days. A start of 31 January, Monthly, pays on 31 January, 28 February (29 in a leap year), 31 March, 30 April.
    violation: Not applicable (always holds)
    source: FR4.3

  - id: BR1.4
    statement: A stream pays on every scheduled date up to and including its end date, and never after it.
    category: constraint
    applies_to: PaymentEntry
    trigger: Generating payments for a stream
    logic: Keep payment dates while the date is on or before the stream's endDate; stop at the first one after it. A stream whose first payment is the end date has one payment.
    violation: Not applicable (always holds)
    source: FR4.2

  - id: BR1.5
    statement: A OneTime stream pays once, on its start date.
    category: calculation
    applies_to: PaymentEntry
    trigger: Stream frequency is OneTime
    logic: One PaymentEntry with dueDate equal to startDate and the stream's amount. endDate is not used.
    violation: Not applicable (always holds)
    source: FR2.3

  - id: BR1.6
    statement: The payment schedule holds the payments of every stream, each linked to its own stream, in a fixed order.
    category: policy
    applies_to: ScheduleResult
    trigger: Request has more than one stream
    logic: >
      Every PaymentEntry carries its stream's streamId. Entries are ordered by dueDate; entries with the same dueDate
      are ordered by the position of their stream in the request. paymentTotal is the sum of all entry amounts.
    violation: Not applicable (always holds)
    source: FR4.1

  - id: BR1.7
    statement: The same request always produces the same result.
    category: policy
    applies_to: ScheduleResult
    trigger: Any run
    logic: The engine uses only its input. It reads no clock, random value, file, database or network, and keeps nothing between runs.
    violation: Not applicable (always holds)
    source: NFR4

  - id: BR1.8
    statement: The engine rejects input that breaks the caller's promises.
    category: validation
    applies_to: ScheduleRequest
    trigger: Start of every run
    logic: >
      Check that there is at least one stream; every amount is greater than zero; every stream's endDate is not before its startDate
      (and equals it for OneTime); Overtime has a start and end date with the end not before the start; AllAtOnce has a date.
    violation: Stop with an argument error that names the broken promise. Nothing is returned.
    source: Contract C2 preconditions

  - id: BR2.1
    statement: The amount to expense is the total of the payment schedule.
    category: calculation
    applies_to: ExpenseEntry
    trigger: Expense method is Overtime or AllAtOnce
    logic: The base amount is paymentTotal, the sum of every payment amount.
    violation: Not applicable (always holds)
    source: FR3.4

  - id: BR2.2
    statement: AllAtOnce books the whole amount on one date.
    category: calculation
    applies_to: ExpenseEntry
    trigger: Expense method is AllAtOnce
    logic: One ExpenseEntry with periodStart and periodEnd both equal to the expense date, and amount equal to the base amount.
    violation: Not applicable (always holds)
    source: FR5.3

  - id: BR2.3
    statement: Overtime spreads the amount equally per day, with one row per calendar month the range touches.
    category: calculation
    applies_to: ExpenseEntry
    trigger: Expense method is Overtime
    logic: >
      The range runs from the expense start date to the end date, both included. totalDays is the number of days in it.
      For each calendar month that has days in the range, one row covers those days only: periodStart is the later of the range start and
      the first of that month, periodEnd is the earlier of the range end and the last of that month. monthDays is the number of days in that row.
      A range that starts mid-month gives a short first row.
    violation: Not applicable (always holds)
    source: FR5.2

  - id: BR2.4
    statement: The Overtime amounts add up exactly to the base amount, with any rounding difference on the last row.
    category: calculation
    applies_to: ExpenseEntry
    trigger: Expense method is Overtime
    logic: >
      Every row except the last has amount = base amount times monthDays divided by totalDays, rounded to 2 decimals (BR2.6).
      The last row has amount = base amount minus the sum of the other rows. A range inside one month has one row holding the whole base amount.
    violation: Not applicable (always holds)
    source: FR5.2

  - id: BR2.5
    statement: The method None produces no expense schedule.
    category: calculation
    applies_to: ScheduleResult
    trigger: Expense method is None
    logic: expenses is empty and expenseTotal is 0.
    violation: Not applicable (always holds)
    source: FR5.4

  - id: BR2.6
    statement: Every amount has 2 decimal places, rounded half away from zero.
    category: calculation
    applies_to: ExpenseEntry
    trigger: Any amount that is divided
    logic: A value with a third decimal of 5 or more rounds up, below 5 rounds down (for positive amounts). Money is handled as an exact decimal, never as a binary floating-point number.
    violation: Not applicable (always holds)
    source: FR5.5
```

## Rules Summary

| Rule | In short | Source |
|---|---|---|
| BR1.1 | First payment is on the stream start date | FR4.2 |
| BR1.2 | Weekly: every 7 days | FR4.2 |
| BR1.3 | Monthly, Quarterly, Annually: same day of month, cut back to the last day of a short month, counted from the original start | FR4.3 |
| BR1.4 | Pay up to and including the end date | FR4.2 |
| BR1.5 | OneTime: one payment on the start date | FR2.3 |
| BR1.6 | All streams combined, each payment linked to its stream, fixed order | FR4.1 |
| BR1.7 | Same input always gives the same output | NFR4 |
| BR1.8 | Reject input that breaks the caller's promises | Contract C2 |
| BR2.1 | Expense base amount is the payment total | FR3.4 |
| BR2.2 | AllAtOnce: one row on one date | FR5.3 |
| BR2.3 | Overtime: equal per day, one row per calendar month | FR5.2 |
| BR2.4 | Overtime rows add up exactly; difference on last row | FR5.2 |
| BR2.5 | None: no rows, total 0 | FR5.4 |
| BR2.6 | 2 decimals, half away from zero, exact decimal | FR5.5 |
