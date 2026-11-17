# Code Summary: u3-contract-web

## What was built

The React frontend for the contract management system, in `web/`:

- **Screens:** contracts list with pager (`/contracts`), New contract form (`/contracts/new`) and contract detail with payment and expense schedule tabs (`/contracts/:id`), as in the Refined Mockups.
- **API client** (`web/src/api/contracts.ts`, `types.ts`): the three contract C1 calls, no credentials, base address from `VITE_API_BASE_URL`, typed outcomes for validation errors, not found and failure.
- **Validation** (`web/src/contracts/validation.ts`): one Zod schema with the six agreed messages. Backend Problem Details errors are shown next to the matching fields.
- **Tests** in `web/src/test/`: smoke, New contract, contract detail, contracts list, accessibility (axe) and the 5,200-row render check, all against a fake API (`fakeApi.ts`) that follows C1.
- **Checks:** ESLint rules banning `dangerouslySetInnerHTML` and browser storage, TypeScript strict mode, and `web/scripts/size-check.mjs` (list screen JavaScript under 300 KB gzip).
- **Verification:** a frontend section in `scripts/verify.ps1` runs `npm i`, `npm audit --omit=dev --audit-level=high`, lint, typecheck, tests, build and the size check after the .NET steps, with a fallback to `C:\Program Files\nodejs` when npm is not on PATH.

## Built with

React 19, TypeScript 6, Vite 8, Tailwind CSS 4, shadcn/ui (Radix UI), React Router 7, React Hook Form, Zod 4, Vitest 5, React Testing Library, vitest-axe, ESLint 9. Node.js 22 or later.

## How to run

See `web/README.md`: `npm i`, then `npm run dev` (http://localhost:5173). Unit tests: `npm test -- --run` in `web/`.

## Test results

The unit's tests and the full verification are run and recorded by the unit checkpoint, which runs the recorded verification command (`scripts/verify.ps1`) and stores its output with the proof.

## Deviations from the plan

- At the person's request, the frontend section of `scripts/verify.ps1` installs packages with `npm i` instead of the plan's `npm ci`. `npm i` keeps the existing `node_modules` and can update `package-lock.json`, so the verification is faster but no longer installs exactly what the lock file pins.
- The build was interrupted twice (a usage limit, then a cancellation) after Step 9. Steps 1 to 9 were completed by the developer agent before the interruption; Steps 10 and 11 were finished afterwards. The person ran `npm ci` themselves to restore `node_modules`.
- The current shadcn/ui generates `import { cn } from "cn"` (the `cn` package from the shadcn-ui repository) instead of a local `lib/utils.ts` helper. That package needs Node.js 22, so `package.json` declares `"engines": { "node": ">=22" }` instead of the plan's "Node 20 or later". Node.js 26 is installed here.
