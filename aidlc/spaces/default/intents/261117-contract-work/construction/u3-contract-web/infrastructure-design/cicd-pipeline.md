# Delivery Pipeline: u3-contract-web

There is no CI server in this plan. The same steps run locally through `scripts/verify.ps1`, the verification command recorded for every unit. This unit adds a frontend section to that script.

## Stages

| Stage | Command (run in `web/`) | Gate to pass |
|---|---|---|
| 1. Install | `npm ci` | Installs exactly what `package-lock.json` pins |
| 2. Dependency audit | `npm audit --omit=dev --audit-level=high` | No high or critical vulnerability in production packages (NFR3.8) |
| 3. Lint and types | `npm run lint` and `npm run typecheck` | No errors, including the rules banning `dangerouslySetInnerHTML` and browser storage (NFR3.5, NFR3.6, NFR7.5) |
| 4. Tests | `npm test` (Vitest, single run) | All component tests and the axe accessibility checks pass (NFR7.3, NFR7.4) |
| 5. Build | `npm run build` | Build succeeds |
| 6. Size check | `npm run size` | The list screen's JavaScript is under 300 KB compressed (NFR5.5) |

`scripts/verify.ps1` runs these after the .NET steps and stops at the first failure. If `npm` is not found, it fails with a message saying Node.js is needed, rather than skipping the frontend.

## Deployment

Not part of this plan. When a static host is chosen:
- Build with `VITE_API_BASE_URL` set for that environment.
- Upload `web/dist` to the host. Hashed asset files are uploaded before `index.html`, so a visitor never gets an `index.html` that points at missing files.
- Rollback: upload the previous build's `web/dist` again. The app holds no data, so a rollback loses nothing.

## Secrets

None. `VITE_API_BASE_URL` is a public address, not a secret.
