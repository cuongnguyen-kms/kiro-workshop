# Observability Design: u2-contract-api

| Area | Design | Serves |
|---|---|---|
| Request logging | ASP.NET Core request logging middleware writes one line per request: method, path (no query values for the list beyond page and pageSize), status, duration. | NFR6.7 |
| Log content rules | Application log calls pass the contract id and counts only. The request body, names, amounts and connection settings are never logged. | NFR6.8 |
| Error logging | The exception handler logs once at error level with the exception and the request trace id (`Activity.Current.TraceId`, falling back to the request's trace identifier). The same value is returned as `traceId` in the 500 body. | NFR6.9 |
| Validation logging | Validation failure logs the list of field paths at information level. | NFR6.10 |
| Format | Console logging with the built-in JSON formatter outside Development and the plain console formatter in Development. | NFR6.7 |
| Metrics, tracing, alerts | None built in this version. The built-in .NET request metrics exist if someone later attaches a collector. The two indicators (create latency 95th percentile, 5xx share) can be read from the request log lines. | NFR6.7 |
| Dashboards | None. | NFR6.7 |
| Correlation | A trace id is created per request by the framework and appears in the log lines of that request and in 500 responses. There is no cross-service tracing: the frontend is a browser app and the schedule engine runs in process. | NFR6.9 |
| Health | `/health` as in the reliability design. | NFR4.8 |
