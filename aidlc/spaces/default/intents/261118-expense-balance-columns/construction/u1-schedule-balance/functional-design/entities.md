# Entities: u1-schedule-balance

U1 adds one value object to the schedule library. It is calculated, never stored (ADR-002), and owned by ScheduleGenerator. It is built from the existing PaymentEntry and ExpenseEntry values, which do not change.

```yaml
entities:
  - name: BalanceRow
    description: One calendar month of the expense balance schedule, built from payment and expense rows.
    attributes:
      - { name: periodStart, type: calendar date, required: true, constraints: "first day of the month; for a month with an Overtime expense row, that row's period start" }
      - { name: periodEnd, type: calendar date, required: true, constraints: "last day of the month; for a month with an Overtime expense row, that row's period end" }
      - { name: expense, type: money, required: true, min: 0, constraints: "sum of the expense rows in the month; 0 for a payment-only month; 2 decimal places" }
      - { name: accrualPrepaid, type: money, required: true, constraints: "signed; payments due in the month minus expense" }
      - { name: cumulativeBalance, type: money, required: true, constraints: "signed; running total of accrualPrepaid; 0 on the last row" }
      - { name: stAccruedLiability, type: money, required: true, min: 0 }
      - { name: ltAccruedLiability, type: money, required: true, min: 0 }
      - { name: stPrepaidAsset, type: money, required: true, min: 0 }
      - { name: ltPrepaidAsset, type: money, required: true, min: 0 }
    constraints:
      - "at most one row per calendar month; rows in month order"
      - "when cumulativeBalance is negative, both prepaid columns are 0; when positive, both accrued columns are 0; when 0, all four are 0"
      - "the two filled columns add up exactly to the size of cumulativeBalance"
    relationships:
      - { to: PaymentEntry, cardinality: many to many, direction: "a row sums the payments due in its month" }
      - { to: ExpenseEntry, cardinality: one to zero or one, direction: "a row takes the expense row of its month, if any" }
```

## Summary

BalanceRow is the only new type. The input types (PaymentEntry with stream id, due date and amount; ExpenseEntry with period start, period end and amount) are unchanged. ScheduleResult gains a list of BalanceRow values (contract C2).
