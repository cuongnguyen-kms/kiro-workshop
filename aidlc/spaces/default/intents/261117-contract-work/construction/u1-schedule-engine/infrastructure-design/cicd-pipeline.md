# Delivery Pipeline: u1-schedule-engine

This unit is a .NET class library that is built into the backend (U2) and is not deployed on its own. It has no servers, storage or monitoring of its own, so its delivery pipeline is a build and test run only.

## Stages

| Stage | What runs | Gate to pass |
|---|---|---|
| 1. Restore | `dotnet restore` for the solution | Restores without errors. The library project has no package references (NFR1.2), so only the test project's packages are fetched. |
| 2. Build | `dotnet build --no-restore -warnaserror` for the library and its test project | Zero warnings and errors. The analyzer rule that bans file, network, environment and clock APIs (NFR3.2) is part of the build. |
| 3. Test | `dotnet test` for the library's test project | Every test passes, including the worked examples from the functional spec and the 200 ms performance check (NFR5.1). |
| 4. Verify | `scripts/verify.ps1`, the verification command recorded for every unit | Exit code 0. It runs stages 1 to 3 for the whole solution. |

## Deployment

The library is not deployed separately. It ships inside U2's build output as a project reference, so it moves through environments with U2. There is no blue-green, canary or rollback step of its own: rolling back U2 rolls back the library.

## Versioning and Artifacts

- No package is published. The library is referenced as a project, not as a NuGet package, because U2 is its only consumer and both live in the same solution (contract C2 ownership rules).
- A change to the C2 interface is made in one change set with U2's matching change.

## Secrets

None. The library uses no secrets, and the pipeline for this unit needs none.

## Environments

None of its own. It is built wherever the solution is built: the developer's machine now, and any build server added later.
