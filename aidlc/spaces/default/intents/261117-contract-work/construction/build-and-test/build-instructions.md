# Build Instructions

## Prerequisites

| Tool | Version | Used by |
|---|---|---|
| .NET SDK | 10 | u1-schedule-engine, u2-contract-api |
| PostgreSQL | local install, `localhost:5432`, user `postgres` | u2-contract-api (app and integration tests) |
| Node.js with npm | current LTS (20 or later) | u3-contract-web |

## Environment setup

1. Database password, set once per machine. It is never written to a file in the project:
   ```powershell
   dotnet user-secrets set "ConnectionStrings:ContractDbPassword" "<your local postgres password>" --project src/ContractManagement.Api
   dotnet user-secrets set "ConnectionStrings:ContractDbPassword" "<your local postgres password>" --project tests/ContractManagement.Api.IntegrationTests
   ```
   Instead of user secrets, the environment variable `ConnectionStrings__ContractDbPassword` works too.
2. Databases: `contract_management` (the app) and `contract_management_test` (integration tests) are created by migrations when missing. Nothing to create by hand.
3. Frontend address: `Cors:AllowedOrigin` in `src/ContractManagement.Api/appsettings.Development.json` is `http://localhost:5173`. The frontend reads the API address from `VITE_API_BASE_URL` (see `web/.env.example`).

## Build

```powershell
dotnet restore
dotnet build --no-restore -warnaserror
```

Frontend, in `web/`:

```powershell
npm i
npm run lint
npm run typecheck
npm run build
npm run size
```

## Build verification

The recorded verification command runs every build and test step above and stops at the first failure:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1
```

## Run locally

```powershell
dotnet run --project src/ContractManagement.Api
```

Then in `web/`: `npm run dev`.

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| `dotnet` or `npm` not found | The terminal was opened before the install. Reopen it; `verify.ps1` also falls back to the default install folders. |
| Integration tests fail naming `ConnectionStrings:ContractDbPassword` | The password secret is not set. Run step 1. |
| API stops at start with a database connection error | PostgreSQL is not running, or the host, port or user differ from `appsettings.json`. |
| `dotnet build` with two project paths fails | The CLI builds one project per call. Build each project, or the whole solution. |
| Vitest prints `HTMLCanvasElement's getContext()` | jsdom has no canvas. The message is harmless; the tests still pass. |
