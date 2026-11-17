// The only module that calls the backend (contract C1).
// - Sends no credentials and no Authorization header (NFR3.4).
// - Base address from VITE_API_BASE_URL; empty means the page's own address (NFR3.7).
// - Every call accepts an AbortSignal so a screen can cancel a stale request (NFR5.4).
// - Never retries on its own, so a POST can never be sent twice by the client (W1 step 7).
import type {
  ContractDetail,
  ContractPage,
  CreateContractRequest,
  CreateContractResult,
  GetContractResult,
  ListContractsResult,
  ValidationErrors,
} from './types'

function baseUrl(): string {
  const configured = (import.meta.env.VITE_API_BASE_URL ?? '').trim()
  return `${configured.replace(/\/+$/, '')}/api/v1`
}

async function send(path: string, init: RequestInit): Promise<Response> {
  return fetch(`${baseUrl()}${path}`, {
    ...init,
    credentials: 'omit',
    headers: { Accept: 'application/json, application/problem+json', ...init.headers },
  })
}

/** Reads a JSON body, or returns undefined when the body is not JSON (treated as a failure). */
async function readJson(response: Response): Promise<unknown> {
  const type = response.headers.get('Content-Type') ?? ''
  if (!type.includes('json')) return undefined
  try {
    return await response.json()
  } catch {
    return undefined
  }
}

function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function readValidationErrors(body: unknown): ValidationErrors | undefined {
  if (!isObject(body) || !isObject(body.errors)) return undefined
  const result: ValidationErrors = {}
  for (const [key, value] of Object.entries(body.errors)) {
    if (Array.isArray(value)) {
      result[key] = value.filter((m): m is string => typeof m === 'string')
    }
  }
  return result
}

function logFailure(operation: string, error: unknown): void {
  if (error instanceof DOMException && error.name === 'AbortError') return
  console.error(`Contract API ${operation} failed`, error)
}

export async function createContract(
  input: CreateContractRequest,
  signal?: AbortSignal,
): Promise<CreateContractResult> {
  try {
    const response = await send('/contracts', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(input),
      signal,
    })
    const body = await readJson(response)
    if (response.status === 201 && isObject(body)) {
      return { kind: 'ok', contract: body as unknown as ContractDetail }
    }
    if (response.status === 400) {
      const errors = readValidationErrors(body)
      if (errors) return { kind: 'validation', errors }
    }
    logFailure('createContract', new Error(`Unexpected status ${response.status}`))
    return { kind: 'failure' }
  } catch (error) {
    logFailure('createContract', error)
    return { kind: 'failure' }
  }
}

export async function listContracts(
  page: number,
  pageSize: number,
  signal?: AbortSignal,
): Promise<ListContractsResult> {
  try {
    const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
    const response = await send(`/contracts?${query.toString()}`, { method: 'GET', signal })
    const body = await readJson(response)
    if (response.ok && isObject(body) && Array.isArray(body.items)) {
      return { kind: 'ok', page: body as unknown as ContractPage }
    }
    logFailure('listContracts', new Error(`Unexpected status ${response.status}`))
    return { kind: 'failure' }
  } catch (error) {
    logFailure('listContracts', error)
    return { kind: 'failure' }
  }
}

export async function getContract(id: string, signal?: AbortSignal): Promise<GetContractResult> {
  try {
    const response = await send(`/contracts/${encodeURIComponent(id)}`, { method: 'GET', signal })
    if (response.status === 404) return { kind: 'notFound' }
    const body = await readJson(response)
    if (response.ok && isObject(body)) {
      return { kind: 'ok', contract: body as unknown as ContractDetail }
    }
    logFailure('getContract', new Error(`Unexpected status ${response.status}`))
    return { kind: 'failure' }
  } catch (error) {
    logFailure('getContract', error)
    return { kind: 'failure' }
  }
}
