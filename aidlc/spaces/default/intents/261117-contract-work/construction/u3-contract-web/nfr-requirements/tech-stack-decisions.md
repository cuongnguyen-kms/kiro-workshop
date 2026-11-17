# Tech Stack Decisions: u3-contract-web

| ID | Decision | Rationale | Inception NFR |
|---|---|---|---|
| NFR1.3 | React 19 with TypeScript, built and served in development by Vite. | The requester set React. TypeScript catches mismatches with the API types, and Vite is the standard fast tooling for a React single-page app. | NFR1 |
| NFR1.4 | Component library: shadcn/ui on Tailwind CSS and Radix UI primitives. | Chosen by the requester in Refined Mockups (Q2). Radix gives keyboard and ARIA behavior the WCAG 2.2 AA target needs. | NFR1, NFR6 |
| NFR1.5 | Forms with React Hook Form and Zod for the validation schema. | Matches the shadcn/ui form components and keeps the validation rules in one schema (functional-spec.md). | NFR1, NFR6 |
| NFR1.6 | Routing with React Router. | Three screens with addresses that can be shared and refreshed, including the list's page and page size. | NFR1 |
| NFR1.7 | Node.js current LTS and npm, with a committed `package-lock.json`. | Needed to build and test the app. The requester has not chosen a package manager; npm ships with Node.js and needs no extra install. | NFR1, NFR3 |
| NFR2.2 | The unit stores nothing in the browser; the backend is the only store. | See NFR3.6. | NFR2 |
| NFR4.4 | The client never adds, splits or rounds money. Amounts and totals are shown exactly as the API returns them, formatted to 2 decimals for display only. | Schedule math belongs to u1, so totals can never disagree between screen and storage. | NFR4 |
| NFR7.3 | Tests use Vitest with React Testing Library and a fake API module that follows contract C1. | Lets this unit build and pass without the backend, as the unit plan requires. | NFR7 |
| NFR7.4 | Accessibility is checked automatically with axe (through `vitest-axe`) on screens S1, S2 and S3 against WCAG 2.2 AA rules, and the check must report no serious or critical issues. | The accessibility checklist target from Refined Mockups (Q3). Automated checks catch part of the list; the rest is in the checklist for review. | NFR7 |
| NFR7.5 | Code quality: ESLint and TypeScript strict mode run in the verification command and must pass with no errors. | The org code style defers to the project's linter. Neither was chosen by the requester; these are the Vite React TypeScript defaults. | NFR7 |
| NFR1.8 | The frontend lives in `web/` at the workspace root, apart from the .NET `src/` and `tests/` folders. | Keeps the two toolchains from sharing folders. | NFR1 |

## Assumptions

- npm, Vitest, ESLint and the package choices above are defaults, not requester choices. They are easy to change before Code Generation for this unit.
- Node.js and npm are not yet installed on this machine (checked in the last step). The unit's build stops at its first step until they are.
