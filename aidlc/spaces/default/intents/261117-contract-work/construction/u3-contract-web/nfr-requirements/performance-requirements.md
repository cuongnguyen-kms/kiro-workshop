# Performance Requirements: u3-contract-web

The backend (U2) carries the save budget of 2 seconds for 10 streams over 10 years (NFR5). This unit's share is how fast the screens respond and render.

| ID | Requirement | Inception NFR | How it is checked |
|---|---|---|---|
| NFR5.2 | Pressing Save shows the saving state (spinner, disabled button, read-only fields) within 100 ms, before the response arrives. | NFR5 | Component test with a fake API that answers slowly |
| NFR5.3 | The contract detail screen renders a payment schedule of 5,200 entries (10 weekly streams over 10 years) in under 1 second after the data arrives, on a typical development machine. | NFR5 | Component test that renders the largest schedule from the performance test data and measures the render |
| NFR5.4 | The client sends one request per screen load (one list request, or one contract request) and never repeats a failed `POST` by itself. | NFR5 | Component tests count calls to the API module |
| NFR5.5 | The production build keeps the first page load small: route screens load on demand, and the built JavaScript for the list screen stays under 300 KB compressed. | NFR5 | Build output size check in the unit's verification |

If NFR5.3 fails on a long schedule, the fix is to show the table in pages of 100 rows (still with the total), not to loosen the target.
