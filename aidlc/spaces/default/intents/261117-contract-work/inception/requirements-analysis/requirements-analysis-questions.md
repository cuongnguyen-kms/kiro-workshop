# Requirements Analysis Questions

Context: a contract management system (.NET 10 backend, React frontend). A contract holds a name, start/end dates, one or more payment streams and an expense method. Saving a contract generates a payment schedule and an expense schedule, stored with the contract.

## Q1. [Product owner] What does the expense schedule expense?

The expense schedule needs an amount to spread over time (Overtime) or to book on one date (All at once). Your description does not say where that amount comes from.

A. The total of all payments in the payment schedule
B. A separate expense amount entered on the contract
C. Each payment is expensed on its own (one expense line per payment)
X. Other (please specify)

[Answer]: A

## Q2. [Product owner] How is an Overtime expense split across its date range?

Overtime means the amount is spread between a start date and an end date you select. The split rule decides the numbers in every generated row.

A. Equal amount each month (the last month absorbs any rounding difference)
B. Equal amount each day, grouped into monthly rows
C. Equal amount each period of the payment stream's frequency
X. Other (please specify)

[Answer]: B

## Q3. [Product owner] What frequencies can a payment stream use, and can a contract have more than one stream?

The frequency drives how the payment schedule dates are generated.

A. Weekly, Monthly, Quarterly, Annually, and One-time; a contract can have several streams
B. Monthly, Quarterly, Annually; a contract can have several streams
C. Weekly, Monthly, Quarterly, Annually, and One-time; a contract has exactly one stream
X. Other (please specify)

[Answer]: A

## Q4. [Developer] Which database and sign-in approach should the system use?

The backend needs somewhere to store contracts and their schedules, and the API needs to know who may call it.

A. SQL Server, with no sign-in for now
B. PostgreSQL, with no sign-in for now
C. SQL Server, with sign-in (Microsoft Entra ID / OAuth)
D. PostgreSQL, with sign-in (OAuth / OpenID Connect)
X. Other (please specify)

[Answer]: B
