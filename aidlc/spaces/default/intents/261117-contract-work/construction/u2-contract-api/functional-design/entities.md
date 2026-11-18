# Entities: u2-contract-api

The Contract aggregate owned by ContractService (ADR-002). Contract is the aggregate root. Streams and both schedules exist only inside a contract and are saved and loaded with it.

```yaml
entities:
  - name: Contract
    description: A saved contract with its period and the chosen expense method.
    attributes:
      - { name: contractId, type: identifier, required: true, unique: true, default: generated on save }
      - { name: name, type: text, required: true, constraints: "trimmed, 1 to 200 characters; duplicates allowed (Q1)" }
      - { name: startDate, type: calendar date, required: true }
      - { name: endDate, type: calendar date, required: true, constraints: "not before startDate" }
      - { name: expenseMethod, type: text, required: true, allowed: [Overtime, AllAtOnce, None] }
      - { name: expenseStartDate, type: calendar date, required: false, constraints: "set only for Overtime" }
      - { name: expenseEndDate, type: calendar date, required: false, constraints: "set only for Overtime, not before expenseStartDate" }
      - { name: expenseDate, type: calendar date, required: false, constraints: "set only for AllAtOnce" }
      - { name: createdAt, type: timestamp, required: true, default: time of save, constraints: "audit only, not part of the API contract" }
    constraints:
      - "expense dates are within startDate and endDate"
      - "at least one PaymentStream"
    relationships:
      - { to: PaymentStream, cardinality: one to many (1..*), direction: Contract owns streams }
      - { to: PaymentScheduleEntry, cardinality: one to many (0..*), direction: Contract owns rows }
      - { to: ExpenseScheduleEntry, cardinality: one to many (0..*), direction: Contract owns rows }

  - name: PaymentStream
    description: One way the contract pays out, with an amount, a frequency and a date range.
    attributes:
      - { name: streamId, type: identifier, required: true, unique: true, default: generated on save }
      - { name: contractId, type: identifier, required: true, references: Contract }
      - { name: position, type: whole number, required: true, min: 0, constraints: "order the stream had in the request; keeps output order stable" }
      - { name: amount, type: money, required: true, min: "greater than 0", constraints: "at most 2 decimal places" }
      - { name: frequency, type: text, required: true, allowed: [Weekly, Monthly, Quarterly, Annually, OneTime] }
      - { name: startDate, type: calendar date, required: true }
      - { name: endDate, type: calendar date, required: true, constraints: "not before startDate; equals startDate for OneTime" }
    constraints:
      - "startDate and endDate are within the contract dates"
    relationships:
      - { to: Contract, cardinality: many to one, direction: belongs to }
      - { to: PaymentScheduleEntry, cardinality: one to many (1..*), direction: source of rows }

  - name: PaymentScheduleEntry
    description: One generated payment, linked to the stream it came from.
    attributes:
      - { name: paymentEntryId, type: identifier, required: true, unique: true }
      - { name: contractId, type: identifier, required: true, references: Contract }
      - { name: streamId, type: identifier, required: true, references: PaymentStream }
      - { name: dueDate, type: calendar date, required: true }
      - { name: amount, type: money, required: true, constraints: "greater than 0, 2 decimal places" }
    constraints:
      - "never edited after save (editing is out of scope)"
    relationships:
      - { to: PaymentStream, cardinality: many to one, direction: generated from }

  - name: ExpenseScheduleEntry
    description: One generated expense row. Overtime has one per calendar month, AllAtOnce has one, None has none.
    attributes:
      - { name: expenseEntryId, type: identifier, required: true, unique: true }
      - { name: contractId, type: identifier, required: true, references: Contract }
      - { name: periodStart, type: calendar date, required: true }
      - { name: periodEnd, type: calendar date, required: true, constraints: "equals periodStart for AllAtOnce" }
      - { name: amount, type: money, required: true, constraints: "2 decimal places" }
    constraints:
      - "sum of amounts equals the payment schedule total unless the method is None"
    relationships:
      - { to: Contract, cardinality: many to one, direction: belongs to }
```

## Summary

Four entities in one aggregate. Totals are not stored: the payment total and expense total are the sums of their rows, computed when the contract is read, so they cannot disagree with the rows. `periodOrDate` in the component catalogue is stored as `periodStart` and `periodEnd`, which matches the C1 contract. Schedule rows keep their generated order: payments by due date then stream position, expenses by period start.
