# Tech Stack Decisions: u1-schedule-engine

| ID | Decision | Rationale | Inception NFR |
|---|---|---|---|
| NFR1.1 | Class library in C# targeting .NET 10 (`net10.0`), in the same solution as the backend (U2). | The requester set .NET 10 for the backend, and U2 calls this library in-process (contract C2). | NFR1 |
| NFR1.2 | No third-party packages in the library project. | Nothing here needs one: dates, decimals and lists are in the base library. Fewer dependencies mean a smaller security surface and nothing to keep up to date. | NFR1, NFR3 |
| NFR4.1 | All money uses `decimal`, never `double` or `float`. Dates use `DateOnly`. | Exact base-10 arithmetic gives exact totals (BR2.6, BR2.4). `DateOnly` has no time or time zone, matching the requirement that dates are calendar dates. | NFR4 |
| NFR4.2 | Rounding uses `MidpointRounding.AwayFromZero` to 2 decimals. | Matches BR2.6. | NFR4 |
| NFR4.3 | The public surface is the `IScheduleGenerator` interface and the records from contract C2, with immutable records and read-only lists. | Immutable inputs and outputs make the same-input-same-output rule (BR1.7) easy to keep and to test. | NFR4 |
| NFR5.1 | Performance target: for 10 payment streams over 10 years at the weekly frequency (about 5,200 payment entries) plus an Overtime expense over the same 10 years, `Generate` completes in under 200 ms on a typical development machine. | The whole save is budgeted at 2 seconds (NFR5); the library takes a small share so storage has the rest. | NFR5 |
| NFR7.1 | The project is separate from the web project and references no web, database or UI packages. | Lets the schedule rules be tested alone, as ADR-001 intends. | NFR7 |
| NFR7.2 | Tests use xUnit in a separate test project, with plain assertions and no mocks. | xUnit is the common default for .NET. The requester has not chosen a test framework; this is the default pick and is easy to change. The library has no dependencies, so nothing needs mocking. | NFR7 |
| NFR2.1 | This unit stores nothing. | PostgreSQL storage belongs to U2. | NFR2 |
