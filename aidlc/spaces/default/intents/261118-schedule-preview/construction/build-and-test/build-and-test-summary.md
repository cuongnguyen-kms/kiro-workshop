# Build and Test Summary

## Overall status

- **Build:** succeeds (.NET with warnings as errors, and the frontend build).
- **Tests:** 130 pass, 0 fail. The schedule library has 19, the API 50 unit and 27 integration, and the frontend 34.
- **Verification command:** fails at the dependency audit (u3 NFR3.8). A new picomatch advisory reaches the production dependency list through `@tailwindcss/vite`. This change did not cause it.

## Test types

| Type | Location |
|---|---|
| Unit (service, with fakes) | `tests/ContractManagement.Api.Tests/ContractPreviewServiceTests.cs` |
| Integration (real PostgreSQL) | `tests/ContractManagement.Api.IntegrationTests/PreviewApiTests.cs` |
| Component (jsdom, with axe) | `web/src/test/schedulePreview.test.tsx` |

## Target Verification Matrix

| Target ID | Source | Expected | Actual | Evidence | Owning Stage | Verdict |
|---|---|---|---|---|---|---|
| TC-SUITE | Testing Contract | Suites green | 130 of 130 pass | test runs | build-and-test | Met |
| TC-MIN | Testing Contract | One test per requirement | Traceability passes | `cross-unit-traceability.md` | build-and-test | Met |
| NFR1 | requirements.md | Preview within 2 s | Under 2 s | `PreviewApiTests.cs` | build-and-test | Met |
| NFR2 | requirements.md | Preview equals Create | Equal | `ContractPreviewServiceTests.cs` | build-and-test | Met |
| NFR3 | requirements.md | Existing API rules | Applied | `PreviewApiTests.cs` | build-and-test | Met |
| NFR4 | requirements.md | axe clean with popup open | Clean in jsdom | `schedulePreview.test.tsx` | build-and-test | Unverified (jsdom only) |
| NFR5 | requirements.md | Additive API change | Additive | `ApiTests.cs` | build-and-test | Met |
| u3 NFR5.5 | earlier u3 requirement | Under 300 KB | 130.8 KB | `npm run size` | build-and-test | Met |
| u3 NFR3.8 | earlier u3 requirement | No high production advisory | 4 high | `npm audit` | build-and-test | Not Met |

## Readiness

- **Build-ready:** yes.
- **Test-ready:** yes. Every test passes.
- **Deployment-ready:** no, until the audit finding is resolved or accepted.

## Known limitations

- NFR4 was checked in jsdom, not in a real browser.
- The audit finding has no upstream fix yet. The candidate fix is in `test-results.md`.
