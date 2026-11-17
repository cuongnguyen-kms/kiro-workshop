# Performance Design: u3-contract-web

| Concern | Design | Covers |
|---|---|---|
| Instant save feedback | The save status moves to `saving` synchronously in the submit handler, before the request is sent, so the spinner and disabled state appear on the next render. | NFR5.2 |
| Long schedules | ScheduleTable renders plain table rows with no per-row components that hold state, and the rows are computed once per data change (`useMemo`). If the render test for 5,200 rows exceeds 1 second, the table shows 100 rows per page with the total row always visible; the target is not loosened. | NFR5.3 |
| One request per load | Each screen loads its data in one effect keyed by the address (id, or page and page size). A request still in flight is cancelled with `AbortController` when the address changes, so stale answers never replace newer ones. The submit handler sends `POST` once and never retries on its own. | NFR5.4 |
| Small first load | Each screen is loaded with `React.lazy` and dynamic import. The list screen's built JavaScript is checked against the 300 KB compressed limit by a size check in the unit's verification. | NFR5.5 |
| No caching layer | No client cache. Data is small and must reflect what is stored. | NFR5.4 |
