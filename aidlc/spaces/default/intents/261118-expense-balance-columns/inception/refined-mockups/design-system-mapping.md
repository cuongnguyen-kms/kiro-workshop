# Design System Mapping: expense balance columns

The app uses shadcn/ui on Tailwind CSS. No new library and no new shadcn component is needed.

| Element | Component | Change |
|---|---|---|
| Expense schedule table | existing `ScheduleTable` (`web/src/components/ScheduleTable.tsx`), built on the shadcn `Table` | Support several right-aligned amount columns (today only the last column is right-aligned), an optional total row (off for expense), a sticky first column and sticky header |
| Horizontal scroll | the `div` wrapper the shadcn `Table` already puts around `<table>` (`overflow-x-auto`) | Make it focusable with an accessible name; sticky first column through `sticky left-0` with the card background |
| Expense popup | existing shadcn `Dialog` (`web/src/components/ui/dialog.tsx`) | A wider content class for the expense popup only: `max-w-[80vw]` (Q2); payment popup unchanged |
| Tabs | existing shadcn `Tabs` | none |
| Amount text | Tailwind `tabular-nums text-right` | applied to every amount column |
| Money formatter | `formatMoney` in `web/src/contracts/display.ts` | New format: thousands separators, 2 decimals, brackets for negatives, never `-0.00` |

## Tokens

- Text colour for negatives: the normal foreground token (no red, Q3).
- Sticky column background: the card or popup background token, so it matches in both places.
- Spacing: existing table cell padding.
