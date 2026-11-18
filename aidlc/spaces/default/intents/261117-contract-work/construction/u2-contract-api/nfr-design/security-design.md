# Security Design: u2-contract-api

| Area | Design | Serves |
|---|---|---|
| Open access, ready for sign-in | No authentication now. All three endpoints live in one route group (`/api/v1`). Adding sign-in later means adding an authorization requirement to that group and an authentication scheme in startup; the data model and request and response shapes do not change. | NFR3.1, NFR3.2 |
| Input validation | Each request is parsed into a request model, then checked by validator classes that return field path and message pairs. Every BR1.x to BR3.x rule runs on the server and messages are collected, never thrown one at a time. Unknown JSON properties are ignored; wrong types return 400. | NFR6.1 |
| Safe queries | EF Core generates parameterized SQL. Raw SQL is not used. | NFR6.2 |
| Body size limit | Kestrel `MaxRequestBodySize` set to 256 KB; larger bodies get 413. | NFR6.3 |
| Error exposure | One exception handler converts any unhandled exception to a generic 500 Problem Details with a trace id and no detail. Development detail pages are off. | NFR6.4 |
| Transport | HTTPS redirection and HSTS enabled outside Development. No cookies are set or read. | NFR6.5 |
| CORS | One named policy built from `Cors:AllowedOrigin`: that origin only, methods GET and POST, header Content-Type. An empty setting allows no origin. | NFR6.6 |
| Secrets | `appsettings.json` holds host, port, database and user only. The password comes from user secrets (`dotnet user-secrets`, key `ConnectionStrings:ContractDb:Password` style or a full connection string) in development and from an environment variable elsewhere. The connection string is assembled at start; logging never prints it. | NFR2.1 |
| Database account | Development uses the local `postgres` account because that is what is installed. Other environments use a limited account created by the deployer with rights on the API's tables only. | NFR2.2 |
| Security headers | `X-Content-Type-Options: nosniff` and `Cache-Control: no-store` on API responses. The API serves JSON only, so no content security policy is needed. | NFR6.4 |

The development password (`postgres`) is held by the developer in user secrets on their machine. It is not recorded in any file of the project.
