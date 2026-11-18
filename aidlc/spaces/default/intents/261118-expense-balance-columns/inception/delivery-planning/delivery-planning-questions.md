# Delivery Planning Questions: expense balance columns

A Bolt is one build pass over a piece of the work, ending in something that runs. The units are U1 (balance rules in the schedule library), U2 (API returns the balance rows; needs U1) and U3 (screen: 8-column table and bracket format; needs only the API contract). Team practices already settle the rest: no thin end-to-end slice first, one Unit built at a time, nothing outside the team to wait for, release by running `scripts/verify.ps1` locally.

## Question 1
What should be built first?

A. Riskiest first: U1 (the accounting rules and the 12-month split), then U2, then U3. Each Bolt is one Unit
B. Rules first, then the screen: U1, then U3, then U2, so the new table can be seen against the fake API before the real API changes
C. Most visible first: U3 (bracket format and table against the fake API), then U1, then U2
X. Other (please specify)

[Answer]: A
