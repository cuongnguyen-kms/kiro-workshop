# Logical Components: u2-contract-api

One deployable process (the API) and one database. This view tells Infrastructure Design what exists and where the NFR patterns apply.

| Logical component | Kind | Where it runs | NFR patterns applied |
|---|---|---|---|
| ContractApi (endpoints, exception handler, CORS, body limit, health) | in-process module | API process | request validation shape, Problem Details, CORS, security headers, request logging, `/health` |
| Validators | in-process module | API process | server-side rule enforcement, field path messages |
| ContractService | in-process module | API process | size check, orchestration, one create operation |
| ScheduleGenerator (U1 library) | in-process library | API process | deterministic generation, no I/O |
| ContractRepository (EF Core + Npgsql) | in-process module | API process | one transaction per save, batched writes, split queries, pooled connections |
| PostgreSQL `contract_management` | shared resource | database server | `numeric(18,2)` money, indexes, durability by database owner |
| Configuration (appsettings, user secrets, environment) | shared resource | API process start | secrets handling, CORS origin, connection settings |

## Failure domains and blast radius

| Failure | Affected | Not affected |
|---|---|---|
| API process stops | All three endpoints, so the frontend shows its failure states | Stored data |
| Database down | Create, list and read return 500 (`/health` returns 503) | Process stays up and recovers when the database returns |
| Schedule engine defect | Create returns 500 and stores nothing | List and read |
| Frontend origin changed in configuration | Browser calls from other origins are blocked | The API itself |

## Shared resources

- The database connection pool (maximum 20) is shared by all requests.
- No shared files, queues or caches.
