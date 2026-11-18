## Review

**Verdict:** READY
**Reviewer:** aidlc-architecture-reviewer-agent
**Date:** 2026-11-18T06:06:18Z
**Iteration:** 1
**Verdict Dictated:** no

### Findings

| ID | Severity | Location | Finding | Required action | Status |
|---|---|---|---|---|---|
| R-01 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/performance-design.md > "No per-row components" row | The design says alignment comes from table-level descendant classes. The current ScheduleTable right-aligns body cells only for the last column (`[&_tbody_td:last-child]:text-right`), and the functional design needs all seven amount columns right-aligned with tabular figures. The design does not say how columns 2 to 7 get that alignment without per-cell attributes. A `th scope="row"` Period cell also gets none of the current `[&_tbody_td]:px-3 py-2` padding. | State the class approach. For example, use an `[&_tbody_td:not(:first-child)]` selector for the amount cells, or a small per-column class computed once per column rather than per row. Also give the row-header `th` the same padding. | New |
| R-02 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md > SEC2 (balanceRows missing) | A missing `balanceRows` is meant to show the eight headers with no rows. In `ScheduleTable`, `rows.length === 0 && emptyMessage` returns a bare paragraph with no headers. The design does not say the expense table must be rendered without `emptyMessage`. | Say that the expense tab and popup pass no `emptyMessage`. The SEC2 component test should assert the eight headers are present. | New |
| R-03 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md > Accessibility (scroll region, pager) | In the current `web/src/components/ScheduleTable.tsx` the pager is a child of the focusable scroll region, and the region's `aria-label` is the caption. The design requires the pager outside the scrolled area and a label different from the caption. Both are structural changes, and the new prop for the label is not named. Putting the table and the pager under a common wrapper also changes where the focus ring sits. | Name the new prop (for example `regionLabel`). State that the scroll region wraps only the table and that the pager moves into a sibling element. Keep the payment table's existing label and focus behaviour as the default. | New |
| R-04 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md > Accessibility (sticky header and column) | Sticky cells with row borders do not keep their borders under `border-collapse`. The `[&_tbody_tr]:border-b` rows will lose their lines on the sticky first column while scrolling. The design covers the background colour but not the borders or the z-order between the sticky header, the first column and the top-left cell. | Add a border or shadow on the sticky cells and a z-index order (top-left cell above header and column). The by-eye browser check should include these. | New |
| R-05 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/security-design.md > Exact money format; display.ts | `formatMoney` is shared by the payment table, `StreamsSummary` and the stream labels, so the comma and bracket change reaches all of them. The design lists only expense-table tests. NFR2.1 allows updating "only the expected amount strings", and this also applies to those other screens. | Add a note and a test that the payment schedule, stream summary and stream labels show the new format (for example `1,000.00`). Also note that the existing display-helper header comment ("never round money") needs updating. | New |
| R-06 | Minor | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/nfr-design/performance-design.md > Timing test | A fixed 200 ms wall-clock assertion in jsdom, including cold module and first-render cost, may fail intermittently on slow CI machines. | Run the render once before timing, or keep the threshold generous. If it proves flaky, record it as advisory. | New |

### Validation Tool Results

| Tool | Result | Interpretation |
|---|---|---|
| none listed | not run | The stage lists no validation tools. Cross-references were checked by hand: all nine NFR IDs in traceability.json map to a design section, and the component files match the functional design's component table. |

### Summary

The design covers all nine upstream NFR IDs. It specifies the formatter algorithm concretely, including the 999.995 carry and the negative-zero case, and gives a workable sticky and scroll structure. The remaining findings are implementation details that a developer can settle without the architect. The design is buildable as written.
