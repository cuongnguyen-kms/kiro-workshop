# Security Design: u1-schedule-engine

The library has no network, file or database access and no users, so the design is small: keep it free of side effects, check its input, and bound its work.

## Design Decisions

| Concern | Design | Covers |
|---|---|---|
| No authentication or authorization | The public interface takes only the request records. No user, token or identity parameter, and no policy code. The backend (U2) decides who may call. | NFR3.1 |
| No side effects | The project references no I/O packages and the code does not use file, network, environment, database, console or clock APIs. A build check (a code analyzer rule banning those namespaces and `DateTime.Now`, `DateTime.UtcNow`, `DateOnly.FromDateTime(DateTime.Now)`) enforces it. | NFR3.2 |
| Input checking | The first step of `Generate` checks every promise in BR1.8 and throws `ArgumentException` naming the broken promise, before any work. Nothing is returned for bad input. | NFR6.1 |
| Bounded work | Before building payment entries, the engine counts how many entries the streams would produce, using the date arithmetic without storing them. If the total is over 100,000 it throws `ArgumentException`. | NFR6.2 |
| No third-party packages | The library project has no package references. The test project holds the only packages (xUnit and its runner). | NFR1.2 |
| No secrets | Nothing is configured, so nothing secret exists to leak or log. Error messages name rules, never echo values beyond ids and dates the caller sent. | NFR3.2 |

## Bounded Work: How the Count Works

The count for a stream is how many payment dates fall between its start and end dates. For Weekly it is the number of days divided by 7, rounded down, plus 1. For the month-based frequencies it is the number of whole steps that fit, plus 1. For OneTime it is 1. The engine adds the counts of all streams and compares to the limit. Counting costs almost nothing compared with building the entries.

## Failure Behavior

| Failure | Result |
|---|---|
| Broken input promise | `ArgumentException` with a message naming the promise, for example "Stream 2: endDate is before startDate". |
| Too many entries | `ArgumentException`: "The request would produce more than 100000 payment entries." |
| Programming error | An unexpected exception passes up unchanged. The library does not catch and hide it. |
