# Entities: u1-schedule-engine

The schedule engine takes validated payment streams and an expense method, and returns two schedules. Everything here is a value: nothing is stored, and nothing has an identity of its own beyond the stream id that payments point back to. Names match the Contract Design interface (C2).

```yaml
entities:
  - name: StreamInput
    description: One payment stream the contract owner entered, already validated by the caller.
    attributes:
      - { name: streamId, type: identifier, required: true, unique: true, constraints: Unique within one request }
      - { name: amount, type: money, required: true, constraints: Greater than zero, at most 2 decimal places }
      - { name: frequency, type: choice, required: true, allowed: [Weekly, Monthly, Quarterly, Annually, OneTime] }
      - { name: startDate, type: calendar date, required: true }
      - { name: endDate, type: calendar date, required: true, constraints: Not before startDate; equal to startDate for OneTime }
    relationships: []

  - name: ExpenseInput
    description: How the contract's cost is recognised.
    attributes:
      - { name: method, type: choice, required: true, allowed: [Overtime, AllAtOnce, None] }
      - { name: startDate, type: calendar date, required: false, constraints: Required when method is Overtime; absent otherwise }
      - { name: endDate, type: calendar date, required: false, constraints: Required when method is Overtime, not before startDate; absent otherwise }
      - { name: date, type: calendar date, required: false, constraints: Required when method is AllAtOnce; absent otherwise }
    relationships: []

  - name: ScheduleRequest
    description: Everything the engine needs for one run.
    attributes:
      - { name: streams, type: list of StreamInput, required: true, constraints: At least one }
      - { name: expense, type: ExpenseInput, required: true }
    relationships:
      - { to: StreamInput, cardinality: one to many, direction: request holds streams }
      - { to: ExpenseInput, cardinality: one to one, direction: request holds expense }

  - name: PaymentEntry
    description: One payment due on one date, linked to the stream that produced it.
    attributes:
      - { name: streamId, type: identifier, required: true, references: StreamInput.streamId }
      - { name: dueDate, type: calendar date, required: true }
      - { name: amount, type: money, required: true, constraints: Equals the stream's amount }
    relationships:
      - { to: StreamInput, cardinality: many to one, direction: entry points to its stream }

  - name: ExpenseEntry
    description: One row of the expense schedule. For Overtime it covers the days of the expense range that fall in one calendar month. For AllAtOnce it is a single date.
    attributes:
      - { name: periodStart, type: calendar date, required: true }
      - { name: periodEnd, type: calendar date, required: true, constraints: Not before periodStart; equals periodStart for AllAtOnce }
      - { name: amount, type: money, required: true, constraints: 2 decimal places }
    relationships: []

  - name: ScheduleResult
    description: Both schedules and their totals.
    attributes:
      - { name: payments, type: list of PaymentEntry, required: true, constraints: Ordered by dueDate, then by stream order in the request }
      - { name: paymentTotal, type: money, required: true, constraints: Equals the sum of payments }
      - { name: expenses, type: list of ExpenseEntry, required: true, constraints: Ordered by periodStart; empty when method is None }
      - { name: expenseTotal, type: money, required: true, constraints: Equals the sum of expenses; equals paymentTotal unless method is None, then 0 }
    relationships:
      - { to: PaymentEntry, cardinality: one to many, direction: result holds payments }
      - { to: ExpenseEntry, cardinality: one to many, direction: result holds expenses }
```

## Summary

- Two inputs (streams and the expense method), one request that holds them, and one result that holds two lists and their totals.
- The engine has no state. The same request always gives the same result.
- The `Contract` itself, with its id, name and dates, belongs to the backend (U2), not to this unit.
