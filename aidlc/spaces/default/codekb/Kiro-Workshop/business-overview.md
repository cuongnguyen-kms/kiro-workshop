# Business Overview

## Domain
A contract management system. A user records a contract (name, start and end date), one or more payment streams, and an expense method. The system generates a payment schedule and an expense schedule and stores everything.

## Key Functionality
- Create a contract: `POST /api/v1/contracts` validates the input, generates both schedules, and saves contract, streams and schedules in one transaction (`ContractService.CreateAsync`).
- Preview schedules before saving: two buttons on the form ("Payment schedule", "Expense schedule") open a popup with the schedules of the values entered so far. `POST /api/v1/contracts/preview` runs the same validation and generation as create and stores nothing (`ContractService.Preview`, `SchedulePreview.tsx`).
- List contracts, newest start date first, paged (10, 20, 50 or 100 per page).
- View one contract with its streams, payment schedule and expense schedule (tabs on the detail screen).

## Business Rules (from code)
- Payment frequencies: Weekly, Monthly, Quarterly, Annually, OneTime. A OneTime stream stores its start date as its end date.
- Expense methods: Overtime (spread per day, one row per calendar month over a date range), AllAtOnce (one date), None (no expense schedule). Expense total equals payment total unless the method is None.
- Stream and expense dates must lie within the contract dates; amounts are above zero with at most 2 decimals.
- A contract that would create more than 10,000 schedule rows is refused.
- A payment preview ignores the expense fields; the name is not needed for a preview.
- Payments and expense rows are two independent lists. Nothing in the code relates a payment to an expense period, and there is no accrual, prepaid, balance or short-term/long-term concept yet. Amounts shown are always positive today.

## Active intent (expense-balance-columns)
Add expense schedule columns (Accrual)/Prepaid, Cumulative Balance, ST and LT Accrued Liability, ST and LT Prepaid Asset, and show negative values in schedules as (1000) instead of a minus sign. Where the code stands: see `architecture.md` (Expense schedule display and data path) and `code-quality-assessment.md`.

Details: rules in `api-documentation.md`; components in `component-inventory.md`.
