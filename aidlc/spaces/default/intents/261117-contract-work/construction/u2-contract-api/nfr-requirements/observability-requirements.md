# Observability Requirements: u2-contract-api

Sized for a development-scale service. No dashboards, tracing backend or alerting are built in this version.

| ID | Requirement | Source |
|---|---|---|
| NFR6.7 | Structured logs through the standard .NET logging, one line per request with method, path, status and duration. | NFR6 |
| NFR6.8 | Log a contract id, never the request body, on save and on failure. Names, amounts and the password never appear in logs. | NFR6, NFR2.1 |
| NFR6.9 | Unhandled errors are logged once at error level with the stack trace and a correlation id (the request trace id). The same id is returned in the 500 response as the `traceId` field so a user report can be matched to the log. | BR6.3 |
| NFR6.10 | Validation failures are logged at information level with the field paths only, not the values. | NFR6 |
| NFR4.8 | `/health` returns 200 when the API runs and the database answers a trivial query, and 503 otherwise. It is not part of the C1 contract. | NFR4.7 |

## Indicators (not alerted on in this version)

| Indicator | Definition | Target for later use |
|---|---|---|
| Create latency | 95th percentile of POST /contracts duration | 2 seconds (NFR5.1) |
| Error rate | Share of requests answered with 5xx | Below 1% |
