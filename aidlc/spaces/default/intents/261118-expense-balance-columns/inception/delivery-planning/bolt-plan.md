# Bolt Plan: expense balance columns

A Bolt is one build pass over a piece of the work, ending in something that runs. Each Bolt here is one Unit of Work (from `inception/units-generation/unit-of-work.md`), built one at a time in risk-first order (answer to Q1). The FR and NFR IDs come from `inception/requirements-analysis/requirements.md`, and the component names from `inception/domain-design/components.md`. There is no walking skeleton (a thin end-to-end slice built first): the team does not use one (team.md), and the app already runs end to end.

## Bolt 1. U1 u1-schedule-balance

- **Units:** U1 (library).
- **Walking skeleton:** no.
- **Definition of Done:** the balance-row builder (contract C2) exists and `Generate` returns its rows; unit tests cover every story example (AC1.1.1, AC1.1.3, AC2.1.1, AC2.1.2, AC2.2.1 to AC2.2.6), payment-only months, All at once, a sign change, a schedule that ends within 12 months, the look-ahead to a month with no row, and a leap-year month end; all existing schedule-library tests still pass.
- **Confidence hypothesis:** the accounting rules (FR2.1 to FR2.4) can be expressed exactly in the pure library, with ST and LT always adding up to the balance.
- **Expected demo:** the U1 test run, with the worked examples from the stories passing.

## Bolt 2. U2 u2-api-balance

- **Units:** U2 (service).
- **Walking skeleton:** no.
- **Definition of Done:** the contract detail and preview responses carry `expenseSchedule.balanceRows` (contract C1 v1.1.0); a saved contract created before this change shows balance rows without migration; integration tests on the local PostgreSQL show the same rows for a preview and the saved contract; reads and previews stay within 2 seconds for 10 streams over 10 years; all existing API tests still pass.
- **Confidence hypothesis:** rebuilding balance rows from stored rows at read time is fast and gives the same result as the preview (FR4.3, NFR1, NFR2).
- **Expected demo:** a `GET /api/v1/contracts/{id}` response for an existing contract with the new rows.

## Bolt 3. U3 u3-web-balance

- **Units:** U3 (ui).
- **Walking skeleton:** no.
- **Definition of Done:** the 8-column expense table on the tab and in the 80% wide popup, with no expense total row; the bracket money format everywhere; the payment schedule keeps its total row; component tests and axe checks pass in jsdom; updated existing tests pass; `scripts/verify.ps1` passes apart from the accepted `npm audit` finding.
- **Confidence hypothesis:** the wider table stays readable and accessible, and the new format reads correctly on every screen (FR2.5, FR2.6, FR3, NFR4).
- **Expected demo:** the running app showing a saved contract's expense tab and a preview popup with the new columns.

## Construction settings

- Iteration: one Unit at a time through its design stages and Code Generation (unit-major), serial execution (team.md, Deployment and Way of Working).
- Integration branch: `development` (team.md).
