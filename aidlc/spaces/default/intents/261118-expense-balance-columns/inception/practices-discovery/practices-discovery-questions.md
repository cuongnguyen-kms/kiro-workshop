# Practices Discovery Questions

These questions settle how the team works on this project. Each one offers what the repo already suggests.

## Question 1
How does work reach the shared code? The repo has one branch, `development`, with commits made straight to it.

A. Commit straight to `development`; it is the branch all work merges into
B. Short-lived feature branches merged into `development` after a review
C. Short-lived feature branches merged into `main` (a `main` branch would be created)
X. Other (please specify)

[Answer]: A

## Question 2
Build a thin end-to-end slice first? A walking skeleton is a minimal version that runs the whole way through, built first to prove the pieces connect before the real features go in.

A. No: the app already runs end to end, so changes are built directly
B. Yes, for every new feature
C. Only for brand-new projects
X. Other (please specify)

[Answer]: A

## Question 3
When are tests written? So far each layer was built first and its tests written straight after.

A. Test-after: build each layer, then write and run its tests
B. Test-first (TDD): write a failing test, make it pass, then tidy up
C. Behaviour-first (BDD): write the scenarios first, then build the feature to pass them
X. Other (please specify)

[Answer]: A

## Question 4
Should there be a minimum test coverage? The project measures none today.

A. No coverage minimum; every requirement must have a test, and all tests must pass
B. 80% line coverage, measured on the whole solution
X. Other (please specify)

[Answer]: A

## Question 5
How is a change released? There is no CI pipeline or hosting target today.

A. Run `scripts/verify.ps1` locally; no deployment for now
B. Add a CI pipeline that runs `scripts/verify.ps1` on every push
X. Other (please specify)

[Answer]: A

## Question 6
Code style beyond the existing ESLint and warnings-as-errors settings?

A. Nothing more: ESLint for the frontend, warnings as errors for .NET, follow the existing code's patterns
B. Also add formatters (Prettier for the frontend, `dotnet format` for .NET)
X. Other (please specify)

[Answer]: A
