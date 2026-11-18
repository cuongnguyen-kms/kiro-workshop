# Units of Work: expense balance columns

Three units, one per existing project, matching how the earlier contract work was split (approved plan). Components come from `inception/domain-design/components.md`; the FR and NFR IDs below come from `inception/requirements-analysis/requirements.md`.

| Unit ID | Directory | Kind | Components | Deployment | Complexity |
|---|---|---|---|---|---|
| U1 | u1-schedule-balance | library | ScheduleGenerator | Embedded: part of the existing .NET class library referenced by the API | M |
| U2 | u2-api-balance | service | ContractApi, ContractService (ContractRepository unchanged) | Standalone: the existing .NET 10 web API | S |
| U3 | u3-web-balance | ui | ContractWebApp | Standalone: the existing React app | M |

## U1. u1-schedule-balance (library)

- **Description:** the balance rules in the schedule library (ADR-001).
- **Boundaries:** pure and deterministic; no database, file, clock or network access (existing banned-API rule). No HTTP types.
- **Responsibilities:**
  - One balance-row builder that takes a payment schedule and an expense schedule and returns the balance rows. Used for both previews and saved contracts (FR4.3).
  - One row per calendar month with a payment or an expense, in date order; expense 0 for payment-only months; one row per month (FR1.1 to FR1.3). No rows for method None (FR1.4).
  - (Accrual)/Prepaid, Cumulative Balance ending at 0, ST and LT split with the 12-month look-ahead, the carry-forward from the latest earlier row, and the "schedule ends within 12 months" rule; asset or liability columns by sign; ST and LT never negative and adding up to the balance (FR2.1 to FR2.4, FR4.2).
  - Include the balance rows in the generator's result for previews.
- **Implementation notes:** `decimal` throughout; exact to 2 decimals. Unit tests cover the worked examples in the stories, a sign change, payment-only months, All at once, a schedule ending within 12 months, and the look-ahead to a month without a row (NFR3).

## U2. u2-api-balance (service)

- **Description:** wire the balance rows into the API responses.
- **Boundaries:** owns the Contract aggregate, unchanged; stores nothing new (ADR-002). Calls U1.
- **Responsibilities:**
  - For a saved contract, build the balance rows from the stored payment and expense rows with U1's builder and add them to the expense schedule part of the contract detail response (FR4.1, AC4.1.4).
  - For a preview, add the balance rows from the generator's result to the preview response (FR2.6).
  - Additive change within API v1; existing fields unchanged (NFR2).
- **Implementation notes:** keep within the 2-second targets for read and preview (NFR1). Integration tests on the local PostgreSQL for a saved contract and for a preview giving the same rows.

## U3. u3-web-balance (ui)

- **Description:** show the balance table and the new money format.
- **Boundaries:** talks to U2 only through the HTTP API; never calculates money (ADR-003).
- **Responsibilities:**
  - The 8-column expense table on the Expense schedule tab and in the Expense schedule preview popup, no total row, Period sticky on the left, sideways scroll, popup up to 80% of the screen width (FR2.5, FR2.6, refined mockups M1 and M2).
  - The shared money format: thousands separators, 2 decimals, brackets for negatives, 0.00 for zero, used for every amount (FR3.1 to FR3.4).
  - Accessibility per the refined mockups checklist (NFR4).
- **Implementation notes:** the shared table component gains several right-aligned amount columns and an optional total row. Existing tests that check the old number strings are updated. Tests use the fake API, extended with the balance rows from the API contract, so U3 builds and passes without U2.
