# Reliability Requirements: u2-contract-api

| ID | Requirement | Target | Source |
|---|---|---|---|
| NFR4.1 | Atomic save | A contract, its streams and both schedules are stored in one transaction. After any failure, including a failure injected in a test after each of the four writes, no part of the contract exists | NFR4, FR6.1, BR5.1 |
| NFR4.2 | Deterministic output | The same valid input gives identical schedules and identical totals on every run, on every machine | NFR4, BR4.1 |
| NFR4.3 | Exact totals | Money uses a fixed-precision decimal type end to end, including the database column (numeric with 2 decimal places). Totals read back equal the totals returned on save | NFR4, BR4.2 |
| NFR4.4 | Failure behaviour | A database outage or an engine error returns 500 Problem Details, stores nothing, and the next request works once the cause is gone. No automatic retry of a save | FR6.1, BR6.3 |
| NFR4.5 | Startup | If the database cannot be reached at start, the API fails to start with a log line that names the host and database but not the password | BR6.2 |
| NFR4.6 | Durability | Saved contracts are as durable as the PostgreSQL installation. Backup and recovery are the database owner's responsibility; no backup tooling is built in this version | NFR2 |
| NFR4.7 | Availability | No availability target (SLA or SLO) in this version: a development-scale deployment. A health endpoint (`/health`) reports whether the API and its database answer | NFR2 |
