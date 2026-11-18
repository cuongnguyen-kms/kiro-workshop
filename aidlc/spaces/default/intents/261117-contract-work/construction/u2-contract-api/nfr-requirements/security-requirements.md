# Security Requirements: u2-contract-api

## Authentication and authorization

| ID | Requirement | Source |
|---|---|---|
| NFR3.1 | No sign-in in this version: every endpoint is open. | NFR3 |
| NFR3.2 | Authentication can be added later without changing the contract data model or the C1 request and response shapes. Endpoints are registered in one route group, so an authorization requirement can be added to the group in one place. No user id column is added now. | NFR3 |

## Data protection and input handling

| ID | Requirement | Source |
|---|---|---|
| NFR6.1 | Every rule in Functional Design BR1.x to BR3.x is enforced on the server; the frontend checks are a convenience. Invalid input returns 400 with the agreed wording and saves nothing. | NFR6 |
| NFR6.2 | All database access uses parameterized queries (through the ORM). No SQL is built by joining strings with request values. | NFR6 |
| NFR6.3 | Request bodies are limited to 256 KB. A larger body is rejected with 413 before it is read in full. | NFR6, BR5.4 |
| NFR6.4 | Error responses use Problem Details and never include stack traces, SQL text, connection strings or file paths (BR6.3). | NFR6 |
| NFR6.5 | The API accepts HTTPS in every environment other than local development, and sends no cookies. | NFR6 |
| NFR6.6 | Cross-origin calls are allowed only from the configured frontend origin, for GET and POST with the Content-Type header (BR6.1). | NFR6, C1 |

## Secrets

| ID | Requirement | Source |
|---|---|---|
| NFR2.1 | The database password is never in source, design files, test files or logs. It comes from user secrets in development or an environment variable elsewhere. A committed settings file holds no password. | NFR2, Q3, Construction security rules |
| NFR2.2 | The database user in non-development environments has only the rights the API needs (read and write its own tables); the `postgres` superuser with the local development password is for local development only. | NFR2 |

## Threat considerations

- The contract data is business data but holds no personal data and no payment card data; no compliance regime applies in this version.
- Denial of service by oversized contracts is limited by BR5.4 (10,000 rows) and NFR6.3 (256 KB body).
- Duplicate or rapid saves create duplicate contracts, which is allowed (BR1.3).
