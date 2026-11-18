# Performance Design: u3-web-balance

Designs how U3 meets `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-requirements/performance-requirements.md`, using `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-requirements/tech-stack-decisions.md`, the workflows in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md` and contract C1 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md`.

| Decision | How | Covers |
|---|---|---|
| Build rows once | `expenseBalanceRows` is a pure function called in `useMemo`, keyed on the balance rows array, in both the tab and the popup. | NFR1.2 |
| No per-row components | Rows stay plain `tr` elements; amount alignment and the sticky first column come from table-level descendant classes, as the payment table does today, so a row carries no per-cell style attributes. | NFR1.2 |
| Paging | The existing 100-row pager stays; 10 years of monthly rows (120) is two pages. | NFR1.1 |
| Cheap formatter | The money formatter does string work on at most about 20 characters per value; 120 rows times 7 amounts is 840 calls. | NFR1.1 |
| No new packages | Hand-written formatter and CSS sticky positioning. | NFR1.3 |

Timing test: render the expense tab with 120 fake rows (first page of 100 shown) and assert it completes under 200 ms in jsdom. This is a lenient guard against an accidental slowdown, not a browser measurement; real-browser timing is not automated (team.md). The popup uses the same table and row builder, so it is not timed separately.
