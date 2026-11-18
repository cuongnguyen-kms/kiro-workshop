# Contract Summary: expense balance columns

Two boundaries change, both from the existing system and both additive. The unit DAG (`inception/units-generation/unit-of-work-dependency.md`) has one code edge, U2 to U1, which is contract C2. U3 reaches U2 over HTTP, which is contract C1. The earlier versions of both contracts are in `aidlc/spaces/default/intents/261117-contract-work/inception/contract-design/contract-summary.md`; the preview endpoint was added by the schedule-preview work. No questions were needed: requirement NFR2 already fixes the change as additive within v1, and every other point (mechanism, ownership, versioning, errors) is unchanged.

## Contracts

| # | Provider Unit | Consumer | Mechanism | Owner |
|---|---|---|---|---|
| C1 | U2 u2-api-balance | U3 u3-web-balance (browser, cross-origin) | Synchronous REST over HTTPS, JSON, `/api/v1` | U2 |
| C2 | U1 u1-schedule-balance | U2 u2-api-balance | In-process .NET library call | U1 |

## Design decisions

- **Additive, not a reshaped expense schedule.** The existing `expenseSchedule.entries` and `expenseSchedule.total` stay exactly as they are (same rows, same meaning). A new array `expenseSchedule.balanceRows` carries the monthly rows with the new columns. A v1 client that does not know the field keeps working (NFR2). U3 shows `balanceRows` and no longer shows `entries` or `total` for the expense schedule (FR2.5).
- **Same shape in both responses.** The contract detail response (`GET` and `POST /contracts`) and the preview response (`POST /contracts/preview`) both use the same `ExpenseSchedule` schema, so the tab and the popup render the same rows (FR2.6, FR4.3).
- **Empty for method None, and for a payment preview.** `balanceRows` is `[]` when the expense method is None (FR1.4), and in a `kind=payment` preview, where the expense is not considered.
- **Signed values, positive splits.** `accrualPrepaid` and `cumulativeBalance` are signed (negative means accrual). The four ST and LT values are never negative (FR2.4). Formatting with brackets is the client's job (ADR-003).

## C1. Contract API changes (U2 to U3)

Only the changed schemas are shown; everything else in C1 v1 is unchanged.

```yaml
openapi: 3.1.0
info:
  title: Contract Management API
  version: 1.1.0
servers:
  - url: /api/v1
components:
  schemas:
    ExpenseSchedule:
      type: object
      required: [entries, total, balanceRows]
      description: >
        Used by ContractDetail.expenseSchedule and SchedulePreview.expenseSchedule.
        entries and total are unchanged from v1.0. balanceRows is new and is empty when the
        expense method is None or the preview kind is payment.
      properties:
        entries:
          type: array
          items: { $ref: '#/components/schemas/ExpenseEntry' }
        total: { type: number, multipleOf: 0.01 }
        balanceRows:
          type: array
          description: One row per calendar month with a payment or an expense, in date order.
          items: { $ref: '#/components/schemas/BalanceRow' }
    BalanceRow:
      type: object
      required:
        - periodStart
        - periodEnd
        - expense
        - accrualPrepaid
        - cumulativeBalance
        - stAccruedLiability
        - ltAccruedLiability
        - stPrepaidAsset
        - ltPrepaidAsset
      properties:
        periodStart:
          type: string
          format: date
          description: First day of the month, or the expense start date when an Overtime expense starts later in that month.
        periodEnd:
          type: string
          format: date
          description: Last day of the month, or the expense end date when an Overtime expense ends earlier in that month. The 12-month look-ahead counts from this date.
        expense: { type: number, multipleOf: 0.01, minimum: 0, description: The month's expense; 0 for a payment-only month }
        accrualPrepaid: { type: number, multipleOf: 0.01, description: Payments due in the month minus the month's expense; negative means accrual }
        cumulativeBalance: { type: number, multipleOf: 0.01, description: Running total of accrualPrepaid; 0 on the last row }
        stAccruedLiability: { type: number, multipleOf: 0.01, minimum: 0 }
        ltAccruedLiability: { type: number, multipleOf: 0.01, minimum: 0 }
        stPrepaidAsset: { type: number, multipleOf: 0.01, minimum: 0 }
        ltPrepaidAsset: { type: number, multipleOf: 0.01, minimum: 0 }
```

Example `balanceRows` for 12,000.00 paid on 2027-01-15 and an Overtime expense of 1,000.00 a month from 2027-01-01 to 2027-12-31 (first two rows):

```json
[
  { "periodStart": "2027-01-01", "periodEnd": "2027-01-31", "expense": 1000.00, "accrualPrepaid": 11000.00, "cumulativeBalance": 11000.00,
    "stAccruedLiability": 0, "ltAccruedLiability": 0, "stPrepaidAsset": 11000.00, "ltPrepaidAsset": 0 },
  { "periodStart": "2027-02-01", "periodEnd": "2027-02-28", "expense": 1000.00, "accrualPrepaid": -1000.00, "cumulativeBalance": 10000.00,
    "stAccruedLiability": 0, "ltAccruedLiability": 0, "stPrepaidAsset": 10000.00, "ltPrepaidAsset": 0 }
]
```

**Unchanged C1 rules:** errors (RFC 9457 Problem Details), dates as `YYYY-MM-DD`, amounts as JSON numbers with at most 2 decimals, CORS for the configured origin, paging, and the preview endpoint's `kind` parameter.

## C2. Schedule engine interface changes (U1 to U2)

U1 owns the interface. New members only; existing records and `Generate` keep their meaning.

```csharp
namespace ContractManagement.Scheduling;

public sealed record BalanceRow(
    DateOnly PeriodStart, DateOnly PeriodEnd, decimal Expense,
    decimal AccrualPrepaid, decimal CumulativeBalance,
    decimal StAccruedLiability, decimal LtAccruedLiability,
    decimal StPrepaidAsset, decimal LtPrepaidAsset);

public interface IBalanceScheduleBuilder
{
    /// Pure and deterministic. One row per calendar month with a payment or an expense, by date.
    /// Empty when expenses is empty (method None). Used for previews and for saved contracts.
    /// Throws ArgumentException when the totals of payments and expenses differ.
    IReadOnlyList<BalanceRow> Build(IReadOnlyList<PaymentEntry> payments, IReadOnlyList<ExpenseEntry> expenses);
}
```

- `ScheduleResult` gains `IReadOnlyList<BalanceRow> Balance`, filled by `Generate` with the same builder, so a preview and a later read of the saved rows give the same rows (FR4.3).
- **Preconditions (U2 guarantees, U1 checks):** the payment total equals the expense total whenever expenses are given; rows have at most 2 decimals.
- **Rules owned by U1 (detailed in U1's Functional Design):** FR1.1 to FR1.3 and FR2.1 to FR2.4. The look-ahead uses the cumulative balance of the latest row whose period end is on or before the row's period end plus 12 calendar months; when no later row reaches that date the whole balance is ST; when the later balance has the opposite sign or is 0 the whole balance is ST; when the later balance has the same sign and is larger, ST is 0 and LT is the whole balance; otherwise ST is the reduction and LT the remaining later balance.

## Contract Ownership Rules

- **C1:** U2 owns the specification. This is an additive change within v1 (version 1.1.0): a new field only; clients ignore fields they do not know. U3 builds against this document and tests with its fake API updated from it before U3's construction starts.
- **C2:** U1 owns the interface. U2 is its only consumer, in the same solution; both change in the same change set and U2's build and tests check it.
- **Breaking changes:** unchanged rule from the earlier contract: removing or renaming a field, or changing a type or meaning, needs `/api/v2`.

## Open Questions

| Contract | Question | Blocks |
|---|---|---|
| C1 | None. | none |
