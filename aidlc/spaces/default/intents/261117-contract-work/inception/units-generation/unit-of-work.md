# Units of Work

Three units, grouped from the Domain Design components (Q1, option A). The API and the React app are deployed separately (Q2, option B).

| Unit ID | Directory | Kind | Components | Deployment | Complexity |
|---|---|---|---|---|---|
| U1 | u1-schedule-engine | library | ScheduleGenerator | Embedded: a .NET class library referenced by U2 | M |
| U2 | u2-contract-api | service | ContractApi, ContractService, ContractRepository | Standalone: .NET 10 web API with PostgreSQL | L |
| U3 | u3-contract-web | ui | ContractWebApp | Standalone: static React build served on its own address | M |

## U1. u1-schedule-engine (library)

- **Description:** pure schedule rules. Takes validated payment streams and an expense method and returns the payment schedule and the expense schedule.
- **Boundaries:** no database, file, clock or network access (ADR-001, NFR7). No HTTP types.
- **Responsibilities:**
  - Payment dates for Weekly, Monthly, Quarterly, Annually and One-time streams, inclusive of the stream end date (FR4.1, FR4.2).
  - Last-day-of-month rule for Monthly, Quarterly and Annually streams (FR4.3).
  - Expense schedule for All at once, Overtime (equal per day, summed per calendar month) and None (FR5.1 to FR5.4).
  - Expense amount is the total of the payment schedule (FR3.4); rounding to 2 decimals with the difference on the last row (FR5.5, NFR4).
- **Implementation notes:** `decimal` for all money. Deterministic output for the same input. Unit tests run without any other unit.

## U2. u2-contract-api (service)

- **Description:** the .NET 10 backend that validates, generates and stores contracts.
- **Boundaries:** owns the Contract aggregate (ADR-002) and its PostgreSQL tables. Calls U1 for schedules. Exposes HTTP endpoints only (ADR-003).
- **Responsibilities:**
  - Endpoints to create a contract, list contracts and read one contract (FR6.2, FR6.3).
  - Validation of every contract, stream and expense rule with the agreed messages (FR1 to FR3, NFR6).
  - Generate both schedules through U1 and save the contract with them in one transaction (FR6.1).
  - Allow cross-origin calls from the configured frontend address only, because the frontend is deployed separately (Q2).
- **Implementation notes:** PostgreSQL (NFR2); no sign-in in this version, but endpoints are structured so authentication can be added later (NFR3). Performance target from NFR5.

## U3. u3-contract-web (ui)

- **Description:** the React frontend with shadcn/ui components.
- **Boundaries:** talks to U2 only through the HTTP API defined in Contract Design. Never generates schedules; displays what the API returns.
- **Responsibilities:**
  - Screens S1 (contracts list), S2 (new contract form) and S3 (contract detail with schedule tabs) from Refined Mockups.
  - Client-side validation with the same messages as the backend (FR7.1 to FR7.3).
  - Loading, empty, error and success states; WCAG 2.2 AA (accessibility checklist).
- **Implementation notes:** the API base address is configuration, not code. Tests use a fake API that follows the Contract Design contract, so U3 builds and passes without U2.
