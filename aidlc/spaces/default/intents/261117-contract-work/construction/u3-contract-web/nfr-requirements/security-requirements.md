# Security Requirements: u3-contract-web

A browser app with no sign-in in this version. Its risks are showing untrusted text safely and not leaking data.

| ID | Requirement | Inception NFR | Notes |
|---|---|---|---|
| NFR3.4 | The app contains no sign-in, token or session code. The API is called without credentials. | NFR3 | Authentication can be added later at the API client layer without changing screens. |
| NFR3.5 | Text that comes from the API (contract names, error messages) is always shown as plain text. The app never uses `dangerouslySetInnerHTML`, `innerHTML` or any other way to turn text into markup. | NFR3, NFR6 | A contract name is entered by a user, so it must not run as script when shown on the list and detail screens. |
| NFR3.6 | The app stores nothing in the browser (no local storage, session storage or cookies) for contracts or entries. | NFR2, NFR3 | A refresh clears an unsaved form. That is accepted in this version. |
| NFR3.7 | The API base address is configuration (`VITE_API_BASE_URL`). The built app contains no secrets, keys or connection strings. | NFR3 | Anything in a browser bundle is public. |
| NFR6.3 | Client-side checks mirror the backend rules and messages but are a convenience. The app always shows the backend's own validation messages for the fields they name. | NFR6 | See functional-spec.md, validation. |
| NFR6.4 | Input length is limited on the client to match the API (contract name at most 200 characters). | NFR6 | Same limit as contract C1. |
| NFR3.8 | Dependencies are pinned by a lock file, and the verification command fails when `npm audit` reports a high or critical vulnerability in a production dependency. | NFR3 | Keeps known vulnerable packages out of the shipped bundle. |

## Threat Considerations

| Threat | Handling |
|---|---|
| Script injection through a contract name | NFR3.5 |
| Leaking data through browser storage | NFR3.6 |
| Secrets in the bundle | NFR3.7 |
| Vulnerable packages | NFR3.8 |
| Cross-origin calls | The API allows only the configured frontend address (contract C1); the app sends no cookies |
