# Build Instructions

Same prerequisites and steps as the existing project (see `aidlc/spaces/default/intents/261117-contract-work/construction/build-and-test/build-instructions.md`). This change adds no package, configuration key or database change.

## Prerequisites

- .NET 10 SDK, local PostgreSQL (`localhost:5432`, user `postgres`), the password in user secrets (`ConnectionStrings:ContractDbPassword`) for the API and the integration test project.
- Node.js LTS with npm, for `web/`.

## Build and verify

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1
```

It runs `dotnet restore`, `dotnet build --no-restore -warnaserror`, `dotnet test --no-build`, then in `web/`: `npm i`, `npm audit --omit=dev --audit-level=high`, `npm run lint`, `npm run typecheck`, `npm test -- --run`, `npm run build`, `npm run size`.

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| `dotnet build` cannot copy `ContractManagement.Api.dll` | The API is running from `dotnet run` and locks its files. Stop it, build, start it again. |
| `npm audit` stops `verify.ps1` | A high advisory in a production dependency. See `test-results.md` for the current finding. |
