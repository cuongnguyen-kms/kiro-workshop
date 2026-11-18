# Monitoring Design: u3-web-balance

The app has no deployed environment or monitoring stack (team.md), and observability was skipped for earlier work for the same reason. U3 adds no metrics, logs or alerts. Its quality signals are the checks in the verification script. Inputs: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/performance-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/logical-components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/domain-design/components.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md` and `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

## Metrics and KPIs

| Metric | Source | Threshold | Why it matters |
|---|---|---|---|
| Web tests passing | `npm test -- --run` | all pass | Formatter, table, tab and popup behaviour (NFR3.1, NFR3.2, NFR4.1) |
| Expense tab render time in jsdom | timing test | under 200 ms, lenient | Guards against a slowdown (NFR1.1) |
| Bundle size (gzip) | `npm run size` | under 300 KB | NFR1.3 |
| Lint warnings | `npm run lint` | 0 | Rendering safety rules (SEC1) |

## Alerts

| Alert | Condition | Severity | Routes to |
|---|---|---|---|
| Verification failed | the verification script exits non-zero | blocking | the developer running it |

## SLIs and SLOs

| SLI | SLO target | Measurement window |
|---|---|---|
| Not applicable | no deployed service | none |

## Logs and tracing

None added. A failed API call keeps today's on-screen error and retry.
