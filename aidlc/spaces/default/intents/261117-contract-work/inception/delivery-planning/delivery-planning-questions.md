# Delivery Planning Questions

Context: Construction builds the three units (U1 schedule rules library, U2 .NET backend, U3 React frontend) in Bolts. A Bolt is one build pass over one or more units that ends in something that runs and is tested. U2 needs U1; U3 needs only the API contract. The classic plan does not use a walking skeleton (a first thin end-to-end slice), so the order below is a free choice within those dependencies.

## Q1. [Product owner] What should be built first?

All three orders respect the dependencies; they differ in what you can see working early.

A. Risk first: schedule rules (U1), then the backend (U2), then the frontend (U3). The hardest logic is proven first, and a working API exists before the screens.
B. Rules, then screens, then backend (U1, U3, U2). The screens can be clicked through early against a fake API; real saving comes last.
C. Screens first (U3, U1, U2). You see the UI first; the rules are proven later.
X. Other (please specify)

[Answer]: A

## Q2. [Delivery lead] How big should each Bolt be?

Smaller Bolts give more review points; bigger Bolts give fewer stops.

A. One Bolt per unit: three Bolts, each reviewed when it is done
B. Two Bolts: the backend with its rules (U1 and U2 together), then the frontend (U3)
X. Other (please specify)

[Answer]: B

## Q3. [Developer] How should PostgreSQL be available for development and the backend's tests?

The backend's save and read tests need a real database, and the verification command that checks each finished unit depends on this.

A. Testcontainers: the tests start a throwaway PostgreSQL in Docker automatically (needs Docker installed)
B. A local PostgreSQL install, with the connection string in configuration
C. A docker compose file that starts PostgreSQL, used for both running the app and the tests
X. Other (please specify)

[Answer]: B
