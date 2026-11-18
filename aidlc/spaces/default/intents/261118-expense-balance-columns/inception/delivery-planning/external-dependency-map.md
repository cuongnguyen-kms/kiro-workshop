# External Dependency Map: expense balance columns

A Bolt is one build pass over a piece of the work, ending in something that runs.

The work is fully contained in this repository and machine. Nothing outside the team gates any Bolt.

| Item | Owner | Lead time | Blocks | If it slips |
|---|---|---|---|---|
| Local PostgreSQL with the password in user secrets | developer machine (already set up) | none | Bolt 2 integration tests | Bolt 2 stops at its integration tests until the database is back |
| `npm audit` picomatch advisory (accepted, team.md) | upstream package maintainers | unknown | none: accepted | `scripts/verify.ps1` keeps stopping at the audit step; every other step is run and reported |
