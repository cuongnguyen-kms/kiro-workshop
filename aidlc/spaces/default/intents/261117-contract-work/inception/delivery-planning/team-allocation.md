# Team Allocation

A Bolt is one build pass over one or more units that ends in something that runs and is tested. A mob is the group that builds a Bolt together. The classic plan skips Team Formation, so every Bolt is planned for the AI developer agent (aidlc-developer-agent), with you approving each code plan and each finished unit.

| Bolt | Units | Mob | Human role |
|---|---|---|---|
| Bolt 1: Backend with schedule rules | U1 u1-schedule-engine, U2 u2-contract-api | aidlc-developer-agent | Approve each code plan, provide the local PostgreSQL connection string, approve each finished unit |
| Bolt 2: Frontend | U3 u3-contract-web | aidlc-developer-agent | Approve the code plan, approve the finished unit |

There is one team, so no Program Board (a cross-team view of which team builds what, when) is needed.
