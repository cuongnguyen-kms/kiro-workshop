# Monitoring Design: u3-contract-web

The frontend has no server, so its monitoring is limited. This version adds no browser error reporting service, because none was asked for and it would send data to a third party. The backend's logs are the main record of failed calls.

## Metrics and KPIs

| Metric | Source | Threshold | Why it matters |
|---|---|---|---|
| Built bundle size of the list screen | Build output, checked in verification | Under 300 KB compressed | First load speed (NFR5.5) |
| Accessibility issues | axe in the component tests | Zero serious or critical | WCAG 2.2 AA target (NFR7.4) |
| High or critical vulnerabilities in production packages | `npm audit` in verification | Zero | NFR3.8 |
| Failed API calls seen by users | Backend request logs (u2) | Owned by u2's monitoring | The frontend reports every failure on screen |

## Alerts

| Alert | Condition | Severity | Routes to |
|---|---|---|---|
| Verification failed | The verification command exits non-zero on bundle size, accessibility, audit or tests | Blocks the unit | The developer at the unit checkpoint |

No runtime alerts in this version, since there is no production host yet.

## SLIs and SLOs

| SLI | SLO target | Measurement window |
|---|---|---|
| Static host availability | Set when a host is chosen (open item) | 30 days |

## Logs and Tracing

- In development, the browser console shows failed requests. The app does not log contract data to the console.
- No tracing in this version. A request ID header from u2, if added later, can be shown in the failure alert for support.
