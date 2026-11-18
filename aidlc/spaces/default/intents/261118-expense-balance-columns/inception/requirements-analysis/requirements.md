# Requirements: Expense schedule balance columns and bracket negatives

## Intent Analysis

Initial description: "the expense schedule need these new columns: (Accrual)/Prepaid, Cumulative Balance, ST Accured Liability, LT Accrued Liability, ST Prepaid Asset, LT Prepaid Asset. Also negative value in schedules use (1000) instead of minus sign." [desc]

Dana wants the expense schedule to show, period by period, how far payments run ahead of or behind the expense, and how that difference sits on the balance sheet: as a prepaid asset or an accrued liability, split into the short-term and long-term parts. She also wants negative amounts shown the accounting way, in brackets.

This is an enhancement to the existing system (brownfield). It touches the schedule rules (u1-schedule-engine), the API responses for saved contracts and previews (u2-contract-api), and the schedule tables on the contract detail screen and in the preview popups (u3-contract-web). No stored data changes: the new columns are calculated from the payment and expense schedules that are already stored.

## Functional Requirements

### FR1. Expense schedule rows

- FR1.1 The expense schedule lists one row per calendar month that has an expense amount or a payment due, in date order. A month with payments but no expense shows an expense of 0. [Q3]
- FR1.2 For Overtime, a row covers the days of the expense range in that month, as today; a payment-only month covers the whole calendar month. For All at once, the expense row is the single expense date's month; payment months before and after it get their own rows. [Q3]
- FR1.3 A month with both a payment and an expense has one row, not two.
- FR1.4 When the expense method is None, there is no expense schedule, as today, and none of the new columns appear.

### FR2. New columns

The expense schedule shows, in this order: Period, Expense, (Accrual)/Prepaid, Cumulative Balance, ST Accrued Liability, LT Accrued Liability, ST Prepaid Asset, LT Prepaid Asset. [desc]

- FR2.1 **(Accrual)/Prepaid** for a row = total payments due in the row's month minus the row's expense. A positive amount is a prepayment; a negative amount is an accrual and shows in brackets. [Q1]
- FR2.2 **Cumulative Balance** for a row = the running total of (Accrual)/Prepaid from the first row up to and including this row. The last row's balance is 0, because total expense equals total payments. [Q2]
- FR2.3 The balance is split into a short-term (ST) and a long-term (LT) part. ST is the part of the row's balance that reverses within the 12 months after the row's period end, according to the later rows of the schedule. LT is the rest. ST + LT = the size of the balance. [Q4]
  - Worked example: at the end of January 2027 the balance is a prepaid 12,000. At the end of January 2028, 12 months later, the balance is a prepaid 3,000. So 9,000 reverses within 12 months: ST Prepaid Asset 9,000.00, LT Prepaid Asset 3,000.00.
  - When the balance 12 months later has the opposite sign or is 0, the whole balance is ST.
  - When no row exists exactly 12 months later, the balance at the latest row ending on or before that date is used. When the schedule ends within the 12 months, the whole balance is ST.
- FR2.4 A negative Cumulative Balance fills ST Accrued Liability and LT Accrued Liability, and the two Prepaid Asset columns show 0.00. A positive balance fills ST Prepaid Asset and LT Prepaid Asset, and the two Accrued Liability columns show 0.00. A balance of 0 shows 0.00 in all four. The four columns hold positive amounts. [Q5]
- FR2.5 The expense schedule has no total row. [Q7]
- FR2.6 The same columns and rows appear on the saved contract's Expense schedule tab and in the Expense schedule preview popup on the New contract form.

### FR3. Amount display

- FR3.1 Every amount in the payment schedule, the expense schedule and the preview popups shows with thousands separators and 2 decimals, for example 1,000.00. [Q6]
- FR3.2 A negative amount shows in brackets instead of a minus sign, for example (1,000.00). No amount shows a minus sign. [desc] [Q6]
- FR3.3 An amount that rounds to zero shows as 0.00, never (0.00) or -0.00.
- FR3.4 The payment schedule keeps its total row, with the total in the new format.

### FR4. Calculation

- FR4.1 The new columns are calculated on the server, from the same payment and expense schedules that are stored or previewed. The browser only displays them. [Q1] [Q2]
- FR4.2 All amounts are exact to 2 decimal places. The ST and LT parts of a row add up exactly to the size of the row's balance.
- FR4.3 A saved contract shows the same rows and values as its preview did for the same input.

## Non-Functional Requirements

- NFR1. Performance: reading a contract with 10 streams over 10 years, including the new columns, responds within 2 seconds, and the preview stays within its 2-second limit.
- NFR2. Compatibility: the API change only adds fields and rows to the expense schedule response within version 1. Stored data and the database schema do not change.
- NFR3. Correctness: for the same input the new columns are always the same. Unit tests cover the worked example in FR2.3, a sign change, a schedule that ends within 12 months, payment-only months, and the All at once method.
- NFR4. Accessibility: the wider expense table stays readable with a keyboard and a screen reader (WCAG 2.2 AA; axe finds no serious or critical issues). On a narrow screen it scrolls sideways rather than squashing.

## Constraints

- Existing stack: .NET 10, PostgreSQL, React with shadcn/ui.
- Team practices (team.md): test-after, every requirement has a test, `scripts/verify.ps1` stays green except for the accepted `npm audit` picomatch finding.

## Assumptions

- The 12-month window counts calendar months from the row's period end date: the end of January 2027 looks at the end of January 2028. Rationale: matches FR2.3's "within 12 months after the row's period end".
- Payments are assigned to rows by due date month. Rationale: the expense rows are monthly.
- The expense total row is removed from the preview popup too, so the two screens match (FR2.6).
- The payment schedule gets no new columns.

## Out of Scope

- Journal entries or export of the balances.
- Changing how the payment or expense amounts themselves are calculated.
- Storing the new columns in the database.

## Open Questions

- None.
