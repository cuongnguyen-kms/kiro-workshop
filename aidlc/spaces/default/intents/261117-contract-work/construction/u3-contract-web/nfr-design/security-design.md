# Security Design: u3-contract-web

## Design Decisions

| Concern | Design | Covers |
|---|---|---|
| No sign-in | The API client sends requests with `credentials: 'omit'` and no `Authorization` header. When sign-in comes later, only the API client module changes. | NFR3.4 |
| Text shown safely | Contract names and messages are rendered through normal React text, which escapes it. An ESLint rule (`react/no-danger`) fails the build if `dangerouslySetInnerHTML` appears, and a code search in the verification step fails on `innerHTML`. | NFR3.5 |
| No browser storage | No use of `localStorage`, `sessionStorage`, `indexedDB` or `document.cookie`. An ESLint `no-restricted-globals` and `no-restricted-properties` rule enforces it. | NFR3.6 |
| No secrets in the bundle | The only configuration value is `VITE_API_BASE_URL`, a public address. `.env` files with real values are listed in `.gitignore`; a committed `.env.example` shows the name only. | NFR3.7 |
| Validation | The Zod schema checks every field before sending. On a 400 the API's `errors` map is shown next to the matching fields, and unknown keys go to the error summary. | NFR6.3 |
| Length limit | The contract name input has `maxLength=200` and the schema rejects longer names. | NFR6.4 |
| Dependencies | `package-lock.json` is committed. The verification step runs `npm audit --omit=dev --audit-level=high` and fails on high or critical findings. | NFR3.8 |
| Security headers | Set by the static host (Content-Security-Policy limiting scripts to the app's own address and `connect-src` to the API address, `X-Content-Type-Options: nosniff`). Configured in this unit's Infrastructure Design. | NFR3.5 |

## Failure Behavior

| Case | Result |
|---|---|
| The API returns HTML or something not JSON | Treated as a failure: the generic failure alert, no raw response shown |
| A network error | The same failure alert, with entries kept on the form |
