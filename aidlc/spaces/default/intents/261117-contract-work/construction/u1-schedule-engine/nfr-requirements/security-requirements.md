# Security Requirements: u1-schedule-engine

This unit is a .NET class library with no network, file or database access. It has no users and no endpoints, so most security concerns belong to the backend (U2) that calls it.

| ID | Requirement | Inception NFR | Notes |
|---|---|---|---|
| NFR3.1 | The library contains no authentication or authorization code and makes no assumption about who the caller is. | NFR3 | No sign-in in this version. U2 may add it later without changing this unit. |
| NFR3.2 | The library does no input or output of any kind: no files, network, environment variables, database, logging of input values, or clock reads. | NFR3, NFR4 | Keeps it free of secrets and side effects. Also supports BR1.7 (same input, same output). |
| NFR6.1 | The library checks its own input promises (BR1.8) and stops with an argument error that names the broken promise. It never returns a partial result for bad input. | NFR6 | The argument error message names the rule but does not echo free text, since this unit takes none. |
| NFR6.2 | The library bounds its work. A request whose payment dates would exceed 100,000 entries in total is rejected with an argument error instead of running. | NFR6 | Stops a mistaken input (for example a Weekly stream across thousands of years) from using all the machine's memory. U2 normally prevents this with its own validation; this is a second guard. |

## Threat Considerations

| Threat | Applies? | Handling |
|---|---|---|
| Untrusted input | Yes, indirectly | The caller (U2) validates first; the library re-checks (NFR6.1, NFR6.2). |
| Secrets in code | No | The library has no configuration or credentials. |
| Dependency vulnerabilities | Minimal | No third-party packages in the library (see tech-stack-decisions.md). |
| Denial of service by huge ranges | Yes | NFR6.2. |
