# CI/CD Pipeline: u2-contract-api

The requirements name no repository host or deployment target, so this design is the pipeline's shape. It runs the same commands a developer runs locally and can be written for any CI service later.

## Stages

| Stage | Command | Gate |
|---|---|---|
| 1. Restore and build | `dotnet restore`, `dotnet build --no-restore -warnaserror` | No build errors or warnings |
| 2. Unit tests | `dotnet test` on the unit test project (fake generator, fake repository) | All pass |
| 3. Integration tests | `dotnet test` on the integration project, against a PostgreSQL service with database `contract_management_test` | All pass; includes the atomic-save and 2-second create tests |
| 4. Contract check | Test that compares the generated OpenAPI document with the Contract Summary | No difference in paths, shapes or messages |
| 5. Package | `dotnet publish -c Release` | Produces the publish folder |

## Deployment strategy

- No deployment stage in this version. The publish folder is the artifact. When a hosting target is chosen, a rolling replace of the single process is enough; a new version runs the database migrations first.
- Rollback: redeploy the previous publish folder. Migrations only add columns or tables within v1, so the previous version keeps working against the newer schema.
- Environment promotion: one environment for now.

## Secrets in CI

- The CI database password is a CI secret passed as an environment variable. It is never printed and never committed.
- The local development password lives only in the developer's user secrets.
