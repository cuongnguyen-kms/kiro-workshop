# Code Quality Assessment

## Tests
- .NET: Scheduling.Tests 19 facts; Api.Tests 35; Api.IntegrationTests 24 (need a local PostgreSQL test database). Counts are from the prior scan and were not re-counted; preview tests were not opened this run.
- Web: Vitest files for new contract, contract detail, list, accessibility (axe), a 5,200-row render timing test, and a smoke test, all against `fakeApi.ts`. These assert current schedule table output (two-column expense rows, `toFixed` strings) and will need updating when columns or number format change.
- No coverage tooling configured.

## Linting and Build Rules
- .NET warnings are errors; Scheduling forbids I/O through a banned-symbols analyzer.
- ESLint (`--max-warnings 0`) bans `dangerouslySetInnerHTML`, `innerHTML`, and browser storage; TypeScript runs strict.
- `npm run size` limits the list screen to 300 KB gzip.

## CI/CD
None in the repository. `scripts/verify.ps1` is the only automated check chain and must be run by hand.

## Documentation
Public C# types have XML summaries. `web/README.md` exists; there is no root README.

## Quality Ratings
Naming clarity good; function size good (small modules); error handling good (typed results, Problem Details); test presence good; duplication fair (validation rules intentionally exist in server and client; schedule row mapping and column sets are repeated in `ScheduleTabs.tsx` and `SchedulePreview.tsx`); dead code none seen.

## Technical Debt
- Client and server validation can drift (`validation.ts` versus `ContractValidator.cs`).
- Expense row mapping is inline in both `ScheduleTabs.tsx` and `SchedulePreview.tsx` (not shared), while `paymentRows` is exported from `ScheduleTabs.tsx` only; preview payment rows use a different mapping.
- `ScheduleTable` supports one right-aligned (last) amount column and one total cell; `ScheduleRow` is all strings, so number formatting is only in `formatMoney`.
- `formatMoney` uses `toFixed(2)`, which prints a minus sign for negatives, has no thousands separator and can print `-0.00`.
- `ContractDetail.expense` is typed with the request type `ExpenseInput` (`web/src/api/types.ts`).
- Overtime expense: the last row is `base - allocated`, so rounding could in theory make it small or negative; not guarded.
- No CI pipeline and no coverage measurement.
