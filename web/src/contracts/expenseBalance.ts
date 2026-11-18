import type { BalanceRow } from '@/api/types'
import type { ScheduleColumn, ScheduleRow } from '@/components/ScheduleTable'
import { formatMoney, formatPeriod } from '@/contracts/display'

/**
 * The eight columns of the expense balance table, in order, with their exact headers (FR2).
 * Shared by the Expense schedule tab and the preview popup so the two cannot drift apart (AC2.1.4).
 */
export const EXPENSE_BALANCE_COLUMNS: ScheduleColumn[] = [
  { key: 'period', label: 'Period', align: 'left' },
  { key: 'expense', label: 'Expense', align: 'right' },
  { key: 'accrualPrepaid', label: '(Accrual)/Prepaid', align: 'right' },
  { key: 'cumulativeBalance', label: 'Cumulative Balance', align: 'right' },
  { key: 'stAccruedLiability', label: 'ST Accrued Liability', align: 'right' },
  { key: 'ltAccruedLiability', label: 'LT Accrued Liability', align: 'right' },
  { key: 'stPrepaidAsset', label: 'ST Prepaid Asset', align: 'right' },
  { key: 'ltPrepaidAsset', label: 'LT Prepaid Asset', align: 'right' },
]

/**
 * Builds one display row per balance row, in the order the API sent them. It only formats: the browser
 * never adds or splits money (FR4.1). A missing or non-array input is treated as an empty list (SEC2).
 */
export function expenseBalanceRows(rows: BalanceRow[] | null | undefined): ScheduleRow[] {
  if (!Array.isArray(rows)) return []
  return rows.map((r) => ({
    period: formatPeriod(r.periodStart),
    expense: formatMoney(r.expense),
    accrualPrepaid: formatMoney(r.accrualPrepaid),
    cumulativeBalance: formatMoney(r.cumulativeBalance),
    stAccruedLiability: formatMoney(r.stAccruedLiability),
    ltAccruedLiability: formatMoney(r.ltAccruedLiability),
    stPrepaidAsset: formatMoney(r.stPrepaidAsset),
    ltPrepaidAsset: formatMoney(r.ltPrepaidAsset),
  }))
}
