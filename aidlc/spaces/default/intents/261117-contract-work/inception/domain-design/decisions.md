# Architecture Decision Records

## ADR-001: Keep schedule generation in its own component

- **Context:** Payment and expense schedules follow date, split and rounding rules (FR4, FR5). NFR7 asks for these rules to be testable without a database, and NFR4 requires exact, repeatable totals.
- **Decision:** A ScheduleGenerator component holds all schedule rules and has no database, clock or network access. ContractService calls it and passes the results to storage.
- **Consequences:**
  - Positive: rules are tested on their own with plain inputs and outputs. Rule changes do not touch storage code.
  - Negative: one more component and one more call boundary to maintain.
- **Alternatives Rejected:**
  - Generation inside ContractService: fewer building blocks, but testing the rules needs the service or a fake store, and rules and orchestration would change together.

## ADR-002: Schedule rows belong to the Contract aggregate

- **Context:** The contract and both schedules must be saved in one operation (FR6.1) and are read back together (FR6.2). This version only creates and reads contracts; editing is out of scope.
- **Decision:** Contract, PaymentStream, PaymentScheduleEntry and ExpenseScheduleEntry form one aggregate owned by ContractService, stored and loaded as one unit by ContractRepository.
- **Consequences:**
  - Positive: one owner, one transaction, no schedule can exist without its contract.
  - Negative: if schedules are edited on their own later, the aggregate boundary will need revisiting.
- **Alternatives Rejected:**
  - A separate Schedule component with its own storage: allows independent schedule edits, but needs extra work to keep schedules in step with the contract, for a feature that is out of scope.

## ADR-003: Separate the HTTP layer from the rules

- **Context:** The backend exposes create, list and read operations to a React app. Request shapes and HTTP status mapping change for different reasons than contract rules.
- **Decision:** ContractApi owns request and response shapes and the mapping of outcomes to HTTP responses. ContractService owns all rules and returns outcomes, not HTTP results. Calls run in one direction: WebApp to Api to Service to Generator and Repository, with no cycles.
- **Consequences:**
  - Positive: rules can be reused or tested without HTTP. The dependency graph stays acyclic.
  - Negative: a small amount of mapping code between request shapes and domain objects.
- **Alternatives Rejected:**
  - Rules inside the endpoint handlers: fewer files, but rules would be tied to HTTP and hard to test alone.
