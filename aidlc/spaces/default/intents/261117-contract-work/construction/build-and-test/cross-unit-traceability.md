# Cross-Unit Traceability

Source: `aidlc engine sensor-traceability --stage build-and-test`, final run. **Verdict: pass.** Every requirement and acceptance criterion is built or marked N/A with a reason. Nothing is uncovered.

Fixes made in this stage before the pass: the code-generation `traceability.json` of all three Units named test methods or several files in one target. The check needs one existing file per target, so each target now names one file. The u2 table also gained its `NFRx.y` rows (NFR1.1 to NFR7.1), which were missing.

## Functional requirements

| ID | Covered through | Built in |
|---|---|---|
| FR1.1, FR1.2 | AC1.1.1 to AC1.1.3 | u2 `ContractValidatorTests.cs`; u3 `newContract.test.tsx` |
| FR1.3, FR2.1 to FR2.4, FR7.2 | AC2.1.1 to AC2.1.6 | u2 `ContractValidatorTests.cs`, `ContractServiceTests.cs`; u3 `newContract.test.tsx` |
| FR1.4, FR3.1 to FR3.3, FR3.5, FR7.3 | AC3.1.1 to AC3.1.5 | u2 `ContractValidatorTests.cs`, `ContractServiceTests.cs`; u3 `newContract.test.tsx` |
| FR4.1 to FR4.3 | AC4.1.1 to AC4.1.6 | u1 `ScheduleGeneratorTests.cs`; u2 `ContractServiceTests.cs` (AC4.1.1) |
| FR3.4, FR5.1 to FR5.5 | AC5.1.1 to AC5.1.5 | u1 `ScheduleGeneratorTests.cs`; u2 `ApiTests.cs`, `ContractServiceTests.cs` |
| FR6.1 | AC6.1.1 to AC6.1.3 | u2 `ApiTests.cs`, `RepositoryTests.cs`, `ContractServiceTests.cs`; u3 `newContract.test.tsx` |
| FR6.2 | AC6.2.1, AC6.2.2 | u2 `RepositoryTests.cs`, `ApiTests.cs`; u3 `contractDetail.test.tsx` |
| FR6.3 | AC6.3.1, AC6.3.2 | u2 `RepositoryTests.cs`, `ApiTests.cs`; u3 `contractsList.test.tsx` |
| FR7.1, FR7.4 | AC1.x, AC2.x, AC3.x, AC6.1.x, AC6.2.x | u3 test files above |

## Non-functional requirements

| ID | Covered through (Unit: detailed ID) | N/A rows |
|---|---|---|
| NFR1 | u2: NFR1.1 to NFR1.3 | none |
| NFR2 | u2: NFR2.1 to NFR2.5 | u2 NFR2.2: limited database account applies only to a hosted environment, which this version has none of |
| NFR3 | u1: NFR3.1, NFR3.2; u2: NFR3.1, NFR3.2; u3: NFR3.4 to NFR3.8 | none |
| NFR4 | u2: NFR4.1 to NFR4.9 | u2 NFR4.6: backup is the PostgreSQL owner's job |
| NFR5 | u2: NFR5.1 to NFR5.9; u3: NFR5.2 to NFR5.5 | none |
| NFR6 | u1: NFR6.1, NFR6.2; u2: NFR6.1 to NFR6.12; u3: NFR6.3, NFR6.4 | none |
| NFR7 | u2: NFR7.1 | none |

## Uncovered elements

None.
