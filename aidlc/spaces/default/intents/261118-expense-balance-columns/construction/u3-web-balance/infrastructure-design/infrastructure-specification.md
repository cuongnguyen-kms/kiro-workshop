# Infrastructure Specification: u3-web-balance

U3 changes only browser code in `web/`. It adds no server, service, storage or secret. Inputs: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/performance-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/logical-components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/domain-design/components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md` and `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

## Deployment

| Facet | Choice | Rationale |
|---|---|---|
| Compute model | Static single-page app built by Vite, served by the Vite dev server locally | Unchanged; the team has no deployment target (team.md). |
| Networking | Browser calls the API at `VITE_API_BASE_URL` (local `http://localhost:5080`), CORS as today | Unchanged; no new endpoint. |
| Storage | None | U3 stores nothing; browser storage is banned by ESLint. |
| Environments | Local development only | team.md: release is a local run of the verification script. |
| IaC approach | None | Nothing to provision. |
| Resource sizing | Bundle under the 300 KB gzip limit for the list screen | NFR1.3; checked by `npm run size`. |

## Infrastructure Services

| Service | Role | Configuration | Notes |
|---|---|---|---|
| Contract API (U2) | API | C1 v1.1.0, `balanceRows` in the expense schedule | Consumed only; no change from U3. |
