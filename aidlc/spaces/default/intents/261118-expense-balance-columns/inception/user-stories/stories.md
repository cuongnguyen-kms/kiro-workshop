# User Stories

Persona: P1 Dana, Contract Manager (see personas.md). Stories are grouped by feature area. Priorities use MoSCoW. Amounts in the examples use the new format from US3.1.

## Group 1. Expense schedule rows

### US1.1 See every month with a payment or an expense
**As** Dana, **I want** the expense schedule to list every month that has an expense or a payment, **so that** I can see the months where payments run ahead of or behind the expense.
**Priority:** Must Have
**Dependencies:** none
**Covers:** FR1.1, FR1.2, FR1.3, FR1.4

- AC1.1.1 Given an Overtime expense from 2027-03-01 to 2027-05-31 and a One-time payment of 3,000.00 on 2027-01-15, when the schedule is shown, then it has rows for January, March, April and May 2027 in date order, and the January row shows an expense of 0.00.
- AC1.1.2 Given a Monthly payment and an Overtime expense in the same months, when the schedule is shown, then each month appears once, with that month's expense.
- AC1.1.3 Given an All at once expense on 2027-06-30 and Quarterly payments from 2027-01-01 to 2027-12-31, when the schedule is shown, then it has rows for January, April, June, July and October 2027, and only the June row has an expense.
- AC1.1.4 Given the expense method None, when the contract is shown, then there is no expense schedule and none of the new columns appear.

## Group 2. Balance columns

### US2.1 See the (Accrual)/Prepaid amount and the running balance
**As** Dana, **I want** each row to show the month's payments minus its expense and the running balance, **so that** I know how far the contract is paid ahead or behind at each month end.
**Priority:** Must Have
**Dependencies:** US1.1
**Covers:** FR2.1, FR2.2, FR2.6, FR4.1, FR4.2, FR4.3

- AC2.1.1 Given a payment of 12,000.00 in January 2027 and an Overtime expense of 1,000.00 a month from January to December 2027, when the schedule is shown, then January shows (Accrual)/Prepaid 11,000.00 and Cumulative Balance 11,000.00, February shows (1,000.00) and 10,000.00, and December shows (1,000.00) and 0.00.
- AC2.1.2 Given Monthly payments of 1,000.00 from April to December 2027 and an All at once expense of 9,000.00 on 2027-01-31, when the schedule is shown, then January shows (9,000.00) and a balance of (9,000.00), and the balance rises by 1,000.00 each month to 0.00 in December.
- AC2.1.3 Given any contract with an expense schedule, when the schedule is shown, then the last row's Cumulative Balance is 0.00.
- AC2.1.4 Given the same contract input, when Dana previews the expense schedule and then saves the contract, then the preview popup and the saved Expense schedule tab show the same rows, columns and values.

### US2.2 See the balance split into short-term and long-term assets and liabilities
**As** Dana, **I want** each row's balance split into ST and LT Prepaid Asset or ST and LT Accrued Liability, **so that** I can give finance the balance sheet figures for that month end.
**Priority:** Must Have
**Dependencies:** US2.1
**Covers:** FR2.3, FR2.4

- AC2.2.1 Given a prepaid balance of 12,000.00 at the end of January 2027 and a prepaid balance of 3,000.00 at the end of January 2028, when the schedule is shown, then the January 2027 row shows ST Prepaid Asset 9,000.00, LT Prepaid Asset 3,000.00, and 0.00 in both Accrued Liability columns.
- AC2.2.2 Given an accrued balance of (6,000.00) at a row's period end that is fully paid within the next 12 months, when the schedule is shown, then that row shows ST Accrued Liability 6,000.00, LT Accrued Liability 0.00, and 0.00 in both Prepaid Asset columns.
- AC2.2.3 Given a prepaid balance at a row's period end and an accrued balance (or 0.00) 12 months later, when the schedule is shown, then the whole balance is ST Prepaid Asset and LT Prepaid Asset is 0.00.
- AC2.2.4 Given a row whose schedule ends within the next 12 months, when the schedule is shown, then the whole balance is short term.
- AC2.2.5 Given a row with a balance of 0.00, when the schedule is shown, then all four ST and LT columns show 0.00.
- AC2.2.6 Given any row, when the schedule is shown, then the ST and LT amounts are positive and add up exactly to the size of the row's balance.

### US2.3 Read the expense schedule without a total row
**As** Dana, **I want** the expense schedule without a total row, **so that** the balance columns are not mixed up with totals that mean nothing for them.
**Priority:** Must Have
**Dependencies:** US2.1
**Covers:** FR2.5

- AC2.3.1 Given a saved contract with an expense schedule, when Dana opens the Expense schedule tab, then the table ends with the last month's row and has no total row.
- AC2.3.2 Given the Expense schedule preview popup, when it opens, then the table has no total row.

## Group 3. Amount display

### US3.1 Read negative amounts in brackets
**As** Dana, **I want** negative amounts shown in brackets and all amounts with thousands separators, **so that** the schedules read like the accounting reports I am used to.
**Priority:** Must Have
**Dependencies:** none
**Covers:** FR3.1, FR3.2, FR3.3, FR3.4

- AC3.1.1 Given an amount of -1000, when it appears in any schedule or popup, then it reads (1,000.00), with no minus sign.
- AC3.1.2 Given an amount of 1234567.5, when it appears, then it reads 1,234,567.50.
- AC3.1.3 Given an amount that is 0 or rounds to 0, when it appears, then it reads 0.00, never (0.00) or -0.00.
- AC3.1.4 Given the payment schedule of a saved contract or of the preview, when it is shown, then it keeps its total row, with the total in the new format.

## Group 4. Quality

### US4.1 Use the wider table comfortably
**As** Dana, **I want** the wider expense table to stay readable and quick, **so that** long contracts are still easy to check.
**Priority:** Should Have
**Dependencies:** US2.1
**Covers:** NFR1, NFR2, NFR3, NFR4

- AC4.1.1 Given a contract with 10 streams over 10 years, when Dana opens it or previews its expense schedule, then the schedule appears within 2 seconds.
- AC4.1.2 Given a narrow window, when the expense table is shown, then it scrolls sideways and every column header stays readable.
- AC4.1.3 Given the expense table on the tab or in the popup, when it is checked with axe, then there are no serious or critical issues, and every column has a header a screen reader announces.
- AC4.1.4 Given an existing saved contract, when it is opened after this change, then it shows the new columns without any data being migrated.

## INVEST Compliance Notes

- Independent: US3.1 stands alone; US1.1 can ship before the balance columns; US2.2 and US2.3 build on US2.1 only.
- Negotiable: the stories fix the numbers and the rules, not the table layout.
- Valuable: each gives Dana a figure or a format she uses at month end.
- Estimable and Small: each is one rule or one display change.
- Testable: every criterion has concrete amounts and dates.

## Won't Have (this version)

- Journal entries or exports of the balances; storing the balances; changes to how payments or expenses are calculated.
