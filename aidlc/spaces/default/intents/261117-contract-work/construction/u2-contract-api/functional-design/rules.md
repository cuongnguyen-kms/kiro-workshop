# Business Rules: u2-contract-api

Message wording comes from the Refined Mockups interaction spec and is part of contract C1. Field paths are camelCase, for example `paymentStreams[0].amount`. All validation errors are collected and returned together in one 400 response, not one at a time.

```yaml
rules:
  - id: BR1.1
    statement: A contract name is required.
    category: validation
    applies_to: Contract.name
    trigger: create contract
    logic: IF name is missing, blank, or only whitespace, or longer than 200 characters after trimming THEN reject.
    violation: "400, errors.name = ['Contract name is required.'] (too long: 'Contract name must be 200 characters or fewer.')"
    source: FR1.1, NFR6
  - id: BR1.2
    statement: The contract end date must not be before its start date.
    category: validation
    applies_to: Contract.startDate, Contract.endDate
    trigger: create contract
    logic: IF either date is missing or not a valid calendar date, or endDate is before startDate THEN reject.
    violation: "400, errors.endDate = ['End date must not be before the start date.'] (a missing or invalid date: 'Enter a valid date.')"
    source: FR1.2, NFR6
  - id: BR1.3
    statement: Contract names do not have to be unique.
    category: policy
    applies_to: Contract.name
    trigger: create contract
    logic: IF another contract has the same name THEN accept anyway; the contract id is the identity.
    violation: none
    source: Q1 (A)
  - id: BR2.1
    statement: A contract has at least one payment stream.
    category: validation
    applies_to: Contract.paymentStreams
    trigger: create contract
    logic: IF the stream list is missing or empty THEN reject.
    violation: "400, errors.paymentStreams = ['Add at least one payment stream.']"
    source: FR1.3, NFR6
  - id: BR2.2
    statement: A stream amount is greater than zero and has at most 2 decimal places.
    category: validation
    applies_to: PaymentStream.amount
    trigger: create contract
    logic: IF amount is missing, zero or negative THEN reject; IF amount has more than 2 decimal places THEN reject.
    violation: "400, errors.paymentStreams[i].amount = ['Amount must be greater than zero.'] (more than 2 decimals: 'Amount can have at most 2 decimal places.')"
    source: FR2.1, FR5.5, NFR6
  - id: BR2.3
    statement: A stream's dates are within the contract dates.
    category: validation
    applies_to: PaymentStream.startDate, PaymentStream.endDate
    trigger: create contract
    logic: IF stream startDate or endDate is before contract startDate or after contract endDate THEN reject. Skipped when the contract dates themselves are invalid (BR1.2), so one mistake gives one message.
    violation: "400, errors.paymentStreams[i].startDate or .endDate = ['Stream dates must be within the contract dates.']"
    source: FR2.4, NFR6
  - id: BR2.4
    statement: A repeating stream has an end date that is not before its start date.
    category: validation
    applies_to: PaymentStream.endDate
    trigger: create contract
    logic: IF frequency is not OneTime AND endDate is missing THEN reject; IF endDate is before startDate THEN reject.
    violation: "400, errors.paymentStreams[i].endDate = ['End date must not be before the start date.'] (missing: 'Enter a valid date.')"
    source: FR2.1, FR4.2, NFR6
  - id: BR2.5
    statement: A One-time stream stores its start date as its end date.
    category: calculation
    applies_to: PaymentStream.endDate
    trigger: create contract
    logic: IF frequency is OneTime THEN ignore any submitted endDate and store endDate = startDate.
    violation: none
    source: FR2.3
  - id: BR2.6
    statement: Frequency is one of Weekly, Monthly, Quarterly, Annually or OneTime.
    category: validation
    applies_to: PaymentStream.frequency
    trigger: create contract
    logic: IF frequency is missing or not one of the five values (exact spelling) THEN reject.
    violation: "400, errors.paymentStreams[i].frequency = ['Choose a frequency.']"
    source: FR2.2, NFR6
  - id: BR3.1
    statement: The expense method is one of Overtime, AllAtOnce or None.
    category: validation
    applies_to: Contract.expenseMethod
    trigger: create contract
    logic: IF method is missing or not one of the three values THEN reject.
    violation: "400, errors.expense.method = ['Choose an expense method.']"
    source: FR1.4, NFR6
  - id: BR3.2
    statement: Overtime needs an expense start date and an expense end date that is not before it.
    category: validation
    applies_to: Contract.expenseStartDate, Contract.expenseEndDate
    trigger: create contract with method Overtime
    logic: IF either date is missing THEN reject; IF expense end date is before expense start date THEN reject.
    violation: "400, errors.expense.endDate = ['Expense end date must not be before the start date.'] (missing: 'Enter a valid date.')"
    source: FR3.1, NFR6
  - id: BR3.3
    statement: All at once needs a single expense date.
    category: validation
    applies_to: Contract.expenseDate
    trigger: create contract with method AllAtOnce
    logic: IF date is missing THEN reject.
    violation: "400, errors.expense.date = ['Enter a valid date.']"
    source: FR3.2, NFR6
  - id: BR3.4
    statement: Expense dates are within the contract dates.
    category: validation
    applies_to: Contract.expenseStartDate, Contract.expenseEndDate, Contract.expenseDate
    trigger: create contract with method Overtime or AllAtOnce
    logic: IF any expense date is before contract startDate or after contract endDate THEN reject. Skipped when the contract dates themselves are invalid (BR1.2).
    violation: "400, errors.expense.startDate, .endDate or .date = ['Expense dates must be within the contract dates.']"
    source: FR3.5, NFR6
  - id: BR3.5
    statement: With method None, or with dates that do not belong to the method, those dates are ignored and not stored.
    category: policy
    applies_to: Contract.expenseStartDate, Contract.expenseEndDate, Contract.expenseDate
    trigger: create contract
    logic: IF method is None THEN store no expense dates; IF method is Overtime THEN ignore any date field; IF method is AllAtOnce THEN ignore any start or end date. Ignored fields are never validated.
    violation: none
    source: FR3.3
  - id: BR4.1
    statement: Both schedules are generated by the schedule engine from valid input, in the order it returns them.
    category: calculation
    applies_to: PaymentScheduleEntry, ExpenseScheduleEntry
    trigger: create contract that passed every validation rule
    logic: THEN map streams and expense to the C2 ScheduleRequest, call IScheduleGenerator.Generate once, and store its payments and expenses unchanged. U2 never recomputes dates or amounts. The same input always gives the same rows.
    violation: "If the engine throws, the save fails with 500 and nothing is stored (BR5.1)."
    source: FR4.1, FR4.2, FR4.3, FR5.1, FR5.2, FR5.3, FR5.4, FR5.5, NFR4
  - id: BR4.2
    statement: Schedule totals agree with the stored rows.
    category: constraint
    applies_to: Contract read and create responses
    trigger: create or read contract
    logic: payment total = sum of payment rows. Expense total = sum of expense rows, which equals the payment total unless the method is None (then 0). Totals are computed from the rows on every response, never stored separately.
    violation: "If the engine's result breaks this, the save fails with 500 and nothing is stored."
    source: FR3.4, FR5.5, NFR4
  - id: BR5.1
    statement: A contract and its streams and both schedules are saved together or not at all.
    category: constraint
    applies_to: Contract aggregate
    trigger: create contract
    logic: Save the contract, streams, payment rows and expense rows in one database transaction. IF any part fails THEN roll back the whole transaction.
    violation: "500 Problem Details; nothing is stored. The client must not retry on its own."
    source: FR6.1
  - id: BR5.2
    statement: A saved contract is read back with every part, or reported as not found.
    category: policy
    applies_to: Contract read
    trigger: get contract by id
    logic: IF id is not a valid identifier or no contract has it THEN return 404 'Contract not found.'; ELSE return the contract with its streams, expense, and both schedules in stored order.
    violation: "404 Problem Details, title 'Contract not found.'"
    source: FR6.2
  - id: BR5.3
    statement: The list is paged, sorted so pages stay stable.
    category: policy
    applies_to: Contract list
    trigger: list contracts
    logic: pageSize must be 10, 20, 50 or 100 (default 10) and page at least 1 (default 1), else 400. Sort by startDate descending, then name ascending, then contractId. totalPages = ceil(totalCount / pageSize). A page past the last returns no items. No contracts gives an empty page, which the UI shows as the empty state.
    violation: "400, errors.page or errors.pageSize = ['Page must be 1 or more.'] / ['Page size must be 10, 20, 50 or 100.']"
    source: FR6.3
  - id: BR5.4
    statement: A contract may not generate more than 10,000 schedule rows in total.
    category: constraint
    applies_to: Contract.paymentStreams
    trigger: create contract that passed BR2.x and BR3.x
    logic: Before calling the engine, estimate the rows each stream would produce from its frequency and date range (days / 7 for Weekly, months / 1, 3, 12 for the others, 1 for OneTime, rounded up) and add the expense rows (months in range, 1, or 0). IF the sum is over 10,000 THEN reject. There is no separate cap on the number of streams (Q2). The estimate never undercounts, so generation never runs on an oversized contract.
    violation: "400, errors.paymentStreams = ['This contract would create more than 10,000 schedule rows. Shorten the dates or use a less frequent payment.']"
    source: Q2 (A), NFR5
  - id: BR6.1
    statement: Cross-origin calls are allowed from the configured frontend origin only.
    category: authorization
    applies_to: HTTP API
    trigger: any browser request
    logic: Read the allowed origin from configuration. Allow only that origin, GET and POST, and the Content-Type header. IF no origin is configured THEN allow none.
    violation: browser blocks the call; no data is exposed to other origins.
    source: Contract C1, Q2 (Units Generation)
  - id: BR6.2
    statement: Database settings and the password come from configuration, never from source.
    category: policy
    applies_to: PostgreSQL connection
    trigger: API start
    logic: Read the connection from configuration (user secrets or environment variable). Local default for development, from Q3 (A) - host localhost, port 5432, user postgres, database contract_management; the password is supplied through configuration only. IF the database is missing in development THEN create it and its tables at start. IF the connection fails at start THEN the API fails to start with a clear log line that does not print the password.
    violation: API does not start.
    source: Q3 (A), NFR2, Construction security rules
  - id: BR6.3
    statement: Unexpected failures return a generic problem response.
    category: policy
    applies_to: HTTP API
    trigger: any unhandled error
    logic: Return 500 Problem Details with a generic title and no stack trace, SQL text or connection details; log the details on the server.
    violation: none
    source: Contract C1, NFR6
```

## Rules summary

| Group | Rules | What they cover |
|---|---|---|
| Contract | BR1.1 to BR1.3 | name, dates, duplicate names allowed |
| Streams | BR2.1 to BR2.6 | at least one, amount, dates, One-time end date, frequency |
| Expense | BR3.1 to BR3.5 | method, Overtime range, All at once date, within contract, ignored fields |
| Generation | BR4.1, BR4.2 | engine call, totals |
| Save and read | BR5.1 to BR5.4 | atomic save, read, list and paging, size cap |
| Platform | BR6.1 to BR6.3 | CORS, configuration and password, error responses |
