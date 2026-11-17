# Contract Design Questions

Context: there are two contracts to pin down. One is the HTTP API that the React frontend (U3) calls on the .NET backend (U2), which crosses a separate deployment. The other is the in-process library interface that the backend (U2) calls on the schedule engine (U1). These questions cover the HTTP API, which is the one U3 is built against without U2.

## Q1. [Architect] How should the HTTP API be versioned?

Versioning decides how a later change avoids breaking the deployed frontend.

A. Version in the path (`/api/v1/...`); within v1 only add fields, and a breaking change becomes `/api/v2`
B. No version in the path (`/api/...`); frontend and backend are always deployed together at the same version
X. Other (please specify)

[Answer]: A

## Q2. [Developer] What shape should error responses use?

The frontend has to show the backend's validation messages next to the right field (interaction spec).

A. The standard Problem Details format (RFC 9457) that ASP.NET Core produces, with field-level errors in an `errors` map keyed by field path
B. A custom shape: `{ "errors": [ { "field": "...", "message": "..." } ] }`
X. Other (please specify)

[Answer]: A

## Q3. [Product owner] Should the contracts list be paged?

The list screen shows each contract's name and dates (US6.3).

A. No paging in this version: the list returns every contract, newest start date first
B. Page-based paging (`page` and `pageSize`, default 20) with a total count
X. Other (please specify)

[Answer]: B, with page size options 10, 20, 50 and 100, and a default of 10 (the requester's own words: "page size options: 10, 20, 50, 100. default 10")
