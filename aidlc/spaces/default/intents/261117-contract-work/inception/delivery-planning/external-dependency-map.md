# External Dependency Map

A Bolt is one build pass over one or more units that ends in something that runs and is tested. The work is built entirely by this team and the AI developer agent, so the only outside items are local tools and one deployment setting.

| Item | Owner | Lead time | Blocks | If it slips |
|---|---|---|---|---|
| Local PostgreSQL install with a database and a connection string for the backend tests (Q3) | You (the requester) | Under an hour | Bolt 1 integration tests and the verification command | U1 unit tests still run. U2 integration tests cannot pass until it is available. |
| .NET 10 SDK on the build machine | You | Under an hour | Bolt 1 | Nothing in Bolt 1 builds without it |
| Node.js (current LTS) for the React app | You | Under an hour | Bolt 2 | Nothing in Bolt 2 builds without it |
| Deployed frontend address for the API's cross-origin allow list | You, in Infrastructure Design | Settled in Infrastructure Design | Deployment only, not local development | The API allows a local development origin until it is set |
