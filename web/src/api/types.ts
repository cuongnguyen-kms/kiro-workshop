// Types that mirror contract C1 (Contract API, /api/v1).
// Dates are ISO calendar dates (YYYY-MM-DD). Amounts are JSON numbers with at most 2 decimals.

export const FREQUENCIES = ['Weekly', 'Monthly', 'Quarterly', 'Annually', 'OneTime'] as const
export type Frequency = (typeof FREQUENCIES)[number]

export const EXPENSE_METHODS = ['Overtime', 'AllAtOnce', 'None'] as const
export type ExpenseMethod = (typeof EXPENSE_METHODS)[number]

export const PAGE_SIZES = [10, 20, 50, 100] as const
export type PageSize = (typeof PAGE_SIZES)[number]

export interface PaymentStreamInput {
  amount: number
  frequency: Frequency
  startDate: string
  endDate?: string
}

export interface ExpenseInput {
  method: ExpenseMethod
  startDate?: string
  endDate?: string
  date?: string
}

export interface CreateContractRequest {
  name: string
  startDate: string
  endDate: string
  paymentStreams: PaymentStreamInput[]
  expense: ExpenseInput
}

export interface PaymentStream {
  id: string
  amount: number
  frequency: Frequency
  startDate: string
  endDate: string
}

export interface PaymentEntry {
  streamId: string
  dueDate: string
  amount: number
}

export interface ExpenseEntry {
  periodStart: string
  periodEnd: string
  amount: number
}

export interface Schedule<T> {
  entries: T[]
  total: number
}

/** One month of the expense balance table (C1 v1.1.0). Amounts are signed where negative means accrual. */
export interface BalanceRow {
  periodStart: string
  periodEnd: string
  expense: number
  accrualPrepaid: number
  cumulativeBalance: number
  stAccruedLiability: number
  ltAccruedLiability: number
  stPrepaidAsset: number
  ltPrepaidAsset: number
}

/** The expense schedule: the v1.0 entries and total plus the monthly balance rows (C1 v1.1.0). */
export interface ExpenseSchedule extends Schedule<ExpenseEntry> {
  balanceRows: BalanceRow[]
}

export interface ContractDetail {
  id: string
  name: string
  startDate: string
  endDate: string
  paymentStreams: PaymentStream[]
  expense: ExpenseInput
  paymentSchedule: Schedule<PaymentEntry>
  expenseSchedule: ExpenseSchedule
}

export interface ContractSummary {
  id: string
  name: string
  startDate: string
  endDate: string
}

export interface ContractPage {
  items: ContractSummary[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

/** RFC 9457 Problem Details. */
export interface Problem {
  type: string
  title: string
  status: number
  detail?: string
  instance?: string
}

/** Field-level messages keyed by camelCase field path, for example paymentStreams[0].amount. */
export type ValidationErrors = Record<string, string[]>

export interface ValidationProblem extends Problem {
  errors: ValidationErrors
}

// Typed outcomes returned by the API client. Components never see raw responses.
export type CreateContractResult =
  | { kind: 'ok'; contract: ContractDetail }
  | { kind: 'validation'; errors: ValidationErrors }
  | { kind: 'failure' }

export type GetContractResult =
  | { kind: 'ok'; contract: ContractDetail }
  | { kind: 'notFound' }
  | { kind: 'failure' }

export type ListContractsResult = { kind: 'ok'; page: ContractPage } | { kind: 'failure' }

/** Which schedule a preview is for. A payment preview ignores the expense fields. */
export type PreviewKind = 'payment' | 'expense'

/** The preview request: a create request without the name, which a preview does not need. */
export type PreviewRequest = Omit<CreateContractRequest, 'name'>

/** One previewed payment row; streamNumber is the 1-based position of the stream in the form. */
export interface PreviewPaymentEntry {
  streamNumber: number
  dueDate: string
  amount: number
}

export interface SchedulePreview {
  paymentSchedule: Schedule<PreviewPaymentEntry>
  expenseSchedule: ExpenseSchedule
}

export type PreviewScheduleResult =
  | { kind: 'ok'; preview: SchedulePreview }
  | { kind: 'validation'; errors: ValidationErrors }
  | { kind: 'failure' }
