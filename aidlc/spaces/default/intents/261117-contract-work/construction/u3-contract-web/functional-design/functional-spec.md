# Functional Specification: u3-contract-web

The React frontend for Dana, the contract manager. It has three screens: the contracts list (S1), the New contract form (S2) and the contract detail (S3), as drawn in the Refined Mockups. It calls the backend only through contract C1 (`/api/v1`). It never generates schedules; it shows what the API returns.

Stories implemented: US1.1, US2.1, US3.1, US6.1, US6.2, US6.3 (the screen side). The rules for dates, schedules and storage belong to u1 and u2.

## Routes

| Route | Screen | Stories |
|---|---|---|
| `/` | Redirects to `/contracts` | |
| `/contracts` | S1 Contracts list | US6.3 |
| `/contracts/new` | S2 New contract | US1.1, US2.1, US3.1, US6.1 |
| `/contracts/:id` | S3 Contract detail | US6.1, US6.2 |
| anything else | "Page not found" with a link to the list | |

## Form Model (S2)

| Field | Type | Required | Notes |
|---|---|---|---|
| name | text, at most 200 characters | yes | Not blank or whitespace only |
| startDate, endDate | date | yes | End not before start |
| paymentStreams | list of streams, at least 1 | yes | Starts with one empty stream; Remove disabled on the last one |
| stream.amount | decimal, 2 places | yes | Greater than zero |
| stream.frequency | one of Weekly, Monthly, Quarterly, Annually, OneTime | yes | Shown to Dana as Weekly, Monthly, Quarterly, Annually, One-time |
| stream.startDate | date | yes | Within the contract dates |
| stream.endDate | date | yes unless OneTime | Within the contract dates, not before the stream start. For OneTime the field is hidden and the request sends the start date as the end date |
| expense.method | Overtime, AllAtOnce, None | yes | Starts as None; shown as Overtime, All at once, None |
| expense.startDate, expense.endDate | date | Overtime only | Within the contract dates, end not before start |
| expense.date | date | AllAtOnce only | Within the contract dates |

Changing the frequency or the expense method hides the fields that no longer apply and clears their values, so a hidden value is never sent (AC2.1.5, AC3.1.1 to AC3.1.3).

## Validation Messages

The client uses exactly the wording the API returns (contract C1 and the interaction spec).

| Rule | Field | Message |
|---|---|---|
| Name blank | name | Contract name is required. |
| End before start | endDate | End date must not be before the start date. |
| Amount not above zero | paymentStreams[i].amount | Amount must be greater than zero. |
| Stream outside contract | paymentStreams[i].startDate or endDate | Stream dates must be within the contract dates. |
| Expense outside contract | expense.startDate, expense.endDate or expense.date | Expense dates must be within the contract dates. |
| Overtime end before start | expense.endDate | Expense end date must not be before the start date. |

The client check is a convenience. The API checks everything again and its answer wins: when it returns a 400, each message under `errors` is shown next to the field whose path matches the key. A key that matches no field is shown in the error summary only.

## Workflows

### W1. Create a contract (US1.1, US2.1, US3.1, US6.1)

1. Dana opens `/contracts/new`. The form starts with the initial state from the mockups.
2. Dana fills the fields. Each field is checked when it loses focus.
3. Dana presses Save contract. All fields are checked.
   - If any fail: show the error summary at the top, move focus to it, show each message under its field, and stop. Nothing is sent.
4. Otherwise send `POST /api/v1/contracts`. The button shows a spinner and is disabled, and the fields are read-only.
5. On 201: go to `/contracts/{id}` and show the dismissible "Contract saved." banner (AC6.1.1).
6. On 400: put the form back in edit mode, show the messages as in the validation section, and move focus to the error summary.
7. On any other failure or no connection: put the form back in edit mode, keep all entries, and show the alert "The contract was not saved. Nothing was stored. Try again." (AC6.1.2). The client never sends the request again by itself, so a contract cannot be saved twice by accident.
8. Cancel: if the form has entries, ask "Discard this contract?" (Discard, Keep editing); otherwise go straight back to the list.

### W2. Open a contract (US6.2)

1. Dana opens `/contracts/{id}`. Show skeletons while loading.
2. On 200: show the name, period, expense method and payment streams, with the Payment schedule tab selected. The Payment schedule tab shows due date, stream and amount for each entry, and a Total row. The Expense schedule tab shows its rows and a Total row (AC6.2.1).
3. On 404: show "Contract not found." with a link to the list (AC6.2.2).
4. On any other failure: show the alert "Could not load this contract." with a Retry button that sends the request again.

Expense schedule tab by method:
- Overtime: columns Month and Amount, one row per entry, Month shown as the year and month of `periodStart`.
- AllAtOnce: one row with Date and Amount.
- None: the text "This contract has no expense schedule." (AC5.1.5 on screen).

Stream labels in the payment table are the frequency and the stream's amount (for example "Monthly 1000.00"), since streams have no names.

### W3. List contracts (US6.3)

1. Dana opens `/contracts`. The page reads `page` and `pageSize` from the address (defaults 1 and 10) and sends `GET /api/v1/contracts?page=&pageSize=`.
2. Show a table of name, start and end dates. The name links to `/contracts/{id}` (AC6.3.1).
3. Below the table, a pager shows the page, the total, Previous and Next, and a page-size select with the options 10, 20, 50 and 100, defaulting to 10 (decision in Contract Design, Q3). Changing the page size goes back to page 1. The page and size are kept in the address, so a refresh or a shared link shows the same page.
4. Empty (`totalCount` of 0): show "No contracts yet." with the New contract button below it (AC6.3.2).
5. A page past the last one returns no items: show the last page instead by moving to `totalPages`.
6. On failure: show the alert "Could not load contracts." with a Retry button.

## Screen State Summary

| Screen | States |
|---|---|
| S1 | loading, loaded, empty, error |
| S2 | editing, validation errors, saving, save failed, discard confirmation |
| S3 | loading, loaded, not found, error; banner visible or dismissed |

## Number and Date Display

- Amounts show with exactly 2 decimals and are right-aligned. The client formats for display only and never adds or splits amounts.
- Dates show as `YYYY-MM-DD`, the same as the API.

## Configuration

- The API base address comes from configuration (`VITE_API_BASE_URL`), not from code. When it is empty, calls go to the same address the page was loaded from.
