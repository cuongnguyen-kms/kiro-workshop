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

export interface ContractDetail {
  id: string
  name: string
  startDate: string
  endDate: string
  paymentStreams: PaymentStream[]
  expense: ExpenseInput
  paymentSchedule: Schedule<PaymentEntry>
  expenseSchedule: Schedule<ExpenseEntry>
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
