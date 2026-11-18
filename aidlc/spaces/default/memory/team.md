# Team-Level Rules

> This team's affirmed practices and corrections. Loaded after `org.md` as
> strict-additive guidance.
> Populated by the practices-discovery affirmation gate. Edit at the gate,
> not directly.

## Way of Working

- **Integration branch**: development
- `development` is the only branch, locally and on `origin`, and it is the branch all our work merges into. There is no `main`.
- We commit straight to `development`. History is short and linear, with no feature branches and no pull request flow.

## Walking Skeleton

- We do not build a thin end-to-end slice first. The app already runs end to end, so we build changes directly.

## Testing Posture

- **Methodology**: test-after
- **Methodology evidence**: Earlier build-and-test records show tests written during Code Generation with traceability checked afterwards (one test per requirement). No feature files, scenario specs or test-first history exist, and the team confirmed test-after in the interview.
- **Ordering**: We implement each layer first, then write and run that layer's tests, then run `scripts/verify.ps1`.
- We set no coverage minimum. Every requirement must have a test, and all tests must pass.
- Test stack: xUnit for .NET (unit and integration against a local PostgreSQL), Vitest with Testing Library, vitest-axe and jsdom for the web.
- Existing tests assert the current schedule table output (two-column expense rows, `toFixed` strings), so we update them with this change.
- Checks that need a real browser (timing, axe) run only in jsdom, and we accept that.

## Guard Policy

<!-- Affirmed by the team. Mode: strict, relaxed, or off. Strict here holds for every intent, whatever one person asks in chat; relaxed or off here applies to every intent whose policy came from its scope, unless project.md or the person sets another; changing this line changes it. A section under the retired Change Control heading, written by an earlier release, is still read. -->

## Deployment

- We release by running `scripts/verify.ps1` locally. We have no deployment target or pipeline for now.
- `scripts/verify.ps1` runs: `dotnet restore`, `build -warnaserror`, `test`, then in `web/` `npm i`, `audit --omit=dev --audit-level=high`, `lint`, `typecheck`, `test --run`, `build`, `size` (300 KB gzip limit).
- The `npm audit` picomatch advisory through `@tailwindcss/vite` was accepted in `261118-schedule-preview`. It was not re-raised in the interview, so we treat it as still open and accepted.

## Code Style

- We add nothing beyond existing tooling: ESLint for the frontend, warnings as errors for .NET, and we follow the existing code's patterns.
- C#: .NET 10, nullable on, XML summaries on public types, primary-constructor DI, records for DTOs. The Scheduling library forbids I/O through a banned-symbols analyzer.
- TypeScript: strict, ESLint 9 with typescript-eslint and `--max-warnings 0`. Rules ban `dangerouslySetInnerHTML`, `innerHTML` and browser storage. `@/` path alias, typed result unions for API calls.
- Money is `decimal` / `numeric(18,2)` on the server and formatted with `formatMoney` (`toFixed(2)`) on the client.
- No Prettier, EditorConfig or dotnet format configuration exists, so formatting is not enforced by tooling.
## Forbidden

<!-- Team-specific forbidden patterns -->

## Mandated

<!-- Team-specific mandates -->

## Corrections

<!-- Self-learning loop appends here. -->
