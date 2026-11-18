# Requirements: Schedule preview while creating a contract

## Intent Analysis

Initial description: "I want to view the payment schedule and expense schedule while creating a contract, not only when it is saved. Add 2 new buttons that open a popup for each schedule, once users have entered enough info on the page." [desc]

Dana wants to check the schedules before she commits a contract, so she can fix a wrong frequency, date or amount without saving a contract she then cannot edit (editing is out of scope in the existing system). This is an enhancement to the existing New contract screen (S2) and the contract API. It touches two components: the React form (u3-contract-web) and the API (u2-contract-api). The schedule rules in u1-schedule-engine do not change.

## Functional Requirements

### FR1. Preview buttons on the New contract form
- FR1.1 The New contract form shows two buttons: "Payment schedule" and "Expense schedule". They sit beside the Save button. [desc]
- FR1.2 "Payment schedule" is enabled when the contract start and end dates are valid and every payment stream is valid (the same client checks as Save for those fields). The contract name is not needed. [Q1]
- FR1.3 "Expense schedule" is enabled when everything FR1.2 needs is valid and, in addition, the expense method and its dates are valid. [Q1]
- FR1.4 When the expense method is None, "Expense schedule" stays disabled and shows the hint "This contract has no expense schedule." [Q3]
- FR1.5 A disabled button tells the user why it is disabled (for example "Enter valid contract dates and payment streams to preview."), readable by screen readers.
- FR1.6 The buttons enable and disable as the user edits the form, without a page reload.

### FR2. Preview calculation
- FR2.1 Opening either popup asks the server for a preview built from the current form values. The server checks the input with the same rules as Save and calculates the schedules with the same schedule library, so the preview equals what Save would store. [Q2]
- FR2.2 The preview stores nothing: no contract, stream or schedule row is saved, and the contract list does not change. [Q2]
- FR2.3 A payment-schedule preview does not depend on the expense fields: invalid or incomplete expense fields never block it. [Q1]
- FR2.4 The preview needs no contract name; a blank name never blocks it. [Q1]
- FR2.5 When the server rejects the preview input, the popup shows the server's messages (the same wording as Save) and no table.
- FR2.6 When the preview call fails for another reason, the popup shows "The schedule could not be calculated. Try again." with a Retry button. The form keeps every entry.
- FR2.7 The same size limit as Save applies (more than 10,000 schedule rows is rejected with the same message).

### FR3. Popups
- FR3.1 "Payment schedule" opens a popup with the same table as the saved contract's Payment schedule tab: due date, stream, amount, and the total. [Q4]
- FR3.2 "Expense schedule" opens a popup with the same table as the saved contract's Expense schedule tab: period, amount, and the total. [Q4]
- FR3.3 Payment rows name their stream the way the form does ("Stream 1", "Stream 2", in form order), since nothing is saved yet.
- FR3.4 While the preview loads, the popup shows a loading state.
- FR3.5 The popup has a title, a Close button, closes with Escape, keeps focus inside while open and returns focus to the button that opened it.
- FR3.6 The popup shows the preview for the form values at the moment it was opened. Closing it and opening it again after a change shows the new values.
- FR3.7 Opening or closing a popup never changes or clears the form.

## Non-Functional Requirements

- NFR1. Performance: a preview of 10 streams over 10 years appears in the popup within 2 seconds on a typical development machine, the same budget as Save.
- NFR2. Consistency: for the same input, the preview's rows and totals equal the rows and totals that Save returns.
- NFR3. Security: the preview call follows the existing API rules (input validation, 256 KB body limit, configured CORS origin, generic error responses). It needs no sign-in, like the rest of the API.
- NFR4. Accessibility: the buttons and popups meet WCAG 2.2 AA, like the existing screens (axe finds no serious or critical issues).
- NFR5. Compatibility: the change only adds to API contract v1 (a new endpoint); existing endpoints and responses do not change.

## Constraints

- Existing stack: .NET 10 API, React with shadcn/ui, schedule library u1-schedule-engine (unchanged).
- Validation messages come from the existing interaction spec and Functional Design rules; no new wording for the same rules.

## Assumptions

- The preview call is a new endpoint under `/api/v1` that takes the same request body as create. Rationale: reuse of the existing request shape keeps one validation path.
- Opening a popup always fetches a fresh preview; nothing is cached. Rationale: inputs change between openings and the call is cheap.
- No preview on the saved contract detail screen: it already shows the schedules.

## Out of Scope

- Editing schedule rows in the popup.
- Printing or exporting the preview.
- Live preview that updates while the popup is open.

## Open Questions

- None.
