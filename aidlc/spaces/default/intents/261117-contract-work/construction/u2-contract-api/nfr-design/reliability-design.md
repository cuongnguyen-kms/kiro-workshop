# Reliability Design: u2-contract-api

| Area | Design | Serves |
|---|---|---|
| Atomic save | `ContractRepository.Save` opens one EF Core transaction (explicit `BeginTransaction`), adds the Contract with streams and both schedules, calls `SaveChanges`, then commits. Any exception rolls back by disposing the transaction without commit. | NFR4.1 |
| Failure injection test | The repository takes its context from a factory; the integration tests use a database trigger or a deliberately invalid last row to make the fourth write fail and then assert the tables are empty. | NFR4.1 |
| Deterministic schedules | U2 never generates dates or amounts. It passes validated input to U1 and stores the result unchanged; the stream order and ids passed to U1 are fixed by request order. | NFR4.2 |
| Exact money | `decimal` in C#; `numeric(18,2)` columns; JSON numbers are read as `decimal`, not `double`. Totals are summed from stored rows when read. | NFR4.3 |
| Failure responses | An exception handler returns 500 Problem Details with the trace id. No retry on the server and none on the client for POST (contract C1). No circuit breaker or bulkhead: there is one dependency (the database) and nothing to fall back to. | NFR4.4 |
| Startup check | In Development, apply migrations at start, creating the database if missing. Elsewhere, only check connectivity. A failure stops the process with a log line naming host and database, never the password. | NFR4.5 |
| Backup and recovery | Not built. The database owner backs up PostgreSQL. | NFR4.6 |
| Health | `GET /health` runs `SELECT 1` with a 2 second timeout: 200 if it answers, 503 otherwise. It sits outside `/api/v1`. | NFR4.7, NFR4.8 |
| API description | The OpenAPI document is generated in Development at `/openapi/v1.json` and compared with the Contract Summary by a test. | NFR4.9 |
