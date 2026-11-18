## Review

**Verdict:** READY
**Reviewer:** aidlc-architecture-reviewer-agent
**Date:** 2026-11-18T05:56:00Z
**Iteration:** 1
**Verdict Dictated:** no

### Findings

| ID | Severity | Location | Finding | Required action | Status |
|---|---|---|---|---|---|
| R-01 | Major | aidlc/spaces/default/intents/261118-expense-balance-columns/construction/u3-web-balance/functional-design/functional-spec.md > Layout and accessibility - US4.1 (sticky header row) and frontend-components.md > Changes per component, ScheduleTable | The spec says the header row stays at the top when the container scrolls down. The scroll container is the existing region div in web/src/components/ScheduleTable.tsx, which has overflow-x-auto and no height limit, and a page is at most 100 rows. Setting overflow-x to auto makes overflow-y auto as well, so this div is the nearest scroll container for a sticky header. It never scrolls vertically, so the header would not stay in view. In the popup the vertical scroll happens on DialogContent (max-h-[90vh], overflow-y-auto in web/src/components/ui/dialog.tsx), which a sticky header inside the region does not follow. The sticky header cannot work as written. | State a maximum height for the table region (for example a viewport-based limit, tab and popup) so the region itself scrolls both ways and the sticky header and sticky Period column work. Or drop the sticky header from the spec. Add a test that checks the chosen behaviour. | New |
| R-02 | Minor | functional-spec.md > Layout and accessibility, and frontend-components.md > Changes per component, ScheduleTable | The pager (`schedule-pager-*`) renders inside the same scroll region div as the table. When the table scrolls sideways the Previous and Next buttons scroll out of view with it. The spec does not say where the pager sits, and "More than 100 rows" is a listed screen state. | Put the pager outside the scrolling region, or make it sticky to the left edge. Record which in the spec. | New |
| R-03 | Minor | functional-spec.md > Workflow 1 (Format an amount) and Tests | The spec says "round to 2 decimals (half away from zero)" but names no method. The current formatMoney uses toFixed, and its comment in web/src/contracts/display.ts says display helpers never round money (NFR4.4). Float cases such as 1.005 or -0.005 round the wrong way with a naive Math.round or toFixed. The test table has no half-cent case. API amounts already have at most 2 decimals, so this is low risk. | Say that rounding here only guards display, and name the method (for example round on the integer cents value). Add a half-cent case to the formatter tests. Update the display.ts comment. | New |
| R-04 | Minor | frontend-components.md > API integration points; functional-spec.md > Workflow 3, Screen states | A response without `balanceRows` is treated as an empty list, "so the tab shows an empty table". C1 v1.1.0 marks `balanceRows` as required, and Workflow 3 sets no empty-state text for a non-None method. An older API would show eight headers over no rows with no explanation, and no test covers it. | Add a test for the missing-field case. Either show an empty-state message for a non-None method with no rows, or remove the tolerance and rely on the required field. | New |
| R-05 | Minor | functional-spec.md > Workflow 3, Layout and accessibility; frontend-components.md > Changes per component, ScheduleTable | The existing region already takes `aria-label={caption}`. The spec adds a new name ("Expense schedule, scrolls sideways") through `regionLabel`, but the payment tables keep the plain caption. The spec does not say that the visually hidden caption on the table must stay different from the region name, so a screen reader could read two near-identical names. | Note that the caption stays and that the region name is the only place "scrolls sideways" appears. Check both with axe, which the spec already requires. | New |

### Validation Tool Results

| Tool | Result | Interpretation |
|---|---|---|
| none listed | not run | The stage lists no validation tools. Cross-checks were made by hand: all 24 acceptance criteria in traceability.json (AC1.1.1 to AC4.1.4) match stories.md and the spec. The `balanceRows` fields and headers match C1 v1.1.0 in contract-summary.md. The stream summary and payment paths already call formatMoney, so the one-formatter claim holds in web/src. |

### Summary

The design is implementable: it keeps one shared column and row definition for the tab and the popup, adds no browser-side money logic, and matches C1 v1.1.0 and the stories. One Major design gap should be weighed before approval: the sticky header cannot work with the current scroll container. The Minor items are small clarifications. A developer could build from these files.
