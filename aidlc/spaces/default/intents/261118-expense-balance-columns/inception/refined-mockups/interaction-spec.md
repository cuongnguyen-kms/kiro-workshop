# Interaction Specification: expense balance columns

Format follows `.kiro/knowledge/aidlc-design-agent/component-spec-template.md` at component level.

## Component: Expense schedule table

- **Purpose:** show the expense schedule with the balance columns (US2.1, US2.2) on the detail tab (M1) and in the preview popup (M2).
- **Used in:** `ScheduleTabs` (Expense schedule tab), `SchedulePreview` (Expense schedule popup).
- **Inputs:** a list of rows from the API, each with period start, period end, expense, (accrual)/prepaid, cumulative balance and the four ST/LT amounts. The table never calculates; it formats and shows what the API returns.
- **Columns and labels (exact text):**
  1. `Period` - month and year, for example `Jan 2027`
  2. `Expense`
  3. `(Accrual)/Prepaid`
  4. `Cumulative Balance`
  5. `ST Accrued Liability`
  6. `LT Accrued Liability`
  7. `ST Prepaid Asset`
  8. `LT Prepaid Asset`

  The request spelled "ST Accured Liability"; the header uses the correct spelling "Accrued", matching "LT Accrued Liability".
- **Layout:** headers on one line (no wrapping). The table sits in a container that scrolls sideways when it is wider than the space. The Period column is sticky on the left, with its own background so scrolled cells do not show through. Header row sticky at the top when the container scrolls down.
- **Alignment:** Period left; every amount column right-aligned with tabular figures.
- **Total row:** none.
- **States:** no new states; loading, empty (method None) and error stay as today.

## Component: Expense schedule popup

- **Width:** up to 80% of the viewport width for the Expense schedule popup; on screens where 80% is narrower than the content, the table scrolls sideways inside the popup. The Payment schedule popup keeps today's width.
- **Height:** unchanged maximum height; the body scrolls down.
- **Focus and keyboard:** unchanged (focus moves into the popup, Escape and Close return focus to the button). The table's scroll container is focusable (`tabindex="0"`) with an accessible name, so keyboard users can scroll it with the arrow keys.

## Money format (all screens)

| Value | Shows |
|---|---|
| 1000 | 1,000.00 |
| 1234567.5 | 1,234,567.50 |
| -1000 | (1,000.00) |
| -0.004 or -0 | 0.00 |
| 0 | 0.00 |

- Always 2 decimals, comma thousands separators, full stop decimal point, regardless of the browser's language (the existing app shows dates as `YYYY-MM-DD`, so it is not locale-dependent either).
- Brackets replace the minus sign; same text colour as other amounts.
- The screen reader hears the visible text; the column header names what the amount is.

## Behaviour that does not change

Buttons, enabling rules, popup opening and closing, retry and error messages from the schedule preview work, and the payment schedule's columns and total row.
