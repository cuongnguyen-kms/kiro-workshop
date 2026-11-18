# Scalability Requirements: u2-contract-api

| ID | Requirement | Target | Source |
|---|---|---|---|
| NFR5.6 | Data growth | Works with up to 10,000 saved contracts, up to 10,000 schedule rows each in the worst case; in normal use far fewer | NFR5, BR5.4 |
| NFR5.7 | Concurrency | At least 10 simultaneous create requests succeed without errors or data mixed between contracts; each save is its own transaction | FR6.1 |
| NFR5.8 | Scaling approach | One process, vertical only. The API keeps no state between requests, so more instances can run behind a load balancer later without code changes | NFR3 |
| NFR5.9 | Indexes | The contracts table is indexed for the list order (start date, name, id); child tables are indexed by contract id | FR6.3 |

No scaling triggers or capacity plan are needed for this version: a single development-scale deployment, no autoscaling.
