# Bolt Plan

A Bolt is one build pass over one or more units of work that ends in something that runs and is tested. This plan has two Bolts (Q2), ordered risk first (Q1). The classic plan has no walking skeleton (a first thin end-to-end slice), so neither Bolt carries that marker.

Construction itself builds units in the order of the unit dependency graph: U1, then U2, then U3. That order matches this plan.

## Bolt 1: Backend with schedule rules

- **Units:** U1 `u1-schedule-engine` (library) and U2 `u2-contract-api` (service). U1 is built first inside the Bolt because U2 depends on it.
- **Walking skeleton:** no.
- **Stories:** US4.1 and US5.1 (U1), and all of US1.1 to US6.3 for the backend side (U2).
- **Definition of Done:**
  - U1 unit tests pass for every acceptance criterion in US4.1 and US5.1. That includes the month-end payment (AC4.1.2), the Overtime rounding on the last row (AC5.1.3) and the mid-month start (AC5.1.4).
  - U2 implements contract C1 exactly: `POST /api/v1/contracts`, `GET /api/v1/contracts` with page sizes 10, 20, 50 and 100 (default 10), and `GET /api/v1/contracts/{id}`.
  - Validation returns RFC 9457 Problem Details with the agreed messages.
  - A failed save stores nothing (AC6.1.2), and a test proves it.
  - Backend integration tests run against a local PostgreSQL install whose connection string comes from configuration (Q3). No credentials are in the code.
  - A `scripts/verify.ps1` script exists that runs the solution's tests, and the frontend tests once the frontend exists. It is the verification command for every unit.
- **Confidence hypothesis:** the schedule rules give exact, repeatable totals for every frequency and expense method, and a whole contract with its schedules saves and reads back atomically from PostgreSQL.
- **Expected demo:** call `POST /api/v1/contracts` with a Monthly stream starting on the 31st and an Overtime expense. Show the February payment on the 28th and the expense rows adding up exactly to the payment total. Then read the contract back by id and through the paged list.

## Bolt 2: Frontend

- **Units:** U3 `u3-contract-web` (ui).
- **Walking skeleton:** no.
- **Stories:** US1.1, US2.1, US3.1, US6.1, US6.2 and US6.3 (frontend side).
- **Definition of Done:**
  - Screens S1, S2 and S3 are built with shadcn/ui, as in the Refined Mockups.
  - The contracts list has a pager with a page-size select offering 10, 20, 50 and 100, defaulting to 10 (from Contract Design).
  - Field validation shows the same messages as the backend, and backend Problem Details errors appear next to the matching field.
  - Component tests pass against a fake API that follows contract C1.
  - Automated accessibility checks (axe) pass with no serious or critical issues.
  - Pointed at the Bolt 1 backend, Dana can create a contract and see both schedules.
- **Confidence hypothesis:** Dana can enter a contract end to end in the browser, see correct schedules, and recover from validation and save errors without losing her entries.
- **Expected demo:** in the browser, enter a contract with two streams and an All at once expense, save it, see both schedule tabs, then find it in the paged list.
