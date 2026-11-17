// Display helpers. They format values the API returned; they never add, split or round money (NFR4.4).
import type { ExpenseMethod, Frequency } from '@/api/types'

/** Shows an amount with exactly 2 decimals. Amounts from the API already have at most 2 decimals. */
export function formatMoney(amount: number): string {
  return amount.toFixed(2)
}

/** Shows a date as YYYY-MM-DD, the same as the API. */
export function formatDate(date: string): string {
  return date.slice(0, 10)
}

/** Shows the year and month of a date, YYYY-MM. */
export function formatMonth(date: string): string {
  return date.slice(0, 7)
}

export const FREQUENCY_LABELS: Record<Frequency, string> = {
  Weekly: 'Weekly',
  Monthly: 'Monthly',
  Quarterly: 'Quarterly',
  Annually: 'Annually',
  OneTime: 'One-time',
}

export const EXPENSE_METHOD_LABELS: Record<ExpenseMethod, string> = {
  Overtime: 'Overtime',
  AllAtOnce: 'All at once',
  None: 'None',
}
