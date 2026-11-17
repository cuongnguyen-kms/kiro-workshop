# Requirements: Contract Management System

## Intent Analysis

Build a contract management system (.NET 10 backend, React frontend). A user enters a contract with its dates, payment streams and an expense method. On save, the system generates a payment schedule and an expense schedule and stores both with the contract.

## Functional Requirements

### FR1. Contract entry
- FR1.1 A user can create a contract with a Contract Name (required, non-empty).
- FR1.2 A contract has a start date and an end date. The end date must not be before the start date.
- FR1.3 A contract can have one or more payment streams (Q3).
- FR1.4 A contract has exactly one expense method: Overtime, All at once, or None.

### FR2. Payment streams
- FR2.1 Each payment stream has an amount (greater than zero), a frequency, a stream start date and a stream end date.
- FR2.2 Allowed frequencies are Weekly, Monthly, Quarterly, Annually and One-time (Q3).
- FR2.3 A One-time stream produces a single payment on its start date. Its end date equals its start date or is ignored.
- FR2.4 A stream's start and end dates must fall within the contract's start and end dates.

### FR3. Expense method
- FR3.1 Overtime: the user selects an expense start date and end date. The expense amount is spread across that range (FR5).
- FR3.2 All at once: the user selects a single expense date. The whole expense amount is booked on that date.
- FR3.3 None: no expense schedule is generated.
- FR3.4 The expense amount is the total of all payments in the generated payment schedule (Q1).
- FR3.5 Expense dates must fall within the contract's start and end dates.

### FR4. Payment schedule generation
- FR4.1 On save, the system generates a payment schedule from every payment stream: one row per payment, each with a due date, the amount and the stream it came from.
- FR4.2 Payment dates start on the stream start date and repeat at the stream's frequency until the stream end date (inclusive).
- FR4.3 For Monthly, Quarterly and Annually streams that start on a day that does not exist in a later month (for example the 31st), the payment falls on the last day of that month.

### FR5. Expense schedule generation
- FR5.1 On save, the system generates the expense schedule from the expense method.
- FR5.2 Overtime: the amount is divided equally per day across the date range and summed into one row per calendar month, each with the month and the amount (Q2). Any rounding difference goes to the last row so the rows add up exactly to the total.
- FR5.3 All at once: the schedule has one row with the chosen date and the full amount.
- FR5.4 None: the schedule is empty.
- FR5.5 Amounts are rounded to 2 decimal places.

### FR6. Persistence and viewing
- FR6.1 The contract is saved together with its payment schedule and expense schedule in one operation. If any part fails, nothing is saved.
- FR6.2 A user can open a saved contract and see its details, payment schedule and expense schedule.
- FR6.3 A user can list saved contracts.

### FR7. User interface
- FR7.1 The React frontend provides a contract entry form covering FR1 to FR3, with inline validation messages.
- FR7.2 The form lets the user add and remove payment streams.
- FR7.3 The form shows only the date inputs relevant to the chosen expense method.
- FR7.4 After saving, the frontend shows the generated schedules.

## Non-Functional Requirements

- NFR1. Technology: the backend uses .NET 10 and the frontend uses React.
- NFR2. Storage: contracts and schedules are stored in PostgreSQL (Q4).
- NFR3. Access: no sign-in in this version (Q4). The API is designed so authentication can be added later without changing the contract data model.
- NFR4. Correctness: schedule generation is deterministic. The same contract input always produces the same schedules, and schedule totals match the contract inputs exactly.
- NFR5. Performance: saving a contract with up to 10 payment streams spanning 10 years responds within 2 seconds on a typical development machine.
- NFR6. Validation: the backend validates every rule in FR1 to FR3 and rejects invalid input with a clear message. The frontend checks are a convenience, not the only check.
- NFR7. Testability: schedule generation is a separate unit with no database dependency, so it can be tested on its own.

## Constraints

- .NET 10 backend and React frontend (stated by the requester).
- PostgreSQL as the database (Q4).

## Assumptions

- A contract uses a single currency, and currency is not captured in this version. Rationale: not mentioned in the request.
- Dates are calendar dates with no time or time zone. Rationale: schedules are by day.
- Editing or deleting a saved contract, and regenerating its schedules afterwards, is out of scope for this version. Rationale: the request describes only create and save.
- Every payment in the schedule counts toward the expense total, whatever its stream.

## Out of Scope

- Sign-in, users and roles.
- Editing, deleting or versioning contracts.
- Multiple currencies, taxes, and inflation or escalation of payment amounts.
- Payment tracking (paid or unpaid), invoices and reminders.
- Document or file attachments.

## Open Questions

- None blocking. The requester may later want to change the assumptions above, mainly editing saved contracts.
