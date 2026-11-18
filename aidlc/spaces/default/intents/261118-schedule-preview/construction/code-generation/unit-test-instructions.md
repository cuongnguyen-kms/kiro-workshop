# Unit Test Instructions: schedule preview

## Framework

Existing runners only, nothing new installed:

- Backend: xUnit 2.9.3 in `tests/ContractManagement.Api.Tests` (fakes, no database) and `tests/ContractManagement.Api.IntegrationTests` (real local PostgreSQL `contract_management_test`, password from the user secret `ConnectionStrings:ContractDbPassword`, already set on this machine).
- Frontend: Vitest with React Testing Library, jsdom and vitest-axe, configured in `web/vite.config.ts`. The API module is replaced by `web/src/test/fakeApi.ts`.

## Commands scoped to this change

Run from the workspace root:

```powershell
dotnet build tests/ContractManagement.Api.Tests -warnaserror
dotnet build tests/ContractManagement.Api.IntegrationTests -warnaserror
dotnet test tests/ContractManagement.Api.Tests --no-build --filter "FullyQualifiedName~Preview"
dotnet test tests/ContractManagement.Api.IntegrationTests --no-build --filter "FullyQualifiedName~Preview"
```

Run in `web/`:

```powershell
npm test -- --run src/test/schedulePreview.test.tsx
```

The new backend test classes have `Preview` in their names (`ContractPreviewServiceTests`, `PreviewApiTests`) so the filters select only them. The frontend tests for this change live only in `web/src/test/schedulePreview.test.tsx`.

## Tests (Minimal strategy: one per requirement, about 15)

| Test | Layer | Requirement |
|---|---|---|
| Preview of a valid contract returns both schedules equal to what Create returns for the same input | service (real generator, fake repository) | FR2.1, NFR2 |
| Preview calls the repository zero times | service | FR2.2 |
| Payment preview ignores invalid expense fields | service | FR2.3 |
| Preview with a blank name succeeds | service | FR2.4 |
| Invalid stream gives the same message and field path as Create | service | FR2.5 |
| Over 10,000 rows is rejected with the Create message | service | FR2.7 |
| POST preview returns 200 with both schedules and stores nothing (contract count unchanged) | API integration | FR2.1, FR2.2 |
| POST preview with invalid input returns 400 ValidationProblem | API integration | FR2.5, NFR3 |
| Preview of 10 streams over 10 years responds within 2 s | API integration | NFR1 |
| Payment button disabled until contract dates and streams are valid, then enabled; name not needed | frontend | FR1.2, FR2.4, FR1.6 |
| Expense button needs a valid method and dates; with None it stays disabled with the hint | frontend | FR1.3, FR1.4, FR1.5 |
| Payment popup shows loading, then rows labelled Stream 1, Stream 2 and the total | frontend | FR3.1, FR3.3, FR3.4 |
| Expense popup shows periods and total | frontend | FR3.2 |
| Server 400 shows the messages and no table; a failure shows the retry message and Retry calls again | frontend | FR2.5, FR2.6 |
| Escape closes the popup, focus returns to the button, form values unchanged; reopening after a change sends the new values | frontend | FR3.5, FR3.6, FR3.7 |
| axe finds no serious or critical issues with the popup open | frontend | NFR4 |

## Coverage

No line-coverage floor applies to the express scope. Every requirement above has at least one test. The existing suites (`scripts/verify.ps1`) must stay green.

## Mocking

- Service tests use the real `ScheduleGenerator` and the existing fake repository from `tests/ContractManagement.Api.Tests/Fakes.cs`.
- Frontend tests add `previewSchedule` to the fake API in `web/src/test/fakeApi.ts`.

## Test data

Fixed dates (2027), the story examples (Monthly 1000, All at once 2027-03-01). No test reads the clock.
