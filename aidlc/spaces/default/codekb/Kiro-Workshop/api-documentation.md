# API Documentation

## External API (contract C1, base `/api/v1`)
Defined in `src/ContractManagement.Api/Endpoints/ContractEndpoints.cs`. No authentication. Responses carry `nosniff` and `Cache-Control: no-store`. Errors are Problem Details; unhandled errors return 500 with a `traceId`.

| Method and path | Name | Success | Errors |
|---|---|---|---|
| `POST /contracts` | createContract | 201 `ContractDetailResponse`, `Location` header | 400 validation problem (`errors` keyed by camelCase field path, for example `paymentStreams[0].amount`); 400 or 413 unreadable or too large body |
| `POST /contracts/preview?kind=payment\|expense` | previewContractSchedule | 200 `SchedulePreviewResponse` (nothing stored) | 400 validation problem. Absent or unknown `kind` means expense; `payment` ignores the expense fields; name not needed |
| `GET /contracts?page&pageSize` | listContracts | 200 `ContractPageResponse` | 400 (page below 1, pageSize not 10, 20, 50 or 100) |
| `GET /contracts/{id}` | getContract | 200 `ContractDetailResponse` | 404 "Contract not found." |
| `GET /health` | | 200 `{status:"ok"}` | 503 when the database does not answer within 2 seconds |

OpenAPI is mapped in Development only. Body limit: 256 KB.

### Create request (`Requests.cs`)
`{ name, startDate, endDate, paymentStreams: [{ amount, frequency, startDate, endDate }], expense: { method, startDate, endDate, date } }`. Every field is nullable so all errors are reported together. Dates are `yyyy-MM-dd`; amounts are JSON numbers read as decimal; enums are exact-spelling strings (frequency: Weekly, Monthly, Quarterly, Annually, OneTime; method: Overtime, AllAtOnce, None). The preview request is the same shape (the web type omits `name`).

### Validation messages (`ContractValidator.cs`)
Name required or over 200 characters; valid dates; end not before start; at least one stream; amount above zero with at most 2 decimals; frequency and method chosen; stream and expense dates within the contract dates; one-time streams ignore their end date; with method None the other expense fields are ignored. A contract estimated over 10,000 rows is refused under key `paymentStreams` (`SizeEstimator.TooLarge`).

### Response shapes (`Responses.cs`, `Preview.cs`)
`ContractDetailResponse { id, name, startDate, endDate, paymentStreams[{id, amount, frequency, startDate, endDate}], expense{method, startDate?, endDate?, date?}, paymentSchedule{entries[{streamId, dueDate, amount}], total}, expenseSchedule{entries[{periodStart, periodEnd, amount}], total} }`. Totals are summed from rows by `ContractMapper`. Nulls are omitted from JSON.
`SchedulePreviewResponse { paymentSchedule{entries[{streamNumber, dueDate, amount}], total}, expenseSchedule{entries[{periodStart, periodEnd, amount}], total} }`. The expense part reuses `ExpenseScheduleResponse`/`ExpenseEntryResponse`, so a new expense row field added there reaches both saved and preview responses. Web mirrors: `ContractDetail`, `ExpenseEntry`, `Schedule<T>`, `SchedulePreview` in `web/src/api/types.ts`.

## Internal API (contract C2, `Models.cs`)
`IScheduleGenerator.Generate(ScheduleRequest(Streams: StreamInput[], Expense: ExpenseInput)) : ScheduleResult(Payments, PaymentTotal, Expenses, ExpenseTotal)`; `PaymentEntry(StreamId, DueDate, Amount)`, `ExpenseEntry(PeriodStart, PeriodEnd, Amount)`. Pure and deterministic; payments ordered by due date then stream order; expenses by period start. Throws `ArgumentException` for broken preconditions (no streams, bad amounts or dates, missing expense dates, more than 100,000 entries). Registered as a singleton.

## Internal services
- `ContractService.CreateAsync(CreateContractRequest?)` returns `CreateContractResult(Contract?, Errors?)`; `Preview(request, PreviewKind)` returns `PreviewResult(Preview?, Errors?)`; also `GetAsync`, `ListAsync`.
- `IContractRepository`: `SaveAsync`, `GetAsync`, `ListAsync`.

## Frontend client (`web/src/api/contracts.ts`)
`createContract(input, signal?)`, `previewSchedule(input, kind, signal?)`, `listContracts(page, pageSize, signal?)`, `getContract(id, signal?)`, each returning a typed result union (`types.ts`) and never throwing. Base address from `VITE_API_BASE_URL`; no credentials; no retries.
