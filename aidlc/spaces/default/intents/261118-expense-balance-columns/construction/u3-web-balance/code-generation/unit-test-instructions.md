# Unit Test Instructions: u3-web-balance

## Framework

Vitest with Testing Library, vitest-axe and jsdom, configured in `web/` (`web/src/test/setup.ts`). No new packages and no configuration change. Tests use the fake API in `web/src/test/fakeApi.ts`; no running API or database is needed.

## Run this Unit's tests

From the project folder:

```powershell
npm.cmd --prefix web exec -- vitest --run src/test/display.test.ts src/test/expenseBalance.test.tsx src/test/contractDetail.test.tsx src/test/schedulePreview.test.tsx src/test/schedule.perf.test.tsx src/test/accessibility.test.tsx
```

These are the new U3 test files plus the existing files this Unit updates. Lint, typecheck, build and size run in the verification script.

## Expected coverage targets

No coverage floor (team.md). Every acceptance criterion U3 covers and every NFR row NFR1.1 to NFR4.3 and SEC2 has at least one test. Build and Test runs the full suite.

## Mocking and stubbing

The fake API (`createFakeApi`-style helpers in `web/src/test/fakeApi.ts`) supplies contract detail and preview responses, now with `balanceRows`. Formatter tests call the functions directly.

## Test data

Inline in each test or from the fake API: the C1 example (12,000.00 paid in January 2027, 1,000.00 a month expense), an accrual example with negative values, and a 120-row list for the timing test.

## Checks not automated

Real-browser layout (sticky Period column, sticky header, sideways and keyboard scrolling, popup width) is checked by the person by eye at the end of U3 (team.md).
