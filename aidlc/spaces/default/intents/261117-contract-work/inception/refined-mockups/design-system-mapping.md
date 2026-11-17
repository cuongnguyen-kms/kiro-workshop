# Design System Mapping

Component library: **shadcn/ui** (answer to Q2). shadcn/ui copies component source into the project and builds on Radix UI primitives and Tailwind CSS. Radix supplies the keyboard and ARIA behavior that WCAG 2.2 AA (Q3) needs.

## Component Map

| UI need | shadcn/ui component | Notes |
|---|---|---|
| Buttons (Save, Cancel, Add stream, Remove, New contract) | `Button` | Variants: default for Save and New contract, outline for Cancel and Add stream, destructive-outline for Remove. |
| Text and amount inputs | `Input` with `Label` | Amount uses `inputmode="decimal"`. |
| Frequency select | `Select` | Options: Weekly, Monthly, Quarterly, Annually, One-time. |
| Expense method choice | `RadioGroup` | Options: Overtime, All at once, None. |
| Date fields | `Popover` + `Calendar` + `Input` | Calendar picker with typed-entry fallback. |
| Form layout and errors | `Form` (React Hook Form) with `FormMessage` | Field-level messages. Schema validation can be shared with the backend rules. |
| Stream card | `Card` | Wraps a `fieldset`. |
| Contracts list, schedules | `Table` | Wrapped by the ScheduleTable component. |
| Schedule tabs | `Tabs` | Radix tabs: arrow-key navigation built in. |
| Error summary, load failure, save failure | `Alert` | Use `role="alert"` for failures. |
| Save success banner | `Alert` with dismiss button, or `Sonner` toast | Banner preferred so it stays until dismissed. |
| Discard confirmation | `AlertDialog` | Focus trap and Escape handling built in. |
| Loading placeholders | `Skeleton` | |

## Design Tokens

Use the shadcn/ui default theme variables (CSS variables for `background`, `foreground`, `primary`, `destructive`, `muted`, `border`, `ring`) and change them only if a contrast check fails (see the accessibility checklist). Spacing uses Tailwind's default 4 px scale. Typography uses the system font stack.

| Token | Use |
|---|---|
| `primary` | Save contract, New contract |
| `destructive` | Field errors, Remove |
| `muted` | Section backgrounds, table header |
| `ring` | Focus outline (must be visible, see accessibility checklist) |

## Gaps

- shadcn/ui has no ready-made "money input". Build a thin wrapper around `Input` that formats to 2 decimals.
- No component for a repeating group of fields. The StreamCard uses React Hook Form's `useFieldArray`.
