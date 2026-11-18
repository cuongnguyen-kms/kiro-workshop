# CI/CD Pipeline: u1-schedule-balance

U1 changes only the `ContractManagement.Scheduling` class library, so it needs no infrastructure. The design inputs are `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/nfr-design/security-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/nfr-design/logical-components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/domain-design/components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u1-schedule-balance/functional-design/functional-spec.md` and `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`. The library ships inside the API process. There is no server, database, queue, cache or secret to add. The team has no CI service or deployment target: it releases by running `scripts/verify.ps1` locally and committing to `development` (`team.md`).

## Pipeline steps

These are the steps of the existing `scripts/verify.ps1`, run locally before committing:

1. `dotnet restore`
2. `dotnet build -warnaserror`. This step enforces nullable checks, XML docs and the Scheduling banned-symbols analyzer (no I/O, SEC3).
3. `dotnet test`. It runs the new balance builder tests in `tests/ContractManagement.Scheduling.Tests` along with every existing .NET test (NFR2.1, NFR3.1 to NFR3.3, NFR1.1).
4. The web steps (`npm i`, audit, lint, typecheck, test, build, size). U1 does not change the web code. The `npm audit` picomatch finding is still open and was accepted earlier.

| Step | Gate for U1 |
|---|---|
| Build | Zero warnings. |
| .NET tests | All pass, including E1 to E10, the BR2.4 branches, the BR2.5 signs and the timing test. |
| Web steps | Unchanged by U1. The accepted npm audit finding is the only known failure. |

The unit-scoped test command during Code Generation is `dotnet test tests/ContractManagement.Scheduling.Tests`.

## Deployment and rollback

U1 has no deployment of its own. It reaches users when U2's API is built and started. Rollback means reverting the U1 commit on `development`. No stored data or schema changes, so a revert has no data step.

## Secrets

None. U1 reads no configuration.
