# Contracts web app (u3-contract-web)

React 19 + TypeScript + Vite frontend for the contract management system, using shadcn/ui (Radix UI + Tailwind CSS v4), React Router, React Hook Form and Zod. It calls the backend only through contract C1 (`/api/v1`) and never calculates schedules or money.

## Prerequisites

- Node.js 22 or later with npm (the `cn` class-name package needs Node 22).

## Install

```
npm i
```

## Configure

Copy `.env.example` to `.env.local` and set the API address:

```
VITE_API_BASE_URL=https://api.example.com
```

Leave it empty to call the same address the page was loaded from. The value is public (built into the bundle); never put secrets in `.env` files. `.env` and `.env.local` are git-ignored.

## Run

```
npm run dev
```

Opens on http://localhost:5173. Routes: `/contracts` (list), `/contracts/new` (new contract), `/contracts/:id` (detail).

## Test and check

| Command | What it does |
|---|---|
| `npm test -- --run` | Vitest component tests against a fake API (`src/test/fakeApi.ts`), including axe accessibility checks and the 5,200-row render check |
| `npm run lint` | ESLint, including the bans on `dangerouslySetInnerHTML`, `innerHTML` and browser storage |
| `npm run typecheck` | TypeScript strict mode |
| `npm run build` | Production build into `dist/` |
| `npm run size` | Fails when the list screen's JavaScript is over 300 KB gzip (run after `build`) |

`scripts/verify.ps1` at the repository root runs all of these after the .NET checks and stops at the first failure.
