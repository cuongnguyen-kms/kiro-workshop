# Contract Summary

Two boundaries come from the unit dependency graph and the deployment choice. One is the HTTP API that the separately deployed React app (U3) calls on the backend (U2). The other is the in-process interface that the backend (U2) calls on the schedule library (U1).

## Contracts

| # | Provider Unit | Consumer | Mechanism | Owner |
|---|---|---|---|---|
| C1 | U2 u2-contract-api | U3 u3-contract-web (browser, cross-origin) | Synchronous REST over HTTPS, JSON | U2 |
| C2 | U1 u1-schedule-engine | U2 u2-contract-api | In-process .NET library call | U1 |

## C1. Contract API (U2 to U3)

Decisions behind this contract:
- The version is in the path, `/api/v1`. Within v1, changes only add fields, and a breaking change means `/api/v2` (Q1).
- Errors use RFC 9457 Problem Details. Validation errors put field-level messages in an `errors` map keyed by camelCase field path, for example `paymentStreams[0].amount` (Q2).
- The contracts list is paged. Page sizes are 10, 20, 50 or 100, and the default is 10 (Q3).

Conventions:
- **Dates:** ISO 8601 calendar dates (`YYYY-MM-DD`) with no time.
- **Amounts:** JSON numbers with at most 2 decimal places. The backend uses `decimal`. The frontend only displays amounts and never calculates with them.
- **IDs:** UUIDs.
- **Enum values:** strings, exactly as listed in the schemas.
- **Validation messages:** the backend returns the exact wording from the Refined Mockups interaction spec.
- **Cross-origin calls:** the API allows only the configured frontend origin, for `GET` and `POST` with the `Content-Type` header.

```yaml
openapi: 3.1.0
info:
  title: Contract Management API
  version: 1.0.0
servers:
  - url: /api/v1
paths:
  /contracts:
    post:
      operationId: createContract
      summary: Validate a contract, generate its schedules, and save everything in one transaction (FR6.1)
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/CreateContractRequest' }
      responses:
        '201':
          description: Saved. Body is the saved contract with both schedules.
          headers:
            Location:
              description: URL of the new contract, /api/v1/contracts/{id}
              schema: { type: string }
          content:
            application/json:
              schema: { $ref: '#/components/schemas/ContractDetail' }
        '400':
          description: Validation failed. Nothing was saved.
          content:
            application/problem+json:
              schema: { $ref: '#/components/schemas/ValidationProblem' }
        '500':
          description: Unexpected failure. Nothing was saved.
          content:
            application/problem+json:
              schema: { $ref: '#/components/schemas/Problem' }
    get:
      operationId: listContracts
      summary: One page of saved contracts, newest start date first (FR6.3)
      parameters:
        - name: page
          in: query
          required: false
          schema: { type: integer, minimum: 1, default: 1 }
        - name: pageSize
          in: query
          required: false
          schema: { type: integer, enum: [10, 20, 50, 100], default: 10 }
      responses:
        '200':
          description: A page of contracts. A page past the last one returns an empty items list.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/ContractPage' }
        '400':
          description: page is below 1, or pageSize is not one of 10, 20, 50 or 100.
          content:
            application/problem+json:
              schema: { $ref: '#/components/schemas/ValidationProblem' }
  /contracts/{id}:
    get:
      operationId: getContract
      summary: One saved contract with its streams and both schedules (FR6.2)
      parameters:
        - name: id
          in: path
          required: true
          schema: { type: string, format: uuid }
      responses:
        '200':
          description: The contract.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/ContractDetail' }
        '404':
          description: No contract has this id.
          content:
            application/problem+json:
              schema: { $ref: '#/components/schemas/Problem' }
components:
  schemas:
    Frequency:
      type: string
      enum: [Weekly, Monthly, Quarterly, Annually, OneTime]
    ExpenseMethod:
      type: string
      enum: [Overtime, AllAtOnce, None]
    Money:
      type: number
      exclusiveMinimum: 0
      multipleOf: 0.01
    CreateContractRequest:
      type: object
      required: [name, startDate, endDate, paymentStreams, expense]
      properties:
        name: { type: string, minLength: 1, maxLength: 200, description: Must not be blank or whitespace only }
        startDate: { type: string, format: date }
        endDate: { type: string, format: date, description: Must not be before startDate }
        paymentStreams:
          type: array
          minItems: 1
          items: { $ref: '#/components/schemas/PaymentStreamInput' }
        expense: { $ref: '#/components/schemas/ExpenseInput' }
    PaymentStreamInput:
      type: object
      required: [amount, frequency, startDate]
      properties:
        amount: { $ref: '#/components/schemas/Money' }
        frequency: { $ref: '#/components/schemas/Frequency' }
        startDate: { type: string, format: date, description: Within the contract dates }
        endDate:
          type: string
          format: date
          description: Required unless frequency is OneTime. For OneTime it is ignored and stored as startDate. Within the contract dates and not before startDate.
    ExpenseInput:
      type: object
      required: [method]
      properties:
        method: { $ref: '#/components/schemas/ExpenseMethod' }
        startDate: { type: string, format: date, description: Required for Overtime only }
        endDate: { type: string, format: date, description: Required for Overtime only, not before startDate }
        date: { type: string, format: date, description: Required for AllAtOnce only }
    PaymentStream:
      type: object
      required: [id, amount, frequency, startDate, endDate]
      properties:
        id: { type: string, format: uuid }
        amount: { $ref: '#/components/schemas/Money' }
        frequency: { $ref: '#/components/schemas/Frequency' }
        startDate: { type: string, format: date }
        endDate: { type: string, format: date }
    PaymentEntry:
      type: object
      required: [streamId, dueDate, amount]
      properties:
        streamId: { type: string, format: uuid }
        dueDate: { type: string, format: date }
        amount: { $ref: '#/components/schemas/Money' }
    ExpenseEntry:
      type: object
      required: [periodStart, periodEnd, amount]
      description: >
        Overtime has one entry per calendar month, covering the days of the expense range in that month.
        AllAtOnce has one entry where periodStart equals periodEnd, which is the expense date.
      properties:
        periodStart: { type: string, format: date }
        periodEnd: { type: string, format: date }
        amount: { type: number, multipleOf: 0.01 }
    Schedule:
      type: object
      required: [entries, total]
      properties:
        entries: { type: array }
        total: { type: number, multipleOf: 0.01 }
    ContractDetail:
      type: object
      required: [id, name, startDate, endDate, paymentStreams, expense, paymentSchedule, expenseSchedule]
      properties:
        id: { type: string, format: uuid }
        name: { type: string }
        startDate: { type: string, format: date }
        endDate: { type: string, format: date }
        paymentStreams:
          type: array
          items: { $ref: '#/components/schemas/PaymentStream' }
        expense: { $ref: '#/components/schemas/ExpenseInput' }
        paymentSchedule:
          type: object
          required: [entries, total]
          properties:
            entries: { type: array, items: { $ref: '#/components/schemas/PaymentEntry' } }
            total: { type: number, multipleOf: 0.01 }
        expenseSchedule:
          type: object
          required: [entries, total]
          description: Empty entries and a total of 0 when the method is None.
          properties:
            entries: { type: array, items: { $ref: '#/components/schemas/ExpenseEntry' } }
            total: { type: number, multipleOf: 0.01 }
    ContractSummary:
      type: object
      required: [id, name, startDate, endDate]
      properties:
        id: { type: string, format: uuid }
        name: { type: string }
        startDate: { type: string, format: date }
        endDate: { type: string, format: date }
    ContractPage:
      type: object
      required: [items, page, pageSize, totalCount, totalPages]
      properties:
        items: { type: array, items: { $ref: '#/components/schemas/ContractSummary' } }
        page: { type: integer, minimum: 1 }
        pageSize: { type: integer, enum: [10, 20, 50, 100] }
        totalCount: { type: integer, minimum: 0 }
        totalPages: { type: integer, minimum: 0 }
    Problem:
      type: object
      description: RFC 9457 Problem Details
      required: [type, title, status]
      properties:
        type: { type: string }
        title: { type: string }
        status: { type: integer }
        detail: { type: string }
        instance: { type: string }
    ValidationProblem:
      allOf:
        - $ref: '#/components/schemas/Problem'
        - type: object
          required: [errors]
          properties:
            errors:
              type: object
              description: >
                Keys are camelCase field paths (name, endDate, paymentStreams[0].amount, expense.date).
                Values are lists of messages using the interaction spec wording.
              additionalProperties:
                type: array
                items: { type: string }
```

Example validation response:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "name": ["Contract name is required."],
    "paymentStreams[0].amount": ["Amount must be greater than zero."]
  }
}
```

**Ordering and paging rules:**
- **Sort order:** contracts are sorted by `startDate` descending, then `name` ascending, then `id`, so pages stay stable.
- **Total pages:** `totalPages` is `ceil(totalCount / pageSize)`.
- **Failure behavior:** the backend saves nothing unless the whole operation succeeds. The frontend never retries a failed `POST` on its own, to avoid double saves. Dana retries from the failure alert. A `GET` may be retried from the Retry button.

## C2. Schedule Engine Interface (U1 to U2)

U2 calls U1 in-process with already-validated input. U1 has no I/O (ADR-001). The interface is a shared .NET model owned by U1.

```csharp
namespace ContractManagement.Scheduling;

public enum Frequency { Weekly, Monthly, Quarterly, Annually, OneTime }
public enum ExpenseMethod { Overtime, AllAtOnce, None }

public sealed record StreamInput(
    Guid StreamId, decimal Amount, Frequency Frequency, DateOnly StartDate, DateOnly EndDate);

public sealed record ExpenseInput(
    ExpenseMethod Method, DateOnly? StartDate, DateOnly? EndDate, DateOnly? Date);

public sealed record ScheduleRequest(IReadOnlyList<StreamInput> Streams, ExpenseInput Expense);

public sealed record PaymentEntry(Guid StreamId, DateOnly DueDate, decimal Amount);

public sealed record ExpenseEntry(DateOnly PeriodStart, DateOnly PeriodEnd, decimal Amount);

public sealed record ScheduleResult(
    IReadOnlyList<PaymentEntry> Payments, decimal PaymentTotal,
    IReadOnlyList<ExpenseEntry> Expenses, decimal ExpenseTotal);

public interface IScheduleGenerator
{
    /// Pure and deterministic. Payments are ordered by DueDate, then by stream order.
    /// Expenses are ordered by PeriodStart. ExpenseTotal equals PaymentTotal unless Method is None.
    /// Throws ArgumentException if the input breaks a precondition, which means U2 did not validate it.
    ScheduleResult Generate(ScheduleRequest request);
}
```

**Preconditions (U2 guarantees these, U1 checks them and throws on a violation):**
- At least one stream, and every amount is greater than 0.
- Every stream's `EndDate` is on or after its `StartDate`. For `OneTime`, `EndDate` equals `StartDate`.
- `Overtime` has `StartDate` and `EndDate`, with `EndDate` on or after `StartDate`. `AllAtOnce` has `Date`.

## Contract Ownership Rules

- **C1:** U2 owns the specification. U3 builds against this document and tests with a fake API that follows it.
  - Additive changes (a new optional field or a new endpoint) stay in v1. Consumers ignore fields they do not know.
  - A breaking change is one that removes or renames a field, makes a field required, or changes a type or meaning. It needs agreement between the U2 and U3 owners and is released as `/api/v2`, with v1 kept until U3 has moved.
- **C2:** U1 owns the interface. U2 is its only consumer, and both live in the same solution. A change is agreed in the same change set and checked by U2's build and tests.
- **Validation messages:** this document and the interaction spec are the single source for the message wording. Changing the wording is a change to C1.

## Open Questions

| Contract | Question | Blocks |
|---|---|---|
| C1 | The Refined Mockups list screen (S1) has no pager, but paging was chosen here (Q3). Functional Design for U3 adds a pager with a page-size select offering 10, 20, 50 and 100, defaulting to 10. | U3 |
| C1 | The deployed frontend origin for the cross-origin allow list is not known yet. It is configuration, settled in Infrastructure Design. | U2 deployment only |
