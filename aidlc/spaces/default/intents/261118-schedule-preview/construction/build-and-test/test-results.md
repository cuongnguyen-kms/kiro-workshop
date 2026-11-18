# Test Results

Run on 2026-11-18, development machine, local PostgreSQL. Test strategy Minimal: no extra instruction files.

## Build

| Command | Result |
|---|---|
| `dotnet build --no-restore -warnaserror` (in `verify.ps1`) | Succeeded, 0 warnings, 0 errors |
| `npm run lint`, `npm run build` in `web/` | Passed (typecheck runs inside the build) |

## Tests

| Suite | Command | Total | Passed | Failed |
|---|---|---|---|---|
| Schedule library | `dotnet test` (in `verify.ps1`) | 19 | 19 | 0 |
| API unit (6 new preview tests) | `dotnet test` (in `verify.ps1`) | 50 | 50 | 0 |
| API integration (3 new preview tests) | `dotnet test` (in `verify.ps1`) | 27 | 27 | 0 |
| Frontend (7 new preview tests, jsdom) | `npm test -- --run` | 34 | 34 | 0 |

## Full verification

`scripts/verify.ps1`: **exit 1**. Every .NET step passed. It stopped at `npm audit --omit=dev --audit-level=high`, so lint, typecheck, tests, build and size did not run inside the script. I ran those five steps by hand and all passed.

### Failure details

`npm audit` reports 4 high findings from one new advisory, GHSA-fc5q-39xc-vjpf: a ReDoS in picomatch through repeated extglobs, affecting every picomatch version. The chain is `@tailwindcss/vite` 4.3.5, then `vite` 8.3.8, then `tinyglobby` 0.2.17 and `fdir` 6.5.0, then `picomatch` 4.0.7.

- **Not caused by this change.** This change added no package. The same audit passed on 2026-11-17 before the advisory was published.
- **Why it is counted as production.** `@tailwindcss/vite` and `tailwindcss` are listed under `dependencies` in `web/package.json`. They are build-time tools: none of their code is in the built bundle in `web/dist`.
- **npm's own fix is no good.** `npm audit fix --force` would install `vite@6.2.7`, a major downgrade that breaks the build. No fixed picomatch exists yet.

## Coverage

No line-coverage floor applies to the express scope.

## Target Verification Matrix

"jsdom" means the check ran in Node with a DOM emulator, not in a browser.

| Target ID | Source | Expected | Actual | Evidence | Owning Stage | Verdict |
|---|---|---|---|---|---|---|
| TC-SUITE | Testing Contract | Existing suites stay green | 130 of 130 tests pass | test runs above | build-and-test | Met |
| TC-MIN | Testing Contract | One test per requirement | Traceability passes | `cross-unit-traceability.md` | build-and-test | Met |
| NFR1 | requirements.md | Preview of 10 streams over 10 years within 2 s | Under 2 s | `PreviewApiTests.cs` | build-and-test | Met |
| NFR2 | requirements.md | Preview rows and totals equal Create's | Equal | `ContractPreviewServiceTests.cs` | build-and-test | Met |
| NFR3 | requirements.md | Existing API rules apply to preview | Same route group; invalid input gives 400 | `PreviewApiTests.cs` | build-and-test | Met |
| NFR4 | requirements.md | axe: no serious or critical issues with the popup open | None, in jsdom | `schedulePreview.test.tsx` | build-and-test | Unverified (jsdom only) |
| NFR5 | requirements.md | Additive API change only | OpenAPI test passes with the new path added | `ApiTests.cs` | build-and-test | Met |
| u3 NFR5.5 | 261117 u3 performance-requirements.md | List screen JS under 300 KB gzip | 130.8 KB | `npm run size` | build-and-test | Met |
| u3 NFR3.8 | 261117 u3 security-requirements.md | No high or critical advisory in a production dependency | 4 high (picomatch chain via `@tailwindcss/vite`) | `npm audit --omit=dev --audit-level=high` | build-and-test | Not Met |

## Decision on the failure

On 2026-11-18 the person chose **Accept failure** for u3 NFR3.8 (the picomatch advisory through `@tailwindcss/vite`). The target was not lowered: the matrix keeps it as Not Met. `web/package.json` is unchanged. The candidate fix, moving `@tailwindcss/vite` and `tailwindcss` to `devDependencies`, stays open for later.
