# Monitoring Design: u2-contract-api

Implements the observability design at development scale: logs and a health endpoint, with no collector, dashboard or paging.

## Metrics and KPIs

| Metric | Source | Threshold | Why it matters |
|---|---|---|---|
| Create latency, 95th percentile | request log duration for POST `/api/v1/contracts` | 2 seconds | NFR5.1 |
| 5xx share of requests | request log status | below 1% | NFR4.4 |
| Database reachable | `/health` | 200 | NFR4.7 |

## Alerts

| Alert | Condition | Severity | Routes to |
|---|---|---|---|
| None configured | A person reads the console log and `/health` while developing | n/a | n/a |

## SLIs and SLOs

| SLI | SLO target | Measurement window |
|---|---|---|
| Create latency, 95th percentile | under 2 seconds | the Build and Test run (20 runs) |
| Successful responses to valid requests | no 5xx in the Build and Test run | the Build and Test run |

No availability SLO is set (NFR4.7).

## Logs and tracing

- Console logs: plain format in Development, JSON format elsewhere, one line per request plus errors with a trace id (observability design).
- Nothing is shipped anywhere. If a log service is added later, the JSON lines are ready to ship unchanged.
- Tracing: only the per-request trace id; no tracing backend.
- Dashboards: none.
