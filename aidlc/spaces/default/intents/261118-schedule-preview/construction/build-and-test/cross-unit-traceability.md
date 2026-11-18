# Cross-Unit Traceability

Source: `aidlc engine sensor-traceability --stage build-and-test`. **Verdict: pass.** Every requirement is built and traced to a file. Nothing is uncovered. This work has no Units, so all rows come from the one Code Generation pass.

| ID | Built in |
|---|---|
| FR1.1 to FR1.6 | `web/src/test/schedulePreview.test.tsx` |
| FR2.1, FR2.5 | `tests/ContractManagement.Api.IntegrationTests/PreviewApiTests.cs` |
| FR2.2, FR2.3, FR2.4, FR2.7 | `tests/ContractManagement.Api.Tests/ContractPreviewServiceTests.cs` |
| FR2.6 | `web/src/test/schedulePreview.test.tsx` |
| FR3.1 to FR3.7 | `web/src/test/schedulePreview.test.tsx` |
| NFR1 | `tests/ContractManagement.Api.IntegrationTests/PreviewApiTests.cs` |
| NFR2 | `tests/ContractManagement.Api.Tests/ContractPreviewServiceTests.cs` |
| NFR3 | `src/ContractManagement.Api/Endpoints/ContractEndpoints.cs` |
| NFR4 | `web/src/test/schedulePreview.test.tsx` |
| NFR5 | `tests/ContractManagement.Api.IntegrationTests/ApiTests.cs` |

## Uncovered elements

None.
