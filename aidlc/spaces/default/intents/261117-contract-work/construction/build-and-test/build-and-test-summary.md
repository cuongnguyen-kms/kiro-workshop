# Build and Test Summary

## Overall status

- **Build:** succeeded. The .NET solution builds with 0 warnings and 0 errors (warnings as errors), and the frontend lints, type-checks and builds.
- **Tests:** 114 tests, all passing. u1 has 19, u2 has 44 unit and 24 integration, and u3 has 27.
- **Verification command:** `scripts/verify.ps1` exits 0.
- **Prerequisites:** .NET 10 SDK, a local PostgreSQL with the password in user secrets, and Node.js LTS. See `build-instructions.md`.

## Test types

| Type | Where | Generated |
|---|---|---|
| Unit | `tests/ContractManagement.Scheduling.Tests`, `tests/ContractManagement.Api.Tests`, `web/src/test` | Code Generation |
| Integration (real PostgreSQL) | `tests/ContractManagement.Api.IntegrationTests` | Code Generation, plus `TargetChecksTests.cs` in this stage |
| Component (jsdom) | `web/src/test` | Code Generation |
| Integration, performance and security instruction files | none | Minimal strategy: not generated |

## Coverage expectations

The classic scope has no line-coverage floor. The Minimal strategy needs at least one test per requirement and a happy-path test per component. Cross-unit traceability passes for every FR, AC and NFR.

## Target Verification Matrix

| Target ID | Source | Expected | Actual | Evidence | Owning Stage | Verdict |
|---|---|---|---|---|---|---|
| TC-SUITE | Testing Contract | Existing suite green | 114 of 114 pass | `scripts/verify.ps1` exit 0 | build-and-test | Met |
| TC-MIN | Testing Contract | Requirement-driven tests | Traceability passes | `cross-unit-traceability.md` | build-and-test | Met |
| u1 NFR5.1 | u1 tech-stack-decisions.md | Generate under 200 ms | Under 200 ms | Scheduling performance test | build-and-test | Met |
| u1 NFR6.2 | u1 security-requirements.md | Over 100,000 entries rejected | Rejected | `TooManyEntries_Throws` | build-and-test | Met |
| u1 NFR3.2 | u1 security-requirements.md | No I/O or clock | Analyzer passes | build with warnings as errors | build-and-test | Met |
| u2 NFR5.1 | u2 performance-requirements.md | Create within 2 s | Under 2 s | `ApiTests` | build-and-test | Met |
| u2 NFR5.2 | u2 performance-requirements.md | Create at the cap within 5 s | Under 5 s | `TargetChecksTests` | build-and-test | Met |
| u2 NFR5.3 | u2 performance-requirements.md | Read at the cap within 500 ms | Under 500 ms warm (593 ms cold) | `TargetChecksTests` | build-and-test | Met |
| u2 NFR5.4 | u2 performance-requirements.md | List within 300 ms at 10,000 contracts | Under 300 ms | `TargetChecksTests` | build-and-test | Met |
| u2 NFR5.7 | u2 scalability-requirements.md | 10 parallel creates | All succeed, separate | `TargetChecksTests` | build-and-test | Met |
| u2 NFR4.1 | u2 reliability-requirements.md | Atomic save | Nothing stored on failure | `RepositoryTests` | build-and-test | Met |
| u2 NFR4.3 | u2 reliability-requirements.md | Exact money | Exact | `DataModelTests` | build-and-test | Met |
| u2 NFR4.7 | u2 reliability-requirements.md | `/health` 200 | 200 | `ApiTests` | build-and-test | Met |
| u2 NFR4.9 | u2 tech-stack-decisions.md | OpenAPI matches C1 | Match | `ApiTests` | build-and-test | Met |
| u2 NFR6.1 | u2 security-requirements.md | Server-side rules and messages | All tested | `ContractValidatorTests` | build-and-test | Met |
| u2 NFR6.3 | u2 security-requirements.md | 413 over 256 KB | 413 | `TargetChecksTests` (Kestrel) | build-and-test | Met |
| u2 NFR6.4 | u2 security-requirements.md | 500 without internal detail | Generic 500, no detail, nothing saved | `TargetChecksTests` | build-and-test | Met |
| u2 NFR6.6 | u2 security-requirements.md | CORS for the configured origin only | Confirmed | `ApiTests` | build-and-test | Met |
| u2 NFR2.1 | u2 security-requirements.md | No password in files | None | `DatabaseSettingsTests` | build-and-test | Met |
| u3 NFR5.2 | u3 performance-requirements.md | Saving state within 100 ms | Shown, jsdom | `newContract.test.tsx` | build-and-test | Unverified (jsdom only) |
| u3 NFR5.3 | u3 performance-requirements.md | 5,200 rows under 1 s | Under 1 s, jsdom | `schedule.perf.test.tsx` | build-and-test | Unverified (jsdom only) |
| u3 NFR5.4 | u3 performance-requirements.md | One request per screen | One | `newContract.test.tsx` | build-and-test | Met |
| u3 NFR5.5 | u3 performance-requirements.md | Under 300 KB gzip | 130.7 KB | `npm run size` | build-and-test | Met |
| u3 NFR3.5 | u3 security-requirements.md | Plain text only | Confirmed | `contractsList.test.tsx` | build-and-test | Met |
| u3 NFR3.8 | u3 security-requirements.md | No high production vulnerability | 0 | `npm audit` | build-and-test | Met |
| u3 NFR7.4 | u3 unit-test-instructions.md | axe: no serious or critical issues | None, jsdom | Vitest axe tests | build-and-test | Unverified (jsdom only) |

## Readiness

- **Build-ready:** yes.
- **Test-ready:** yes. Every command passes.
- **Deployment-ready:** for local use, yes. No hosting target or pipeline is in this plan.

## Known limitations

1. **u3 NFR5.2, NFR5.3, NFR7.4:** checked in jsdom, not in a real browser. No browser runner is installed and no later stage in this plan owns browser testing.
2. **u2 test instructions:** the two-project `dotnet build` line in `unit-test-instructions.md` does not run as written. Each project builds on its own.
3. **Development dependencies:** `npm i` reports 4 moderate vulnerabilities in development dependencies. These are outside the NFR3.8 rule, which covers high and critical production dependencies only.
