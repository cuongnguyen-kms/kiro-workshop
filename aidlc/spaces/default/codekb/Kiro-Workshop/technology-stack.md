# Technology Stack

## Backend
- C# on .NET 10 (`net10.0`), nullable and implicit usings on, warnings as errors.
- ASP.NET Core minimal API (`Microsoft.NET.Sdk.Web`), Microsoft.AspNetCore.OpenApi 10.0.4, Microsoft.OpenApi 2.13.1 (pinned for a security advisory).
- EF Core design 10.0.4, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3; PostgreSQL database (`contract_management`).
- Scheduling library: BannedApiAnalyzers 3.3.4 (build-time only).
- Money is `decimal` in C# and `numeric(18,2)` in PostgreSQL; JSON carries amounts as numbers.

## Frontend (`web/`)
- TypeScript ~6.0, React ^19.3.1, Vite ^8.3.6, react-router-dom ^7.
- Forms: react-hook-form ^7.92, zod ^4.6, @hookform/resolvers.
- UI: shadcn/ui on radix-ui ^1.8, Tailwind CSS ^4.3, lucide-react, react-day-picker, date-fns, class-variance-authority.
- Amounts are JavaScript numbers formatted with `Number.toFixed(2)`; no Intl number formatting is used in schedules.
- Node 22 or later required.

## Test and tooling
- xUnit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1 (.NET); Vitest ^5, Testing Library, vitest-axe, jsdom (web); ESLint 9 with typescript-eslint.
- `scripts/verify.ps1` (PowerShell) runs the whole check chain.

Dependency details: `dependencies.md`.
