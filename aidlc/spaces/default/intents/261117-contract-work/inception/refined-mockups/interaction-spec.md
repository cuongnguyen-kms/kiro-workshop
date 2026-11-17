# Interaction Specification

## Contract Entry Form (S2)

| Interaction | Behavior | Story |
|---|---|---|
| Add stream | Appends an empty stream card. Focus moves to the new card's Amount field. | US2.1 |
| Remove stream | Removes that card. Focus moves to the Add stream button. The form always keeps at least one stream card, and Remove is disabled on the last one. | US2.1 (AC2.1.2) |
| Change frequency | Choosing One-time hides the stream end date field and shows a single Payment date field. Choosing any other frequency shows both stream dates. | US2.1 (AC2.1.5) |
| Change expense method | Shows the date fields for that method and hides the others (see mockups S2). Values in hidden fields are cleared. | US3.1 |
| Date entry | Date fields use a calendar picker and also accept typed `YYYY-MM-DD`. | US1.1 |
| Amount entry | Numeric field, decimal point, 2 decimal places, positive values only. | US2.1 |
| Validate | Fields validate when they lose focus and again on Save. The first invalid field receives focus on Save. | NFR6 |
| Save | Disabled while saving. On success, go to S3 with a "Contract saved." banner. On failure, show the alert and keep all entries. | US6.1 |
| Cancel | Returns to S1. If the form has unsaved entries, a confirmation dialog asks "Discard this contract?" with Discard and Keep editing. | US1.1 |

## Validation Messages

The messages are the ones the stories set. The frontend shows them next to the field, and the backend returns the same wording for the same rule.

| Rule | Message |
|---|---|
| Name blank | Contract name is required. |
| End before start | End date must not be before the start date. |
| Amount not positive | Amount must be greater than zero. |
| Stream outside contract | Stream dates must be within the contract dates. |
| Expense outside contract | Expense dates must be within the contract dates. |
| Overtime end before start | Expense end date must not be before the start date. |

## Contracts List (S1)

- Clicking or pressing Enter on a row opens S3. The row's contract name is the link.
- New contract button goes to S2.
- Rows are sorted by start date, newest first.

## Contract Detail (S3)

- The Payment schedule and Expense schedule tabs follow the standard tabs pattern: arrow keys move between tabs, and the selected panel shows below.
- The Payment schedule opens first.
- Tables are read-only. Dates show as `YYYY-MM-DD`, amounts with 2 decimals, right-aligned.

## Feedback Patterns

| Event | Pattern |
|---|---|
| Field error | Text under the field, red with an error icon, linked to the field with `aria-describedby`. |
| Form error summary | Alert region at the top of the form, receives focus after a failed Save. |
| Save success | Banner on S3, announced to screen readers, dismissible. |
| Load failure | Alert with Retry button. |

## Component Specifications

### ScheduleTable
- **Purpose:** show a payment or expense schedule.
- **Props:** `columns` (label and field), `rows`, `totalLabel`, `totalAmount`.
- **States:** populated, empty (message), loading (skeleton).
- **Behavior:** read-only, amounts right-aligned, last row is the total.
- **Accessibility:** native `table` with `caption`, `th scope="col"`, and the total in a `tfoot` row.

### StreamCard
- **Purpose:** one payment stream's inputs in the entry form.
- **Props:** `index`, `value`, `errors`, `onChange`, `onRemove`, `canRemove`.
- **States:** default, One-time variant, with errors.
- **Behavior:** see Change frequency above.
- **Accessibility:** a `fieldset` whose `legend` is "Payment stream N".
