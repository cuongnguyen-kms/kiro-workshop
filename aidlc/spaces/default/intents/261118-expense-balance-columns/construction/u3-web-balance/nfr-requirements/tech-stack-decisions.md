# Tech Stack Decisions: u3-web-balance

No new technology. U3 stays on the web stack in `aidlc/spaces/default/codekb/Kiro-Workshop/technology-stack.md`. The design in `aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md`, the requirements in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/requirements-analysis/requirements.md` and contract C1 in `aidlc/spaces/default/intents/261118-expense-balance-columns/inception/contract-design/contract-summary.md` need nothing beyond it.

| Area | Choice | Reason |
|---|---|---|
| UI | React with TypeScript strict, the existing `ScheduleTable`, shadcn/Radix dialog and tabs, Tailwind classes | Existing code; no new component library. |
| Money format | A hand-written formatter in `web/src/contracts/display.ts` using decimal string arithmetic | `Intl.NumberFormat` depends on the browser language and offers no bracket style everywhere; float multiplication misrounds half-cents (NFR3.1). |
| Month names | A fixed English list | Same output in every browser, like the existing `YYYY-MM-DD` dates. |
| Sticky column and header | CSS `position: sticky` on the first column and the header row, inside a scroll region with a height limit | No library needed; works in all current browsers. |
| Tests | Vitest, Testing Library, vitest-axe, jsdom | Existing setup (team.md). |
| Packages | None added | Keeps the 300 KB size limit (NFR1.3). |

## Alternatives considered

- **`Intl.NumberFormat('en-US', { currencySign: 'accounting' })`**: brackets need a currency style, which adds a currency symbol, and rounding still follows floats; rejected.
- **A table library (for example TanStack Table)**: too much for one table and adds bundle size; rejected.
