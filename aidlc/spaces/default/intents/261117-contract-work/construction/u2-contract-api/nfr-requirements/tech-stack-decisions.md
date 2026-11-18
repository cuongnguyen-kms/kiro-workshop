# Technology Stack Decisions: u2-contract-api

| ID | Area | Decision | Why | Alternative not chosen |
|---|---|---|---|---|
| NFR1.1 | Runtime and language | .NET 10, C# | Required by the request (NFR1) | none |
| NFR1.2 | Web framework | ASP.NET Core minimal APIs, with endpoints grouped under `/api/v1` | Small API of three endpoints; one route group is also the place to add authentication later (NFR3.2) | Controllers: more files for three endpoints |
| NFR2.3 | Database | PostgreSQL, local install, database `contract_management` (Q3) | Required (NFR2) | none |
| NFR2.4 | Data access | Entity Framework Core 10 with the Npgsql provider; schema from EF Core migrations, applied at start in development only | Transactions, parameterized queries, and numeric(18,2) columns come built in; migrations keep the schema in source | Dapper with hand-written SQL: more code and more room for injection mistakes |
| NFR6.11 | Validation | Plain C# validator classes returning field path and message pairs | The rules and messages are fixed and few (BR1.x to BR5.4); no library needed | FluentValidation: a new dependency for about 20 rules |
| NFR6.12 | Errors | Built-in Problem Details (RFC 9457), with a custom validation response matching C1 | C1 requires it | none |
| NFR7.1 | Tests | xUnit. Unit tests for the validators and service use a fake `IScheduleGenerator` and a fake repository. Integration tests run against the local PostgreSQL in a separate database `contract_management_test`, created and emptied by the test run | The service tests need neither U1 nor a database; integration tests prove the transaction behaviour on the real database. No Docker needed | Testcontainers: needs Docker, not installed here |
| NFR2.5 | Configuration | Standard .NET configuration: `appsettings.json` (no password), user secrets in development, environment variables elsewhere. Keys: `ConnectionStrings:ContractDb` parts and `Cors:AllowedOrigin` | NFR2.1 | none |
| NFR1.3 | Schedule engine | Project reference to `u1-schedule-engine` (C2) | Same solution, in-process | none |
| NFR4.9 | API description | The OpenAPI document in the Contract Summary is the reference; the API serves its own generated OpenAPI document in development | Lets U3's fake and U2 be compared | none |

Infrastructure (hosting, containers, pipelines) is out of scope here; it belongs to the infrastructure stages if the scope includes them.
