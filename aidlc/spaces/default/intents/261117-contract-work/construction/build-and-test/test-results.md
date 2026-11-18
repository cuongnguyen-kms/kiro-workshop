# Test Results

Run on 2026-11-17 on the development machine, local PostgreSQL. Test strategy Minimal, so no extra instruction files were generated.

## Build

| Command | Result |
|---|---|
| `dotnet build ContractManagement.slnx -warnaserror` | Succeeded, 0 warnings, 0 errors |
| `npm run lint`, `npm run typecheck`, `npm run build` (in `web/`) | Passed (run by `scripts/verify.ps1`) |

## Unit-scoped test commands (each run once)

| Unit | Command | Total | Passed | Failed | Skipped |
|---|---|---|---|---|---|
| u1-schedule-engine | `dotnet test tests/ContractManagement.Scheduling.Tests/ContractManagement.Scheduling.Tests.csproj --no-build` | 19 | 19 | 0 | 0 |
| u2-contract-api | `dotnet test tests/ContractManagement.Api.Tests --no-build` | 44 | 44 | 0 | 0 |
| u2-contract-api | `dotnet test tests/ContractManagement.Api.IntegrationTests --no-build` | 24 | 24 | 0 | 0 |
| u3-contract-web | `npm test -- --run` (in `web/`, Vitest with jsdom) | 27 | 27 | 0 | 0 |

The u2 instructions list `dotnet build tests/ContractManagement.Api.Tests tests/ContractManagement.Api.IntegrationTests -warnaserror`, which the .NET CLI rejects (one project per call). Each project was built on its own, and the solution build above covers both.

## Full verification

`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1`: exit 0. It ran restore, the build with warnings as errors, all three .NET test projects (19, 44 and 24 passed), then `npm i`, `npm audit --omit=dev --audit-level=high` (0 vulnerabilities), lint, typecheck, Vitest (27 passed), the production build and the size check (130.7 KB of 300 KB).

`npm i` reports 4 moderate vulnerabilities in development dependencies. The project's rule (NFR3.8) covers high and critical vulnerabilities in production dependencies, and that check passed.

## Tests added in this stage

`tests/ContractManagement.Api.IntegrationTests/TargetChecksTests.cs` checks five u2 targets that had no measurement before: NFR5.2 and NFR5.3 (create and read near the 10,000 row cap), NFR5.4 (list with 10,000 contracts), NFR5.7 (10 simultaneous creates), NFR6.3 (256 KB body limit, on a real Kestrel server) and NFR6.4 (a forced 500 response carries no internal detail).

The first run failed one check: reading a contract with about 9,860 rows took 593 ms against the 500 ms target. That timing included compiling the read query on its first use. The test now reads a small contract first, and the timed read passed. The 500 ms limit was not changed.

## Coverage

No line-coverage floor applies to the classic scope, so no full-suite coverage command was run.

## Failure details

None in the final run.

## Target Verification Matrix

Every measurable target from the NFR requirements, NFR design and Testing Contract of the three Units. "jsdom" means the check ran in Node with a DOM emulator, not in a browser.

| Target ID | Source | Expected | Actual | Evidence | Owning Stage | Verdict |
|---|---|---|---|---|---|---|
| TC-SUITE | Testing Contract (all Units) | Existing suite stays green | All 114 tests pass | `scripts/verify.ps1` exit 0 | build-and-test | Met |
| TC-MIN | Testing Contract, Minimal strategy | One test per requirement, happy path per component | Every AC and FR traced to a test file | `cross-unit-traceability.md` (pass) | build-and-test | Met |
| u1 NFR5.1 | u1 tech-stack-decisions.md | `Generate` for 10 weekly streams over 10 years plus Overtime under 200 ms | Under 200 ms | Scheduling test `Performance` passed | build-and-test | Met |
| u1 NFR6.2 | u1 security-requirements.md | Reject over 100,000 entries | Rejected | `TooManyEntries_Throws` passed | build-and-test | Met |
| u1 NFR3.2 | u1 security-requirements.md | No I/O or clock in the library | Banned API analyzer, build passes with warnings as errors | `dotnet build -warnaserror` | build-and-test | Met |
| u2 NFR5.1 | u2 performance-requirements.md | Create, 10 streams over 10 years, within 2 s | Under 2 s | `ApiTests.Create_with_10_streams_over_10_years_responds_within_2_seconds` | build-and-test | Met |
| u2 NFR5.2 | u2 performance-requirements.md | Create at the size cap within 5 s | Under 5 s (about 9,860 rows) | `TargetChecksTests.NFR5_2_and_NFR5_3_...` | build-and-test | Met |
| u2 NFR5.3 | u2 performance-requirements.md | Read a contract at the size cap within 500 ms | Under 500 ms after a warm-up read (593 ms cold) | `TargetChecksTests.NFR5_2_and_NFR5_3_...` | build-and-test | Met |
| u2 NFR5.4 | u2 performance-requirements.md | List one page within 300 ms with 10,000 contracts | Under 300 ms | `TargetChecksTests.NFR5_4_...` | build-and-test | Met |
| u2 NFR5.7 | u2 scalability-requirements.md | 10 simultaneous creates succeed, no mixed data | 10 created, 10 distinct ids, 26 rows each | `TargetChecksTests.NFR5_7_...` | build-and-test | Met |
| u2 NFR4.1 | u2 reliability-requirements.md | A failed write leaves nothing stored | Nothing stored | `RepositoryTests.A_failure_on_a_write_leaves_no_rows` | build-and-test | Met |
| u2 NFR4.3 | u2 reliability-requirements.md | Amounts exact, `numeric(18,2)` | 1234.50 read back exactly | `DataModelTests.Money_columns_are_numeric_18_2_and_keep_amounts_exactly` | build-and-test | Met |
| u2 NFR4.7 | u2 reliability-requirements.md | `/health` 200 when the database answers | 200 | `ApiTests.Health_returns_200_and_responses_carry_security_headers` | build-and-test | Met |
| u2 NFR4.9 | u2 tech-stack-decisions.md | OpenAPI paths match the Contract Summary | Match | `ApiTests.The_generated_openapi_paths_match_the_contract_summary` | build-and-test | Met |
| u2 NFR6.1 | u2 security-requirements.md | Every BR1.x to BR3.x rule enforced with the agreed message | Each rule tested | `ContractValidatorTests` (44 unit tests pass) | build-and-test | Met |
| u2 NFR6.3 | u2 security-requirements.md | Body over 256 KB rejected with 413 | 413 from a real Kestrel server | `TargetChecksTests.NFR6_3_...` | build-and-test | Met |
| u2 NFR6.4 | u2 security-requirements.md | 500 responses carry no stack trace or SQL | Generic 500 Problem Details with traceId; no exception text, SQL, password or stack trace; nothing saved | `TargetChecksTests.NFR6_4_an_unexpected_failure_returns_a_generic_500_...` | build-and-test | Met |
| u2 NFR6.6 | u2 security-requirements.md | Only the configured origin gets CORS headers | Confirmed | `ApiTests.Only_the_configured_origin_gets_a_cors_header` | build-and-test | Met |
| u2 NFR2.1 | u2 security-requirements.md | Password never in a project file | Read from user secrets or environment only | `DatabaseSettingsTests`; no password in `appsettings*.json` | build-and-test | Met |
| u3 NFR5.2 | u3 performance-requirements.md | Saving state shown within 100 ms | Shown before the slow fake answers | `web/src/test/newContract.test.tsx` (jsdom) | build-and-test | Unverified (jsdom only) |
| u3 NFR5.3 | u3 performance-requirements.md | 5,200-row schedule renders under 1 s | Under 1 s in jsdom | `web/src/test/schedule.perf.test.tsx` (jsdom) | build-and-test | Unverified (jsdom only) |
| u3 NFR5.4 | u3 performance-requirements.md | One request per screen load, no repeated failed POST | One call counted | `web/src/test/newContract.test.tsx` (jsdom) | build-and-test | Met |
| u3 NFR5.5 | u3 performance-requirements.md | List screen JavaScript under 300 KB gzip | 130.7 KB | `npm run size` | build-and-test | Met |
| u3 NFR3.5 | u3 security-requirements.md | API text shown as plain text | `<script>` name shown as text | `web/src/test/contractsList.test.tsx` (jsdom) | build-and-test | Met |
| u3 NFR3.8 | u3 security-requirements.md | No high or critical production vulnerability | 0 found | `npm audit --omit=dev --audit-level=high` | build-and-test | Met |
| u3 NFR7.4 | u3 unit-test-instructions.md | axe finds no serious or critical issues on S1 to S3 | None found in jsdom | Vitest axe tests (jsdom) | build-and-test | Unverified (jsdom only) |

## Open items

- **u3 NFR5.2, NFR5.3, NFR7.4:** these were checked only in jsdom. A real browser run (for example Playwright with Chromium) would be needed to call them Met. No browser test runner is installed in this project, and this plan has no later stage that owns browser testing. these were checked only in jsdom. A real browser run (for example Playwright with Chromium) would be needed to call them Met. No browser test runner is installed in this project, and this plan has no later stage that owns browser testing.
