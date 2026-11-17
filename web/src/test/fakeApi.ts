// A fake of web/src/api/contracts.ts that follows contract C1. Tests replace the real module with this one:
//   vi.mock('@/api/contracts', () => import('@/test/fakeApi'))
// Each function is a vi.fn, so a test can set the outcome (201, 400, 404, failure, a ContractPage)
// and check how often it was called.
import { vi } from 'vitest'
import type {
  ContractDetail,
  ContractPage,
  ContractSummary,
  CreateContractRequest,
  CreateContractResult,
  GetContractResult,
  ListContractsResult,
  PaymentEntry,
  ValidationErrors,
} from '@/api/types'

export const CONTRACT_ID = '3f1c2b7a-8d4e-4f6a-9b1c-2d3e4f5a6b7c'
export const STREAM_ID = '9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d'

/** A saved contract from the stories: a Monthly stream of 1000 and an AllAtOnce expense on 2027-03-01. */
export function sampleContract(overrides: Partial<ContractDetail> = {}): ContractDetail {
  const entries: PaymentEntry[] = Array.from({ length: 12 }, (_, i) => ({
    streamId: STREAM_ID,
    dueDate: `2027-${String(i + 1).padStart(2, '0')}-15`,
    amount: 1000,
  }))
  return {
    id: CONTRACT_ID,
    name: 'Office lease HQ',
    startDate: '2027-01-01',
    endDate: '2027-12-31',
    paymentStreams: [
      { id: STREAM_ID, amount: 1000, frequency: 'Monthly', startDate: '2027-01-15', endDate: '2027-12-15' },
    ],
    expense: { method: 'AllAtOnce', date: '2027-03-01' },
    paymentSchedule: { entries, total: 12000 },
    expenseSchedule: {
      entries: [{ periodStart: '2027-03-01', periodEnd: '2027-03-01', amount: 12000 }],
      total: 12000,
    },
    ...overrides,
  }
}

/** The C1 answer to POST /contracts: the saved contract built from the request. */
export function contractFromRequest(request: CreateContractRequest): ContractDetail {
  const streams = request.paymentStreams.map((s, i) => ({
    id: `00000000-0000-4000-8000-00000000000${i}`,
    amount: s.amount,
    frequency: s.frequency,
    startDate: s.startDate,
    endDate: s.endDate ?? s.startDate,
  }))
  return {
    id: CONTRACT_ID,
    name: request.name,
    startDate: request.startDate,
    endDate: request.endDate,
    paymentStreams: streams,
    expense: request.expense,
    paymentSchedule: {
      entries: streams.map((s) => ({ streamId: s.id, dueDate: s.startDate, amount: s.amount })),
      total: streams.reduce((sum, s) => sum + s.amount, 0),
    },
    expenseSchedule: { entries: [], total: 0 },
  }
}

export function summary(index: number, overrides: Partial<ContractSummary> = {}): ContractSummary {
  return {
    id: `c0000000-0000-4000-8000-${String(index).padStart(12, '0')}`,
    name: `Contract ${index}`,
    startDate: '2027-01-01',
    endDate: '2027-12-31',
    ...overrides,
  }
}

/** The C1 answer to GET /contracts: one page out of totalCount contracts. */
export function contractPage(totalCount: number, page = 1, pageSize = 10): ContractPage {
  const totalPages = Math.ceil(totalCount / pageSize)
  const first = (page - 1) * pageSize
  const count = Math.max(0, Math.min(pageSize, totalCount - first))
  return {
    items: Array.from({ length: count }, (_, i) => summary(first + i + 1)),
    page,
    pageSize,
    totalCount,
    totalPages,
  }
}

export const validationResult = (errors: ValidationErrors): CreateContractResult => ({
  kind: 'validation',
  errors,
})

export const createContract = vi.fn(
  async (input: CreateContractRequest, _signal?: AbortSignal): Promise<CreateContractResult> => ({
    kind: 'ok',
    contract: contractFromRequest(input),
  }),
)

export const listContracts = vi.fn(
  async (page: number, pageSize: number, _signal?: AbortSignal): Promise<ListContractsResult> => ({
    kind: 'ok',
    page: contractPage(3, page, pageSize),
  }),
)

export const getContract = vi.fn(
  async (id: string, _signal?: AbortSignal): Promise<GetContractResult> =>
    id === CONTRACT_ID ? { kind: 'ok', contract: sampleContract() } : { kind: 'notFound' },
)

const defaults = {
  createContract: createContract.getMockImplementation()!,
  listContracts: listContracts.getMockImplementation()!,
  getContract: getContract.getMockImplementation()!,
}

/** Puts every fake back to its C1 default answer and clears the call history. */
export function resetFakeApi(): void {
  createContract.mockReset().mockImplementation(defaults.createContract)
  listContracts.mockReset().mockImplementation(defaults.listContracts)
  getContract.mockReset().mockImplementation(defaults.getContract)
}
