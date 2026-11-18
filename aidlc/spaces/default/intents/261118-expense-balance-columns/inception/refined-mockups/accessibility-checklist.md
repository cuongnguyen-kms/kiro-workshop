# Accessibility Checklist: expense balance columns (WCAG 2.2 AA)

| Check | WCAG | How it is met | Verified by |
|---|---|---|---|
| Every column has a header cell (`<th scope="col">`) with the exact label | 1.3.1 | Shared `ScheduleTable` renders headers from the column list | component test, axe |
| Row headers: the Period cell is a row header (`<th scope="row">`) so each amount is announced with its month | 1.3.1 | `ScheduleTable` renders the first column as `th scope="row"` | component test |
| Negative amounts are not shown by colour alone | 1.4.1 | Brackets carry the meaning; no colour coding (Q3) | review |
| Text contrast unchanged | 1.4.3 | Normal foreground token on card background | axe |
| Sideways scroll region reachable by keyboard | 2.1.1 | Scroll container has `tabindex="0"`, `role="region"` and an accessible name such as "Expense schedule, scrolls sideways" | component test, axe |
| Visible focus on the scroll region | 2.4.7 | Existing focus ring class on the container | review |
| Sticky column does not hide focused content | 2.4.11 | The sticky column covers only the Period column; scrolled amount cells stay visible | review |
| Content reflows at 320 px width | 1.4.10 | Data tables are an allowed exception to reflow; the table scrolls inside its container while the page does not | review |
| Popup keeps focus inside and returns it on close | 2.4.3 | Unchanged Radix Dialog behaviour | existing tests |
| No serious or critical axe issues on the tab and the popup | - | Run vitest-axe on both | component test (jsdom; real-browser checks accepted as not run, team.md) |
