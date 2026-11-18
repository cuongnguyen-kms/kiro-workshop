# CI/CD Pipeline: u3-web-balance

There is no CI service or deployment target; the team releases by running the verification script locally and committing to `development` (team.md). Inputs: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/performance-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/logical-components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/domain-design/components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md` and `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

## Pipeline steps

The Unit checks run `scripts/verify-skip-audit.ps1`, which the person approved for this work because the picomatch `npm audit` finding is open and accepted. It runs, in order and stopping at the first failure:

1. .NET restore, build with warnings as errors, all .NET tests (unchanged by U3, must stay green).
2. `npm i` in `web/`.
3. `npm run lint` (`--max-warnings 0`; enforces no `dangerouslySetInnerHTML` and no browser storage).
4. `npm run typecheck` (`tsc -b`; checks the new `BalanceRow` types).
5. `npm test -- --run` (Vitest: formatter, period, table, tab, popup, axe, timing).
6. `npm run build`.
7. `npm run size` (300 KB gzip limit).

| Step | Gate for U3 |
|---|---|
| Lint | 0 warnings |
| Typecheck | 0 errors |
| Web tests | all pass, including the updated existing tests |
| Build and size | build succeeds; under 300 KB |

The unit-scoped command during Code Generation runs only U3's test files with Vitest (named in `unit-test-instructions.md`).

## Manual check

After the build, the person opens the app in a browser and checks by eye: the sticky Period column, the sticky header, sideways and keyboard scrolling, and the popup at up to 80% width (team.md: real-browser checks are not automated).

## Deployment and rollback

No deployment. Rollback is reverting the U3 commit on `development`. U3 depends on U2's `balanceRows`; reverting U3 alone is safe because the API change is additive.

## Secrets

None. `VITE_API_BASE_URL` is a non-secret local setting in `web/.env.development.local`.
