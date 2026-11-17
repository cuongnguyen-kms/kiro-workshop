# Infrastructure Specification: u3-contract-web

The frontend is static files (HTML, JavaScript, CSS) built by Vite into `web/dist`. It needs a static web host and nothing else. The classic plan has no deployment stage, and the requester has not chosen a hosting provider, so this design names what any host must provide and leaves the provider open.

## Deployment

| Facet | Choice | Rationale |
|---|---|---|
| Compute model | Static files only, no server code | The app runs in the browser (Units Generation Q2: separate deployables). |
| Local development | Vite dev server at `http://localhost:5173` | The Vite default. The backend's cross-origin allow list includes this address in development. |
| Production host | Any static host that can set response headers and serve `index.html` for unknown paths (for example Azure Static Web Apps, AWS S3 with CloudFront, Netlify, or nginx) | Not chosen yet. An open item for the requester. |
| Networking | HTTPS only in production. The browser calls the API directly at `VITE_API_BASE_URL`. | Contract C1 is HTTPS JSON and cross-origin. |
| Storage | None | The app stores nothing (NFR3.6). |
| Environments | Development (local) now. Staging and production when a host is chosen, each with its own `VITE_API_BASE_URL` set at build time. | Org default: deploy on merge to staging, production behind manual approval. |
| Infrastructure as code | None yet. Added when a host is chosen. | Nothing to provision locally. |
| Sizing | Not applicable for static files. | |

## Infrastructure Services

| Service | Role | Configuration | Notes |
|---|---|---|---|
| Static web host | cdn | Serve `web/dist`; fallback to `index.html` for client routes such as `/contracts/123`; long cache for hashed asset files, no cache for `index.html` | Routes are client-side (React Router). |
| Response headers | security | `Content-Security-Policy: default-src 'self'; connect-src 'self' <API address>; img-src 'self' data:; style-src 'self' 'unsafe-inline'`; `X-Content-Type-Options: nosniff`; `Referrer-Policy: no-referrer` | Implements NFR3.5. `'unsafe-inline'` for styles only, needed by the Radix popovers. The Vite dev server does not set these locally. |

## Shared Infrastructure

| Shared Resource | Owner Unit | Consumer Units | Access Boundary |
|---|---|---|---|
| The API address and its cross-origin allow list | u2-contract-api | u3-contract-web | u2 allows only the configured frontend address (contract C1). u3 learns the API address from `VITE_API_BASE_URL` at build time. |

## Open Items

| Item | Owner | Blocks |
|---|---|---|
| Choose the production static host | Requester | Production deployment only. Not needed for this plan's Construction. |
| The production frontend address, for u2's allow list | Requester, with the host choice | Production deployment only |
