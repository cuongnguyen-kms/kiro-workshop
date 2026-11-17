# User Stories

Persona: P1 Dana, Contract Manager (see personas.md). Stories are grouped by feature area (answer to Q2). Priorities use MoSCoW.

## Group 1. Contract details

### US1.1 Enter contract details
**As** Dana, **I want** to enter a contract name and its start and end dates, **so that** the contract has a clear identity and period.
**Priority:** Must Have
**Dependencies:** none
**Covers:** FR1.1, FR1.2, FR7.1, NFR6

- AC1.1.1 Given an empty contract form, when Dana enters a name and a start date that is on or before the end date, then the form accepts those fields.
- AC1.1.2 Given a blank or whitespace-only contract name, when Dana tries to save, then the system rejects it with a message that the name is required, in the form and in the API response.
- AC1.1.3 Given an end date before the start date, when Dana tries to save, then the system rejects it with a message that the end date must not be before the start date.

## Group 2. Payment streams

### US2.1 Add payment streams to a contract
**As** Dana, **I want** to add one or more payment streams, each with an amount, a frequency and start and end dates, **so that** every way the contract pays out is captured.
**Priority:** Must Have
**Dependencies:** US1.1
**Covers:** FR1.3, FR2.1, FR2.2, FR2.3, FR2.4, FR7.1, FR7.2, NFR6

- AC2.1.1 Given the contract form, when Dana adds a stream with an amount of 1000, frequency Monthly and dates inside the contract period, then the stream appears in the stream list.
- AC2.1.2 Given two streams on the form, when Dana removes one, then only the other remains.
- AC2.1.3 Given a stream amount of zero or less, when Dana tries to save, then the system rejects it with a message that the amount must be greater than zero.
- AC2.1.4 Given a stream whose start or end date falls outside the contract period, when Dana tries to save, then the system rejects it with a message that stream dates must be within the contract dates.
- AC2.1.5 Given a stream with frequency One-time, when Dana picks the frequency, then the stream needs only one date and its end date equals its start date.
- AC2.1.6 Given the frequency list, when Dana opens it, then it offers exactly Weekly, Monthly, Quarterly, Annually and One-time.

## Group 3. Expense method

### US3.1 Choose how the contract is expensed
**As** Dana, **I want** to choose Overtime, All at once or None as the expense method, **so that** the contract's cost is recognised the way finance expects.
**Priority:** Must Have
**Dependencies:** US1.1
**Covers:** FR1.4, FR3.1, FR3.2, FR3.3, FR3.5, FR7.1, FR7.3, NFR6

- AC3.1.1 Given the expense method Overtime, when Dana selects it, then the form shows an expense start date and an expense end date and no single-date input.
- AC3.1.2 Given the expense method All at once, when Dana selects it, then the form shows one expense date and no date range.
- AC3.1.3 Given the expense method None, when Dana selects it, then the form shows no expense dates.
- AC3.1.4 Given an expense date or range outside the contract period, when Dana tries to save, then the system rejects it with a message that expense dates must be within the contract dates.
- AC3.1.5 Given an Overtime range whose end date is before its start date, when Dana tries to save, then the system rejects it with a message that the end date must not be before the start date.

## Group 4. Schedules

### US4.1 Generate the payment schedule
**As** Dana, **I want** the system to generate a payment schedule from the payment streams, **so that** I do not build due dates by hand.
**Priority:** Must Have
**Dependencies:** US2.1
**Covers:** FR4.1, FR4.2, FR4.3, NFR4

- AC4.1.1 Given a Monthly stream of 1000 from 2027-01-15 to 2027-04-15, when the contract is saved, then the payment schedule has four rows dated 2027-01-15, 2027-02-15, 2027-03-15 and 2027-04-15, each 1000, each linked to that stream.
- AC4.1.2 Given a Monthly stream starting 2027-01-31, when the contract is saved, then the February payment falls on 2027-02-28 and the March payment on 2027-03-31.
- AC4.1.3 Given a One-time stream of 500 on 2027-06-01, when the contract is saved, then the payment schedule has one row dated 2027-06-01 for 500.
- AC4.1.4 Given a Weekly stream, when the contract is saved, then each payment is exactly 7 days after the previous one, up to and including the stream end date.
- AC4.1.5 Given two streams, when the contract is saved, then the payment schedule holds the rows of both, each linked to its own stream.
- AC4.1.6 Given the same contract input saved twice, then both payment schedules are identical.

### US5.1 Generate the expense schedule
**As** Dana, **I want** the system to generate an expense schedule from the chosen expense method, **so that** I can see when the contract cost is recognised.
**Priority:** Must Have
**Dependencies:** US3.1, US4.1
**Covers:** FR3.4, FR5.1, FR5.2, FR5.3, FR5.4, FR5.5, NFR4

- AC5.1.1 Given a payment schedule totalling 12000 and the method All at once on 2027-03-01, when the contract is saved, then the expense schedule has one row dated 2027-03-01 for 12000.
- AC5.1.2 Given a payment schedule totalling 3100 and the method Overtime from 2027-01-01 to 2027-03-31 (90 days), when the contract is saved, then the expense schedule has one row each for January, February and March, and the three amounts add up to exactly 3100.
- AC5.1.3 Given an Overtime split that does not divide evenly to 2 decimal places, when the contract is saved, then the difference is added to the last row and every amount has 2 decimal places.
- AC5.1.4 Given an Overtime range that starts mid-month, when the contract is saved, then the first month's row covers only the days in the range (the daily amount multiplied by the days in that month within the range).
- AC5.1.5 Given the method None, when the contract is saved, then the expense schedule is empty.

## Group 5. Saving and viewing

### US6.1 Save the contract with its schedules
**As** Dana, **I want** the contract and both schedules saved together, **so that** what I see later is exactly what was generated.
**Priority:** Must Have
**Dependencies:** US4.1, US5.1
**Covers:** FR6.1, FR7.4, NFR2, NFR4

- AC6.1.1 Given a valid contract, when Dana saves it, then the contract, the payment schedule and the expense schedule are stored together and the screen then shows both schedules.
- AC6.1.2 Given a failure while storing any part, when Dana saves, then nothing is stored (no contract without its schedules) and Dana sees an error message.
- AC6.1.3 Given a saved contract, then the payment schedule total equals the sum of its streams' payments, and the expense schedule total equals the payment schedule total unless the method is None.

### US6.2 Open a saved contract
**As** Dana, **I want** to open a saved contract, **so that** I can read its details and both schedules later.
**Priority:** Must Have
**Dependencies:** US6.1
**Covers:** FR6.2, FR7.4

- AC6.2.1 Given a saved contract, when Dana opens it, then she sees its name, dates, payment streams, expense method, payment schedule and expense schedule.
- AC6.2.2 Given a contract id that does not exist, when Dana opens it, then she sees a message that the contract was not found.

### US6.3 List saved contracts
**As** Dana, **I want** a list of saved contracts, **so that** I can find the one I need.
**Priority:** Should Have
**Dependencies:** US6.1
**Covers:** FR6.3

- AC6.3.1 Given saved contracts, when Dana opens the list, then each contract shows its name, start date and end date.
- AC6.3.2 Given no saved contracts, when Dana opens the list, then she sees a message that there are none yet.

## INVEST Compliance Notes

- Independent: US6.2 and US6.3 depend only on US6.1; US4.1 and US5.1 are separate generation rules.
- Negotiable: UI layout is left to design; stories fix behavior, not screens.
- Valuable: each story gives Dana something she can see or check.
- Estimable and Small: each is one feature area slice, a day or two of work.
- Testable: every acceptance criterion is a Given/When/Then with concrete values where a calculation is involved.

## Won't Have (this version)

- Editing or deleting a saved contract, sign-in and roles, multiple currencies (see Requirements: Out of Scope).
