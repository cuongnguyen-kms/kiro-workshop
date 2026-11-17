# Code Generation Plan: u3-contract-web

## Summary

- Builds: the React and TypeScript frontend with shadcn/ui: contracts list with pager, New contract form, contract detail with schedule tabs
- Touches: a new `web/` folder (Vite app, components, API client, tests) and a frontend section added to `scripts/verify.ps1`
- Tests: about 23 Vitest and React Testing Library component tests against a fake API, including axe accessibility checks; test-after

## Context

- Unit: U3 `u3-contract-web` (ui). Stories: US1.1, US2.1, US3.1, US6.1, US6.2, US6.3. Design: `functional-design/` (functional-spec.md, frontend-components.md), `nfr-requirements/`, `nfr-design/`, `infrastructure-design/`, contract C1 in `contract-summary.md`, and the Refined Mockups.
- Methodology: test-after. The testable layer here is frontend behavior: implement the screens, then write and run their tests.
- The frontend is built and tested on its own with a fake API. It does not need u2-contract-api to exist.
- Prerequisite: Node.js current LTS with npm. Step 1 checks it before anything is written.

## Steps

- [x] **Step 1: Check the build tools.** Run `node --version` and `npm --version`. Node must be version 20 or later. If either is missing, stop and tell the person; nothing is written. Traces to NFR1.7.
- [ ] **Step 2: Project structure and production configuration skeleton.**
  - Create the Vite React TypeScript app in `web/` with `npm create vite@latest web -- --template react-ts`, then `npm install` in `web/` (NFR1.3, NFR1.8).
  - Add Tailwind CSS and initialise shadcn/ui with `npx shadcn@latest init`, then add the components from `design-system-mapping.md` with `npx shadcn@latest add button input label select radio-group popover calendar form card table tabs alert alert-dialog skeleton` (NFR1.4).
  - Install `react-router-dom`, `react-hook-form`, `zod` and `@hookform/resolvers` (NFR1.5, NFR1.6).
  - Turn on TypeScript strict mode. Add ESLint rules `react/no-danger` and `no-restricted-globals` and `no-restricted-properties` for `localStorage`, `sessionStorage`, `indexedDB` and `document.cookie` (NFR3.5, NFR3.6, NFR7.5).
  - Add `web/.env.example` with `VITE_API_BASE_URL=` and add `web/.env` and `web/.env.local` to `.gitignore` (NFR3.7).
- [ ] **Step 3: Bootstrap the test runner and record the exact unit-scoped command.** Install `vitest`, `jsdom`, `@testing-library/react`, `@testing-library/user-event`, `@testing-library/jest-dom` and `vitest-axe` as dev dependencies. Add the `test` section to `web/vite.config.ts` and the setup file `web/src/test/setup.ts`. Add the scripts `test`, `typecheck`, `lint` and `size` to `web/package.json`. Add one smoke test that renders the App, and run `npm test -- --run` in `web/` (NFR7.3).
- [ ] **Step 4: Frontend behavior: API types and client.** Create `web/src/api/types.ts` with the C1 types (Frequency, ExpenseMethod, CreateContractRequest, PaymentStreamInput, ExpenseInput, ContractDetail, PaymentEntry, ExpenseEntry, ContractSummary, ContractPage, ValidationProblem) and `web/src/api/contracts.ts` with `createContract`, `listContracts` and `getContract`. The client sends no credentials, reads `VITE_API_BASE_URL`, supports cancelling, and returns typed outcomes for validation, not found and failure (NFR3.4, NFR3.7, NFR5.4).
- [ ] **Step 5: Frontend behavior: shared pieces.** The validation schema and message constants (`web/src/contracts/validation.ts`), with the six messages from the functional spec; display helpers for money (2 decimals) and dates (`YYYY-MM-DD`) that never calculate (NFR4.4, NFR6.3, NFR6.4); DateField, MoneyField, ErrorSummary and ScheduleTable components.
- [ ] **Step 6: Frontend behavior: New contract screen (S2).** ContractForm with StreamCard (add, remove, One-time behavior) and ExpenseSection (method-specific dates), NewContractPage with the save flow W1, the API 400 mapping, the failure alert and the discard dialog. Traces to US1.1, US2.1, US3.1, US6.1 and NFR5.2.
- [ ] **Step 7: Frontend behavior: Contract detail screen (S3).** ContractDetailPage with StreamsSummary and ScheduleTabs (payment and expense tables with totals, the None message), the "Contract saved." banner, not found and error states. Traces to US6.1, US6.2 and NFR5.3.
- [ ] **Step 8: Frontend behavior: Contracts list screen (S1) and routing.** ContractsListPage with ContractsTable and Pager (page and page size kept in the address, sizes 10, 20, 50, 100, default 10, past-the-end moves to the last page), the empty and error states, and the router with lazy-loaded screens and a not-found page. Traces to US6.3 and NFR5.5.
- [ ] **Step 9: Frontend behavior: write and run the tests (test-after).** Write `web/src/test/fakeApi.ts` and the component tests listed in `unit-test-instructions.md`, then run `npm test -- --run` in `web/`. Every test must pass, including the axe checks (NFR7.4) and the 5,200-row render under 1 second (NFR5.3). Targets are not loosened; a failure is reported.
- [ ] **Step 10: Environment and build configuration.** Add the size check script that fails when the list screen's built JavaScript is over 300 KB compressed (NFR5.5). Add a frontend section to `scripts/verify.ps1` that runs, in `web/`: `npm ci`, `npm audit --omit=dev --audit-level=high`, `npm run lint`, `npm run typecheck`, `npm test -- --run`, `npm run build` and `npm run size`, stopping at the first failure. If npm is not found, the section fails with a message that Node.js is needed (NFR3.8, NFR7.5).
- [ ] **Step 11: Documentation and traceability.** Add `web/README.md` with how to install, run (`npm run dev`), test and configure `VITE_API_BASE_URL`. Write `code-summary.md`, `source-manifest.json` and `traceability.json` for this unit.

## Test Steps Included

The plan includes test files (Steps 3, 9) and test configuration (Step 3: Vitest in `vite.config.ts`, the setup file, and the test script), as the test strategy requires.

## Assumptions

- Package choices that the requester did not make: Vite, React Router, React Hook Form, Zod, Vitest, React Testing Library, vitest-axe, ESLint and npm (see `nfr-requirements/tech-stack-decisions.md`). They are the common defaults for a React TypeScript app with shadcn/ui.
- The frontend lives in `web/`, apart from the .NET folders.
- Scaffolding commands (`npm create vite`, `npx shadcn`) write files of their own, such as `web/components.json`, `web/src/components/ui/*` and `web/src/lib/utils.ts`. These are kept and listed in the source manifest.

## Story to Step Map

| Story | Steps |
|---|---|
| US1.1 | 5, 6, 9 |
| US2.1 | 5, 6, 9 |
| US3.1 | 5, 6, 9 |
| US6.1 | 4, 6, 7, 9 |
| US6.2 | 4, 7, 9 |
| US6.3 | 4, 8, 9 |
| NFR3.5, NFR3.6, NFR7.5 | 2, 10 |
| NFR5.5, NFR3.8 | 10 |
| NFR7.4, NFR5.3 | 9 |

## Testing Contract

```json
{
  "version": 1,
  "methodology": "test-after",
  "source": "org",
  "ordering": "implement each applicable testable layer, then write and run that layer's tests.",
  "scope": "classic",
  "test_strategy": "minimal",
  "project_type": "greenfield",
  "applicable_notes": [
    {
      "layer": "org",
      "text": "We treat tests as a first-class deliverable in every Bolt. The specific\nmethodology (TDD, BDD, ATDD, or classic test-after) is affirmed at\npractices-discovery and recorded in `team.md` under this heading with explicit\n`Methodology` and `Ordering` fields; Code Generation resolves those fields\nindependently from coverage, tooling, and scope notes.\n\nWhen no posture has been affirmed, our default per scope is:\n- **Methodology**: test-after\n- **Ordering**: implement each applicable testable layer, then write and run\n  that layer's tests.\n- `mvp`, `enterprise`, `feature`, `infra` add an 80% line-coverage\n  floor and CI execution before merge.\n- `bugfix`, `security-patch` add a targeted regression for the specific\n  bug/vulnerability and require the existing suite to remain green.\n- `express` uses the Minimal strategy: requirement-driven unit tests (one per\n  requirement, with a happy-path floor per component); existing tests remain\n  green.\n- `poc`, `refactor`, `workshop`, `classic` add no extra new-test floor and require the\n  existing suite to remain green.\n\nThe active `Test Strategy` still applies in every scope and determines test\nvolume/types. Scope floors are additive; they never reduce or replace the\nselected strategy.\n\nBuild and Test verifies defined coverage floors and affirmed quality targets;\nthey may not be weakened to make a step pass.\n\nAffirm a stricter posture in `team.md` if the team commits to one."
    }
  ],
  "obligations": {
    "strategy": "minimal",
    "strategy_volume": [
      "One verifiable test per requirement at the narrowest effective level.",
      "At least one happy-path unit test per component.",
      "Unit tests are the default; a bugfix/security scope floor may require an integration or E2E regression when that is the narrowest level that reproduces the defect."
    ],
    "scope_floor": [
      "Keep the existing test suite green.",
      "This scope adds no extra new-test floor beyond the selected test strategy."
    ],
    "combination_rule": "Apply every selected-strategy obligation and every scope-floor obligation; neither replaces the other, and a targeted scope regression may add the narrowest necessary test type beyond the strategy default."
  },
  "plan_profile": {
    "methodology": "test-after",
    "runner_step": "Bootstrap the minimal test runner/configuration and record the exact unit-scoped command.",
    "runner_ready_before_first_test": true,
    "testable_layers": [
      "Data model / database behavior",
      "Repository / data access",
      "Business logic",
      "API / endpoint",
      "Frontend behavior"
    ],
    "steps": [
      "Project structure and production configuration skeleton.",
      "Bootstrap the minimal test runner/configuration and record the exact unit-scoped command.",
      "Data model / database behavior - implement.",
      "Data model / database behavior - write and run its tests after implementation.",
      "Repository / data access - implement.",
      "Repository / data access - write and run its tests after implementation.",
      "Business logic - implement.",
      "Business logic - write and run its tests after implementation.",
      "API / endpoint - implement.",
      "API / endpoint - write and run its tests after implementation.",
      "Frontend behavior - implement.",
      "Frontend behavior - write and run its tests after implementation.",
      "Environment/build configuration.",
      "Documentation and traceability."
    ]
  },
  "input_sha256": "sha256:39866c06a848b589e1cecab794bb7df06d0190eb9b72703333ba72e6d8a39031",
  "contract_sha256": "sha256:818cff968cd44820f3eb2e38374d17d8c73f417d0164e1bcc21bcef03e530848"
}
```
