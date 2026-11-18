# Evidence

Collaborators are off: the lead worked alone, no support reviews.

## Inspected

- Git: `git log --oneline -n 30` (2 commits: `845f67e Initial`, `49b7e5c Frontend: page long schedules, fix lint in test fake, use npm i in verify`) and `git branch -a` (`development`, `origin/development` only).
- Reverse-engineering artifacts in `aidlc/spaces/default/codekb/Kiro-Workshop/`: `code-structure.md`, `technology-stack.md`, `code-quality-assessment.md`. Not opened: `dependencies.md`, `architecture.md`, `business-overview.md`.
- Workspace config: `scripts/verify.ps1`, `web/eslint.config.js`, `.gitignore`, repository root listing (no CI folder, no root README, no formatter config). The `.csproj` files, `ContractManagement.slnx` and `web/package.json` were not re-read; their content comes from `technology-stack.md`.
- Memory: `team.md` sections were empty templates (first discovery); `org.md` supplies framework defaults.
- Earlier records: `261117-contract-work` build-and-test-summary, `261118-schedule-preview` test-results.

## Interview decisions

- Q1 A: commit straight to `development`, the branch all work merges into. Integration branch: `development`.
- Q2 A: no walking skeleton; the app already runs end to end, so changes are built directly.
- Q3 A: test-after. Build each layer, then write and run its tests.
- Q4 A: no coverage minimum; every requirement must have a test and all tests must pass.
- Q5 A: release by running `scripts/verify.ps1` locally; no deployment for now.
- Q6 A: nothing more for code style: ESLint for the frontend, warnings as errors for .NET, follow the existing code's patterns.

## Remaining uncertainty

- The accepted `npm audit` picomatch failure was not part of the interview; it is carried forward from `261118-schedule-preview` as still accepted.
- Duplicated schedule column definitions (`ScheduleTabs.tsx`, `SchedulePreview.tsx`) were not decided; the code-style answer (follow existing patterns) implies leaving them unless this change touches them.
- No stated hard constraints, so `discovered-rules.md` holds `None.` in both sections.
