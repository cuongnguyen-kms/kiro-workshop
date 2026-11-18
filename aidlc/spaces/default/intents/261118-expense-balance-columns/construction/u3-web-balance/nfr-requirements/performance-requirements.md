# Performance Requirements: u3-web-balance

Sources: `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md`, `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/requirements-analysis/requirements.md` (NFR1), contract C1 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`, and the web stack in `aidlc/spaces/default/codekb/Kiro-Workshop/technology-stack.md`.

The 2-second target in NFR1 covers the whole read or preview. U2 owns the API time; U3 owns the time to turn the response into a table.

| ID | Requirement | Pass/fail check |
|---|---|---|
| NFR1.1 | For 10 streams over 10 years (120 balance rows), building the display rows and rendering the first page of the expense table takes under 200 ms in the jsdom test run, leaving most of the 2 seconds to the API. | A Vitest test renders the expense tab with 120 fake balance rows and asserts the render completes under 200 ms (lenient, jsdom only, as team.md accepts). |
| NFR1.2 | Display rows are built once per data change (in `useMemo`), with no per-row components, the same as the payment table today. | Code review; the existing performance tests in `web/src/test/schedule.perf.test.tsx` still pass. |
| NFR1.3 | The production bundle stays under the existing 300 KB gzip limit for the list screen. No new packages. | `npm run size` passes. |
