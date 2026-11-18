# Domain Design Questions: expense balance columns

The change adds no new screen or entity. The one boundary decision is where the new balance rules (FR1 rows, FR2 columns) live. Today the schedule rules live in the ScheduleGenerator library (pure, no database), and ContractService stores and reads the rows.

## Question 1
Where should the new balance rows and columns be worked out?

A. In the ScheduleGenerator library: it already builds both schedules, so it also builds the combined monthly balance rows. ContractService passes them through for previews and rebuilds them from the stored payment and expense rows when a saved contract is read. All rules stay in one pure, separately tested place (recommended)
B. In ContractService in the API: it combines the payment and expense rows it already has. The library stays unchanged, but the rules move into the API and need the API's test setup
C. In a new separate library next to ScheduleGenerator, used only for the balance rows
X. Other (please specify)

[Answer]: A
