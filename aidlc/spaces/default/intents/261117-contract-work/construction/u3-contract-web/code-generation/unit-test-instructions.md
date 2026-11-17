# Unit Test Instructions: u3-contract-web

## Setup

- Framework: Vitest with React Testing Library and `@testing-library/user-event`, in a jsdom environment, configured in `web/vite.config.ts` (the `test` section) with a setup file `web/src/test/setup.ts`.
- Accessibility: `vitest-axe` runs axe on rendered screens.
- Prerequisite: Node.js current LTS and npm. No backend, database or network is needed.
- Mocking: tests replace the API client module (`web/src/api/contracts.ts`) with a fake built from contract C1 (`web/src/test/fakeApi.ts`). The fake returns the response shapes from C1 (201 with `ContractDetail`, 400 with a Problem Details `errors` map, 404, a failure, and a `ContractPage`). Components are never tested against a real server.
- Test data: written in the tests, using the examples from the stories (for example a Monthly stream of 1000 and an AllAtOnce expense on 2027-03-01).

## Command (this unit only)

Run in the `web/` folder:

```
npm test -- --run
```

`npm test` runs Vitest, and `--run` makes it run once instead of watching. It runs only this unit's tests, which live under `web/src/`. This command works from Step 3 of the plan with one smoke test.

## Tests (Minimal strategy: at least one test per acceptance criterion, about 20)

| Test | Checks | Traces to |
|---|---|---|
| Form starts empty with one stream and None selected | Initial state | AC1.1.1 |
| Blank name shows "Contract name is required." | Client validation | AC1.1.2 |
| End before start shows the end date message | Client validation | AC1.1.3 |
| Add stream adds a card and focuses its Amount | Add | AC2.1.1 |
| Remove stream removes that card; Remove disabled on the last one | Remove | AC2.1.2 |
| Zero amount shows "Amount must be greater than zero." | Client validation | AC2.1.3 |
| Stream outside the contract shows the stream dates message | Client validation | AC2.1.4 |
| One-time hides the end date and sends start date as end date | Frequency | AC2.1.5 |
| Frequency list offers exactly Weekly, Monthly, Quarterly, Annually, One-time | Options | AC2.1.6 |
| Overtime shows two dates; All at once one date; None no dates | Method fields | AC3.1.1, AC3.1.2, AC3.1.3 |
| Expense date outside the contract shows the expense dates message | Client validation | AC3.1.4 |
| Overtime end before start shows its message | Client validation | AC3.1.5 |
| Valid save sends one POST, shows saving, then opens the detail with "Contract saved." | Save | AC6.1.1, NFR5.2, NFR5.4 |
| Save failure keeps entries, shows the not-saved alert, and does not resend | Failure | AC6.1.2, NFR5.4 |
| API 400 errors appear under the matching fields and in the summary | Server validation | NFR6.3 |
| Detail shows name, dates, streams, method, and both schedules with totals | Detail | AC6.2.1 |
| Unknown contract shows "Contract not found." | Not found | AC6.2.2 |
| None method shows "This contract has no expense schedule." | Detail | AC5.1.5 on screen |
| List shows name, start and end; page-size select offers 10, 20, 50, 100 with default 10 | List and pager | AC6.3.1 |
| Empty list shows "No contracts yet." | Empty | AC6.3.2 |
| A contract name with `<script>` text is shown as plain text | Safe text | NFR3.5 |
| 5,200-row payment schedule renders in under 1 second | Performance | NFR5.3 |
| axe finds no serious or critical issues on S1, S2 and S3 | Accessibility | NFR7.4 |

## Expected Coverage

- Every acceptance criterion this unit owns has at least one test (see `functional-design/traceability.json`).
- No line-coverage floor applies to the classic plan.

## Results

A run is good when every test passes. The performance and accessibility tests must pass as written; a failure is reported, and the targets are not loosened.
