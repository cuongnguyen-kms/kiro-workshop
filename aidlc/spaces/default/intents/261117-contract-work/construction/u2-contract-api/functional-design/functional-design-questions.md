# Functional Design Questions: u2-contract-api

Context: the requirements, units, components and the C1/C2 contracts already settle most of this Unit. Only the points below need your judgement.

## Question 1
Can two contracts have the same name?

A. Yes, names are labels and duplicates are allowed
B. No, reject a name already used by another contract (ignoring upper/lower case), with a clear message
X. Other (please specify)

[Answer]: A

## Question 2
How many payment streams, and how many schedule rows, may one contract have? (NFR5 only promises speed for up to 10 streams over 10 years.)

A. No cap on streams; reject a contract whose schedules would exceed 10,000 rows in total
B. At most 20 streams and 10,000 schedule rows in total, both rejected with a clear message
C. At most 10 streams and 10,000 schedule rows in total, both rejected with a clear message
X. Other (please specify)

[Answer]: A

## Question 3
Local PostgreSQL: you said it is installed with password `postgres`. The password will be read from configuration (user secrets or an environment variable), never written into source or design files. Which local database should the API use?

A. Host `localhost`, port `5432`, user `postgres`, database `contract_management` (created by the app setup if missing)
B. Same host, port and user, but a database name I will give you
X. Other (please specify)

[Answer]: A
