// The form model and its validation rules (functional-spec.md, "Form Model" and "Validation Messages").
// The client check is a convenience; the API checks everything again and its answer wins.
import { z } from 'zod'
import type { CreateContractRequest, ExpenseMethod, Frequency, PreviewKind, PreviewRequest } from '@/api/types'

/** The six messages from the functional spec, worded exactly as the API returns them. */
export const MESSAGES = {
  nameRequired: 'Contract name is required.',
  endBeforeStart: 'End date must not be before the start date.',
  amountNotPositive: 'Amount must be greater than zero.',
  streamDatesOutside: 'Stream dates must be within the contract dates.',
  expenseDatesOutside: 'Expense dates must be within the contract dates.',
  expenseEndBeforeStart: 'Expense end date must not be before the start date.',
} as const

/**
 * Messages for missing or malformed entries. The functional spec does not word these; they are
 * client-side only and never replace an API message.
 */
export const ENTRY_MESSAGES = {
  dateFormat: 'Enter a date as YYYY-MM-DD.',
  frequencyRequired: 'Choose a frequency.',
  amountDecimals: 'Amount can have at most 2 decimal places.',
  nameTooLong: 'Contract name must be at most 200 characters.',
} as const

export const NAME_MAX_LENGTH = 200

export interface StreamFormValues {
  amount: string
  frequency: Frequency | ''
  startDate: string
  endDate: string
}

export interface ExpenseFormValues {
  method: ExpenseMethod
  startDate: string
  endDate: string
  date: string
}

export interface ContractFormValues {
  name: string
  startDate: string
  endDate: string
  paymentStreams: StreamFormValues[]
  expense: ExpenseFormValues
}

export const emptyStream = (): StreamFormValues => ({
  amount: '',
  frequency: '',
  startDate: '',
  endDate: '',
})

export const initialFormValues = (): ContractFormValues => ({
  name: '',
  startDate: '',
  endDate: '',
  paymentStreams: [emptyStream()],
  expense: { method: 'None', startDate: '', endDate: '', date: '' },
})

const DATE_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/
const AMOUNT_PATTERN = /^\d+(\.\d+)?$/

/** True when the text is a real calendar date written as YYYY-MM-DD. */
export function isIsoDate(value: string): boolean {
  const match = DATE_PATTERN.exec(value)
  if (!match) return false
  const [, y, m, d] = match.map(Number)
  const date = new Date(Date.UTC(y, m - 1, d))
  return date.getUTCFullYear() === y && date.getUTCMonth() === m - 1 && date.getUTCDate() === d
}

type Issue = { path: (string | number)[]; message: string }

/** Pure rule check, shared by the Zod schema. Returns every failing rule. */
export function checkContract(values: ContractFormValues): Issue[] {
  const issues: Issue[] = []
  const add = (path: (string | number)[], message: string) => issues.push({ path, message })

  if (values.name.trim() === '') add(['name'], MESSAGES.nameRequired)
  else if (values.name.length > NAME_MAX_LENGTH) add(['name'], ENTRY_MESSAGES.nameTooLong)

  const startOk = isIsoDate(values.startDate)
  const endOk = isIsoDate(values.endDate)
  if (!startOk) add(['startDate'], ENTRY_MESSAGES.dateFormat)
  if (!endOk) add(['endDate'], ENTRY_MESSAGES.dateFormat)
  else if (startOk && values.endDate < values.startDate) add(['endDate'], MESSAGES.endBeforeStart)

  const contractOk = startOk && endOk && values.endDate >= values.startDate
  const outside = (date: string) =>
    contractOk && (date < values.startDate || date > values.endDate)

  values.paymentStreams.forEach((stream, i) => {
    const at = (field: string) => ['paymentStreams', i, field]
    const amount = stream.amount.trim()
    if (!AMOUNT_PATTERN.test(amount) || Number(amount) <= 0) {
      add(at('amount'), MESSAGES.amountNotPositive)
    } else if (/\.\d{3,}$/.test(amount)) {
      add(at('amount'), ENTRY_MESSAGES.amountDecimals)
    }

    if (stream.frequency === '') add(at('frequency'), ENTRY_MESSAGES.frequencyRequired)

    const sStartOk = isIsoDate(stream.startDate)
    if (!sStartOk) add(at('startDate'), ENTRY_MESSAGES.dateFormat)
    else if (outside(stream.startDate)) add(at('startDate'), MESSAGES.streamDatesOutside)

    if (stream.frequency !== 'OneTime') {
      if (!isIsoDate(stream.endDate)) add(at('endDate'), ENTRY_MESSAGES.dateFormat)
      else if (outside(stream.endDate)) add(at('endDate'), MESSAGES.streamDatesOutside)
      else if (sStartOk && stream.endDate < stream.startDate) {
        // A stream ending before it starts cannot lie within the contract period as a range.
        add(at('endDate'), MESSAGES.streamDatesOutside)
      }
    }
  })

  const expense = values.expense
  if (expense.method === 'Overtime') {
    const eStartOk = isIsoDate(expense.startDate)
    const eEndOk = isIsoDate(expense.endDate)
    if (!eStartOk) add(['expense', 'startDate'], ENTRY_MESSAGES.dateFormat)
    else if (outside(expense.startDate)) add(['expense', 'startDate'], MESSAGES.expenseDatesOutside)
    if (!eEndOk) add(['expense', 'endDate'], ENTRY_MESSAGES.dateFormat)
    else if (eStartOk && expense.endDate < expense.startDate) {
      add(['expense', 'endDate'], MESSAGES.expenseEndBeforeStart)
    } else if (outside(expense.endDate)) add(['expense', 'endDate'], MESSAGES.expenseDatesOutside)
  } else if (expense.method === 'AllAtOnce') {
    if (!isIsoDate(expense.date)) add(['expense', 'date'], ENTRY_MESSAGES.dateFormat)
    else if (outside(expense.date)) add(['expense', 'date'], MESSAGES.expenseDatesOutside)
  }

  return issues
}

const streamSchema = z.object({
  amount: z.string(),
  frequency: z.union([z.enum(['Weekly', 'Monthly', 'Quarterly', 'Annually', 'OneTime']), z.literal('')]),
  startDate: z.string(),
  endDate: z.string(),
})

export const contractFormSchema = z
  .object({
    name: z.string(),
    startDate: z.string(),
    endDate: z.string(),
    paymentStreams: z.array(streamSchema).min(1),
    expense: z.object({
      method: z.enum(['Overtime', 'AllAtOnce', 'None']),
      startDate: z.string(),
      endDate: z.string(),
      date: z.string(),
    }),
  })
  .superRefine((values, ctx) => {
    for (const issue of checkContract(values)) {
      ctx.addIssue({ code: 'custom', path: issue.path, message: issue.message })
    }
  })

/** Builds the C1 request. Hidden fields are never sent; One-time sends the start date as the end date. */
export function toCreateRequest(values: ContractFormValues): CreateContractRequest {
  const expense = values.expense
  return {
    name: values.name.trim(),
    startDate: values.startDate,
    endDate: values.endDate,
    paymentStreams: values.paymentStreams.map((s) => ({
      amount: Number(s.amount),
      frequency: s.frequency as Frequency,
      startDate: s.startDate,
      endDate: s.frequency === 'OneTime' ? s.startDate : s.endDate,
    })),
    expense:
      expense.method === 'Overtime'
        ? { method: 'Overtime', startDate: expense.startDate, endDate: expense.endDate }
        : expense.method === 'AllAtOnce'
          ? { method: 'AllAtOnce', date: expense.date }
          : { method: 'None' },
  }
}

/**
 * Builds the preview request (C1 preview). The name is not sent. A payment preview sends no expense
 * fields, so an unfinished expense never matters to it (FR2.3).
 */
export function toPreviewRequest(values: ContractFormValues, kind: PreviewKind): PreviewRequest {
  const { name: _name, ...rest } = toCreateRequest(values)
  void _name
  return kind === 'payment' ? { ...rest, expense: { method: 'None' } } : rest
}

export const PREVIEW_MESSAGES = {
  payments: 'Enter valid contract dates and payment streams to preview.',
  expense: 'Enter valid contract dates, payment streams and expense details to preview.',
  noExpense: 'This contract has no expense schedule.',
} as const

// Previews never need the name, so its issue is left out (FR2.4).
const issuesIgnoringName = (values: ContractFormValues) => checkContract(values).filter((i) => i.path[0] !== 'name')

/** True when contract dates and every stream pass the form rules; name and expense are not needed (FR1.2). */
export function canPreviewPayments(values: ContractFormValues): boolean {
  return issuesIgnoringName(values).every((i) => i.path[0] === 'expense')
}

/** True when the expense method is not None and dates, streams and expense all pass (FR1.3). */
export function canPreviewExpense(values: ContractFormValues): boolean {
  return values.expense.method !== 'None' && issuesIgnoringName(values).length === 0
}

/** Why the payment button is disabled, or undefined when it is enabled (FR1.5). */
export function paymentPreviewReason(values: ContractFormValues): string | undefined {
  return canPreviewPayments(values) ? undefined : PREVIEW_MESSAGES.payments
}

/** Why the expense button is disabled, or undefined when it is enabled (FR1.4, FR1.5). */
export function expensePreviewReason(values: ContractFormValues): string | undefined {
  if (values.expense.method === 'None') return PREVIEW_MESSAGES.noExpense
  return canPreviewExpense(values) ? undefined : PREVIEW_MESSAGES.expense
}

/** Converts an API error key such as paymentStreams[0].amount to the form path paymentStreams.0.amount. */
export function apiKeyToFieldPath(key: string): string {
  return key.replace(/\[(\d+)\]/g, '.$1')
}

/** Every field path the form can show an error under, in screen order. */
export function formFieldPaths(values: ContractFormValues): string[] {
  const paths = ['name', 'startDate', 'endDate']
  values.paymentStreams.forEach((stream, i) => {
    paths.push(`paymentStreams.${i}.amount`, `paymentStreams.${i}.frequency`, `paymentStreams.${i}.startDate`)
    if (stream.frequency !== 'OneTime') paths.push(`paymentStreams.${i}.endDate`)
  })
  paths.push('expense.method')
  if (values.expense.method === 'Overtime') paths.push('expense.startDate', 'expense.endDate')
  if (values.expense.method === 'AllAtOnce') paths.push('expense.date')
  return paths
}

/** A stable DOM id for a field path, used by labels, error text and error-summary links. */
export function fieldId(path: string): string {
  return `field-${path.replace(/\./g, '-')}`
}
