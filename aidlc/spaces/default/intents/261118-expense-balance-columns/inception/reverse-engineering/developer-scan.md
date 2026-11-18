## Developer Code Scan Results

Focused scan (store STALE) for intent expense-balance-columns. Brownfield, depth Standard.

### Scan Coverage
- **Analyzed deeply**:
  - src/ContractManagement.Scheduling/ (Models.cs, ScheduleGenerator.cs, ExpenseScheduleBuilder.cs, PaymentScheduleBuilder.cs)
  - src/ContractManagement.Api/Contracts/ (Responses.cs, Preview.cs)
  - src/ContractManagement.Api/Services/ (ContractService.cs)
  - src/ContractManagement.Api/Persistence/ (ContractDbContext.cs; Migrations/ file names only)
  - web/src/contracts/ (display.ts, ScheduleTabs.tsx, SchedulePreview.tsx, StreamsSummary.tsx; other files listed)
  - web/src/components/ (ScheduleTable.tsx; ui/ listed)
  - web/src/api/ (types.ts, contracts.ts preview function)
- **Skimmed only**: Persistence/ContractRepository.cs, DatabaseSettings.cs, DesignTimeFactory.cs (names only); Scheduling InputChecker.cs and PaymentDates.cs (names only); web/src/components/ui/ (names only); web/src/contracts/ContractForm.tsx, StreamCard.tsx, ExpenseSection.tsx, validation.ts (grep only); outside the snapshot, a grep only: src/ContractManagement.Api/Endpoints/ContractEndpoints.cs (preview route) and Domain/Contract.cs (entity names, `Position`, decimal `Amount`).
- **Left out**: bin/ and obj/ beside the .csproj files, web node_modules/ and dist/; AI-DLC's own install (.kiro/, aidlc/); tests/ and web/src/test/ (not opened).

### Packages Found
- ContractManagement.Scheduling — library — C# — stateless C2 schedule engine
- ContractManagement.Api — web API — C# — endpoints, service, persistence
- web — SPA — TypeScript/React — contract form, preview popup, detail tabs

### Build System
- **Type**: .NET solution (slnx) plus Vite/npm
- **Config Files**: ContractManagement.slnx, *.csproj, web/package.json, scripts/verify.ps1 (names only, not re-read)
- **Build Dependencies**: Api references Scheduling

### APIs Discovered
- REST — ContractEndpoints.cs — createContract, previewContractSchedule (`POST /api/v1/contracts/preview?kind=payment|expense`, new since last scan), listContracts, getContract
- Internal C2 — `IScheduleGenerator.Generate` in Models.cs

### Frameworks & Libraries
- Unchanged from the store (.NET 10, EF Core with Npgsql, React 19, Vite, Zod, react-hook-form, shadcn/radix). `components/ui/dialog.tsx` now exists (used by SchedulePreview).

### Test Coverage
- **Test Directories**: not opened this run
- **Test Frameworks**: unchanged (xUnit, Vitest)
- **Coverage Config**: absent (prior store)

### Code Quality Indicators
- **Linting**: unchanged (prior store)
- **CI/CD**: none (prior store)
- **Documentation**: XML summaries on public C# types

### Technical Debt Signals
- Schedule row mapping is duplicated: expense rows in ScheduleTabs.tsx (inline, in a useMemo) and in SchedulePreview.tsx PreviewBody; payment columns and expense column sets are duplicated across the two files. Any new column must be changed in both.
- `ScheduleRow` is `Record<string, string>`: every cell is a pre-formatted string, so number styling (negatives) happens only in `formatMoney`.
- Preview payment rows use `streamNumber`, saved rows use `streamId` plus a label from the streams list, so the two payment row types differ.

## Handoff Summary
- **Intent-relevant finding**: The expense schedule is built by `ExpenseScheduleBuilder.Build(expense, baseAmount)` (Scheduling/ExpenseScheduleBuilder.cs). `baseAmount` is the payment total (`PaymentScheduleBuilder`), so expense total equals payment total unless method None. Rows are `ExpenseEntry(PeriodStart, PeriodEnd, Amount)`: Overtime gives one row per calendar month, pro-rated by days and rounded to 2 decimals with the last row taking the remainder (`baseAmount - allocated`, so it can be small or in theory negative); AllAtOnce gives one row with start = end = date. Payments are `PaymentEntry(StreamId, DueDate, Amount)` with amount always above zero, ordered by due date. The two schedules are independent lists: no code relates a payment to an expense period. Accrual/prepaid needs a new calculation joining payments to expense periods by date (cumulative payments due up to a period end versus cumulative expense up to that period end), plus a split of the balance into short-term (within 12 months of the period end) and long-term. Nothing in the repo defines ST/LT cut-offs. Display: `web/src/contracts/display.ts` `formatMoney(amount) = amount.toFixed(2)`, used for every amount and total in ScheduleTabs.tsx, SchedulePreview.tsx and StreamsSummary.tsx; `toFixed` gives `-1000.00` for a negative, no thousands separator. `ScheduleTable.tsx` takes columns `{key,label,align}` and string rows; it right-aligns only the LAST column (`lastRight`, by CSS `td:last-child`) and the total row uses `colSpan = columns.length - 1` with one total cell, so a table with several amount columns needs changes there. Saved schedules are returned by `ContractMapper.ToDetail` (Responses.cs) from stored entries (`payment_schedule_entries`, `expense_schedule_entries`, numeric(18,2)); preview comes from `ContractService.Preview` as `SchedulePreviewResponse` (Preview.cs). Both use `ExpenseEntryResponse(PeriodStart, PeriodEnd, Amount)`, and the web type is `ExpenseEntry` in api/types.ts.
- **Risks / follow-up**:
  - Where negatives can appear: today stored/previewed payment and expense amounts are positive (inputs validated above zero), so a negative could show only for the Overtime last-row remainder in a rounding edge case. The new Accrual/Prepaid and Cumulative Balance columns will be signed (accrual versus prepaid), so formatting parentheses applies to those, to totals, and ideally every amount via `formatMoney`. Zero and `-0.00` need a rule (toFixed(-0.001) gives "-0.00").
  - Computing the new columns server-side (response records, mapper, preview) or client-side is open. `display.ts` states formatters never add, split or round money (NFR4.4), and ScheduleTable says totals come from the API and are never summed in the client, so this points to server-side calculation with new fields on `ExpenseEntryResponse` (or a new expense-row record) and no database column needed if computed at read/preview time. Stored rows have only PeriodStart, PeriodEnd, Amount; a persisted column would need a new EF migration (only InitialCreate exists).
  - AllAtOnce rows have one date (start = end); the display shows `date`, the Overtime display shows `month` (YYYY-MM). Period end for the balance-sheet date is available in the data but not shown.
  - Contract with expense method None has no expense schedule (the tab shows a message), so the new columns do not apply there.
  - Payments may fall before, inside, or after the expense range; the cumulative payment versus cumulative expense comparison must handle all three. Total payments equal total expense, so the balance ends at zero.
  - Existing frontend tests under web/src/test and API tests assert the current two-column output and `toFixed` strings; they were not opened and will need updating.
