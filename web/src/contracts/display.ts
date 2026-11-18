// Display helpers. They format values the API returned; they never add or split money (NFR4.4).
import type { ExpenseMethod, Frequency } from '@/api/types'

/**
 * Shows an amount with exactly 2 decimals, a comma every 3 integer digits and brackets for a negative
 * amount: `(1,000.00)`. Any zero result, including -0 and -0.004, shows `0.00`. The result never depends on
 * the browser's language. Rounding is half away from zero on the decimal digit string, so no float is
 * multiplied. A value that is not a finite number (null, undefined, NaN, Infinity) gives an empty string.
 */
export function formatMoney(amount: number | null | undefined): string {
  if (typeof amount !== 'number' || !Number.isFinite(amount)) return ''
  const negative = amount < 0
  const [intPart, fracPart = ''] = Math.abs(amount).toFixed(10).split('.')
  let digits = intPart + fracPart.slice(0, 2)
  if (fracPart.charAt(2) >= '5') {
    // Round half away from zero: add one to the last kept digit and carry.
    const chars = digits.split('')
    let i = chars.length - 1
    while (i >= 0) {
      if (chars[i] === '9') {
        chars[i] = '0'
        i--
      } else {
        chars[i] = String(Number(chars[i]) + 1)
        break
      }
    }
    digits = (i < 0 ? '1' : '') + chars.join('')
  }
  const cents = digits.slice(-2)
  const whole = digits.slice(0, -2).replace(/^0+(?=\d)/, '')
  if (/^0*$/.test(whole + cents)) return '0.00'
  const text = `${whole.replace(/\B(?=(\d{3})+$)/g, ',')}.${cents}`
  return negative ? `(${text})` : text
}

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

/** Shows the month and year of a YYYY-MM-DD date as `Jan 2027`, in English. A malformed value is returned unchanged. */
export function formatPeriod(date: string): string {
  const match = /^(\d{4})-(\d{2})-\d{2}/.exec(date)
  const month = match ? Number(match[2]) : 0
  if (!match || month < 1 || month > 12) return date
  return `${MONTH_NAMES[month - 1]} ${match[1]}`
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
