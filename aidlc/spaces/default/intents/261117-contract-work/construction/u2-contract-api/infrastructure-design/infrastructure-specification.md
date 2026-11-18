# Infrastructure Specification: u2-contract-api

No questions were needed. The requirements name no cloud or hosting target, and the PostgreSQL install on your machine is the one database in use, so this design covers local development and leaves a hosted environment open. No AWS service is chosen because nothing in the requirements asks for one.

## Deployment

| Facet | Choice | Rationale |
|---|---|---|
| Compute model | One .NET 10 web process (Kestrel), run with `dotnet run` | Single small API; Units Generation chose a standalone deployment |
| Networking | API listens on `https://localhost:7080` and `http://localhost:5080` in development; frontend origin set in `Cors:AllowedOrigin` (for example `http://localhost:5173`) | The frontend is deployed separately (Q2) and needs a configured origin |
| Storage | Local PostgreSQL, database `contract_management`, user `postgres`, host `localhost`, port `5432` | Q3 (A); installed already |
| Environments | `Development` (local) only in this version. Other environments use the same configuration keys with their own values | Parity by configuration, not code |
| IaC approach | None. No cloud resources are created | No hosting target is stated |
| Sizing | Default .NET and PostgreSQL settings; connection pool maximum 20 | NFR5.1, NFR5.7 |
| Secrets | Development: user secrets store the connection password. Other environments: environment variable | NFR2.1 |
| Database setup | Development: EF Core migrations run at start and create the database if missing. Other environments: migrations are run by the deployer before start | BR6.2, NFR4.5 |

## Infrastructure Services

| Service | Role | Configuration | Notes |
|---|---|---|---|
| PostgreSQL | database | One database `contract_management`; tables for contracts, payment streams, payment schedule entries, expense schedule entries; money as `numeric(18,2)`; indexes from the scalability design; a second database `contract_management_test` for integration tests | Backup is the database owner's job (NFR4.6) |
| None | cache, queue, search, CDN, DNS, load balancer | Not used | No caching (performance design); no async work; one instance |

## Shared Infrastructure

| Shared Resource | Owner Unit | Consumer Units | Access Boundary |
|---|---|---|---|
| PostgreSQL `contract_management` | u2-contract-api | none (u1 has no storage; u3 reaches data only through the API) | Only u2's repository connects |
| Configuration key `Cors:AllowedOrigin` | u2-contract-api | u3-contract-web (its address must match this value) | Set per environment |

## Configuration keys

| Key | Where it lives | Example |
|---|---|---|
| `ConnectionStrings:ContractDb` | appsettings (host, port, database, user, no password) plus user secrets or environment for the password | host and database as above |
| `Cors:AllowedOrigin` | appsettings per environment | `http://localhost:5173` |
