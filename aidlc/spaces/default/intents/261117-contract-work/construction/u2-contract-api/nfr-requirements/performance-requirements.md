# Performance Requirements: u2-contract-api

No questions were needed: the requirements already give the target, and the other figures follow from the Functional Design size cap (BR5.4).

| ID | Requirement | Target | Source |
|---|---|---|---|
| NFR5.1 | Create a contract with up to 10 payment streams spanning 10 years (validation, schedule generation, one transaction) | Responds within 2 seconds, measured on a typical development machine with local PostgreSQL, 95th percentile over 20 runs | NFR5 |
| NFR5.2 | Create a contract at the size cap (10,000 schedule rows, BR5.4) | Completes without error within 5 seconds; rows are written in batches, not one statement per row | NFR5, BR5.4 |
| NFR5.3 | Read one contract with its streams and both schedules | Within 500 ms for a contract at the size cap, using a bounded number of queries (no query per row) | FR6.2 |
| NFR5.4 | List one page of contracts | Within 300 ms with 10,000 saved contracts; the sort columns are indexed | FR6.3, BR5.3 |
| NFR5.5 | Size check before generation (BR5.4) | Costs a fixed number of operations per stream, independent of the date range | BR5.4 |

## Resource constraints

- One API process on a development machine. No caching: data never changes after save, and reads are fast enough without it.
- Database connections come from a pool with a default maximum of 20.
- The 2-second target is verified by a test in Build and Test (the 10 streams over 10 years case), not by load testing.
