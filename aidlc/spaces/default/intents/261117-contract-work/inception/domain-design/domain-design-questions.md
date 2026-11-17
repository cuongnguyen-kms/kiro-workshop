# Domain Design Questions

Context: the contract management system has to turn a contract's payment streams and expense method into a payment schedule and an expense schedule, then store all of it in one save (FR4, FR5, FR6.1).

## Q1. [Architect] How should schedule generation be separated from the rest of the backend?

Requirement NFR7 asks for schedule generation that can be tested without a database. Where the date and rounding rules live decides how easy that is.

Option A, separate component: a ScheduleGenerator component holds all date, split and rounding rules and has no database access. ContractService calls it. Easy to test alone; one more building block.
Option B, inside ContractService: generation is part of the one component that saves contracts. Fewer building blocks; testing the rules needs the service or a fake store.
Recommendation: A, because the rules change at a different rate from storage and NFR7 asks for it.

A. Separate ScheduleGenerator component
B. Generation inside ContractService
X. Other (please specify)

[Answer]: A

## Q2. [Architect] Who owns the schedule rows (payment and expense rows) once they are generated?

The rows must be saved with the contract in one operation (FR6.1) and are read back with it (FR6.2).

Option A, part of the Contract: the Contract owns its streams, payment rows and expense rows. Saved and loaded as one unit. Simple; schedules are never edited on their own.
Option B, a separate Schedule component: schedules have their own owner and storage. Allows editing schedules later; needs extra work to keep them in step with the contract.
Recommendation: A, because this version only creates and reads contracts (editing is out of scope).

A. Schedules are part of the Contract
B. Separate Schedule component
X. Other (please specify)

[Answer]: A
