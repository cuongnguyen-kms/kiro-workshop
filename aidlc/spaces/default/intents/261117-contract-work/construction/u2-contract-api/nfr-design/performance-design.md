# Performance Design: u2-contract-api

No questions were needed: the requirements fix the targets and the stack, so the patterns follow from them.

| Pattern | Design | Serves |
|---|---|---|
| Pooled connections | Npgsql connection pool, maximum 20, default timeouts. No extra pooling layer. | NFR5.1, NFR5.7 |
| Batched writes | The whole aggregate is added to one EF Core context and saved with one `SaveChanges` call, which sends the inserts in batches inside the single transaction. Rows are never saved one call at a time. | NFR5.1, NFR5.2 |
| Read in few queries | Get contract: one query for the contract with its streams, one for payment rows, one for expense rows (`AsSplitQuery`), read-only without change tracking. | NFR5.3 |
| Paged list | Count query plus one page query that selects only id, name and dates (no streams or schedules). Index on `(start_date desc, name, id)` serves the sort. | NFR5.4 |
| Constant-time size check | BR5.4 estimates rows per stream with date arithmetic (days divided by 7, or month differences divided by 1, 3 or 12) so the cost does not grow with the range. | NFR5.5, NFR5.2 |
| No caching | Contracts never change after save and reads are within budget, so no cache and no cache invalidation. | NFR5.3, NFR5.4 |
| Asynchronous I/O | All database calls use async methods so requests do not block threads while waiting. | NFR5.7 |

## Performance budget for create (2 seconds, NFR5.1)

| Step | Budget |
|---|---|
| Parse and validate | 50 ms |
| Size check and schedule generation (U1, in memory) | 100 ms |
| Save in one transaction | 800 ms |
| Response serialization | 100 ms |
| Headroom | 950 ms |

The 10 streams over 10 years case produces well under 1,300 rows, so it sits far inside the 10,000 row cap used for NFR5.2. Build and Test measures both cases.
