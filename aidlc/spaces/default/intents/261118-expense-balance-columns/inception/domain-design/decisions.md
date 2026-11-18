# Architecture Decision Records: expense balance columns

## ADR-001: Balance rules live in the ScheduleGenerator library

- **Context:** The new columns need payments and expenses compared month by month and a 12-month look-ahead (requirements FR1, FR2). Today the payment and expense rules live in the pure ScheduleGenerator library (earlier ADR-001 of the contract work: separate and testable without a database). The rules must give the same numbers for a preview and for a saved contract (FR4.3).
- **Decision:** Add a balance-row builder to ScheduleGenerator. It takes a payment schedule and an expense schedule and returns the balance rows. The generator's preview result includes them; ContractService also calls the builder with the stored rows when a saved contract is read (answer to Q1, option A).
- **Consequences:** Positive: all money rules sit in one pure, deterministic library with its own fast tests; preview and saved views use the same code, so they match. Negative: the library's public interface (contract C2) grows, so Contract Design must update C2; the library becomes responsible for one more rule set.
- **Alternatives Rejected:** In ContractService (Q1 B): rules split between two places and only testable with the API's test setup. A new separate library (Q1 C): an extra project and dependency for one function that uses the same types.

## ADR-002: Balance rows are calculated, never stored

- **Context:** Saved contracts already store their payment and expense rows. The balance rows follow from those rows alone (FR4.1). Existing contracts must show the columns without migration (AC4.1.4).
- **Decision:** Do not store balance rows. Build them from the stored rows each time a contract is read, and from the generated rows for a preview.
- **Consequences:** Positive: no schema change, no migration, no risk of stored balances drifting from the rows. Negative: a small amount of work on every read (at most a few hundred monthly rows for a 10-year contract, well inside the 2-second target, NFR1).
- **Alternatives Rejected:** Storing the balance columns in the expense table: needs a migration and a backfill for existing contracts, and two sources of truth for the same numbers.

## ADR-003: The browser formats money; it never calculates it

- **Context:** The bracket format (FR3) is a display concern. The existing rule says the frontend only displays amounts (code knowledge base, ContractWebApp).
- **Decision:** One shared money formatter in ContractWebApp applies the new format to every amount. All values, including balances, come from the API.
- **Consequences:** Positive: one change point for the format, consistent everywhere, no money logic in the browser. Negative: the existing frontend tests that check the old number strings must be updated (team.md, Testing Posture).
- **Alternatives Rejected:** The API sending pre-formatted strings: breaks the contract's use of JSON numbers and ties the API to one display style.
