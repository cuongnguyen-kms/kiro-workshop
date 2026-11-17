# User Stories Assessment

## Decision

Execute.

## Rationale

The system is user-facing (a React entry form and schedule views) and has real business logic (schedule generation from payment streams and expense methods). Stories give each behavior a testable acceptance criterion before design starts.

## Factors Considered

- Project type: new (greenfield) web application, .NET 10 backend and React frontend.
- User-facing scope: contract entry form, schedule views, saved contracts list.
- Complexity signals: three expense methods, five payment frequencies, month-end date handling, rounding rules.
- Personas: one (contract manager), per the answer to Q1.

## Where Stories Add the Most Value

- Payment schedule date rules (FR4), including the month-end case.
- Overtime expense splitting and rounding (FR5).
- All-or-nothing save of a contract with its schedules (FR6.1).
