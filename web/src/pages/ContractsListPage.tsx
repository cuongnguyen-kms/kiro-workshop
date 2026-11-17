import { useCallback, useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { listContracts } from '@/api/contracts'
import { PAGE_SIZES, type ContractPage } from '@/api/types'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ContractsTable } from '@/contracts/ContractsTable'
import { Pager } from '@/contracts/Pager'

type LoadState = { kind: 'loading' } | { kind: 'loaded'; data: ContractPage } | { kind: 'error' }

export const DEFAULT_PAGE_SIZE = 10
export const NO_CONTRACTS_MESSAGE = 'No contracts yet.'
export const CONTRACTS_LOAD_FAILED_MESSAGE = 'Could not load contracts.'

function readPage(value: string | null): number {
  const n = Number(value)
  return Number.isInteger(n) && n >= 1 ? n : 1
}

function readPageSize(value: string | null): number {
  const n = Number(value)
  return (PAGE_SIZES as readonly number[]).includes(n) ? n : DEFAULT_PAGE_SIZE
}

function NewContractButton({ testId = 'new-contract' }: { testId?: string }) {
  return (
    <Button asChild>
      <Link to="/contracts/new" data-testid={testId}>
        <Plus aria-hidden="true" /> New contract
      </Link>
    </Button>
  )
}

/** S1: page and page size live in the address, so a refresh or shared link shows the same page (W3). */
export default function ContractsListPage() {
  const [params, setParams] = useSearchParams()
  const page = readPage(params.get('page'))
  const pageSize = readPageSize(params.get('pageSize'))
  const [state, setState] = useState<LoadState>({ kind: 'loading' })
  const [attempt, setAttempt] = useState(0)

  const go = useCallback(
    (nextPage: number, nextSize: number, replace = false) => {
      setParams({ page: String(nextPage), pageSize: String(nextSize) }, { replace })
    },
    [setParams],
  )

  useEffect(() => {
    const controller = new AbortController()
    setState({ kind: 'loading' })
    void listContracts(page, pageSize, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (result.kind !== 'ok') {
        setState({ kind: 'error' })
        return
      }
      const data = result.page
      // A page past the last one: show the last page instead.
      if (data.items.length === 0 && data.totalPages > 0 && page > data.totalPages) {
        go(data.totalPages, pageSize, true)
        return
      }
      setState({ kind: 'loaded', data })
    })
    return () => controller.abort()
  }, [page, pageSize, attempt, go])

  return (
    <main className="mx-auto grid max-w-5xl gap-6 p-6" data-testid="contracts-list-page">
      <div className="flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">Contracts</h1>
        <NewContractButton />
      </div>

      {state.kind === 'loading' && (
        <div className="grid gap-2" aria-busy="true" aria-label="Loading contracts">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      )}

      {state.kind === 'error' && (
        <Alert variant="destructive" role="alert" className="flex items-center justify-between">
          <AlertDescription>{CONTRACTS_LOAD_FAILED_MESSAGE}</AlertDescription>
          <Button type="button" variant="outline" onClick={() => setAttempt((n) => n + 1)} data-testid="retry">
            Retry
          </Button>
        </Alert>
      )}

      {state.kind === 'loaded' && state.data.totalCount === 0 && (
        <div className="grid justify-items-start gap-3" data-testid="contracts-empty">
          <p>{NO_CONTRACTS_MESSAGE}</p>
          <NewContractButton testId="new-contract-empty" />
        </div>
      )}

      {state.kind === 'loaded' && state.data.totalCount > 0 && (
        <>
          <ContractsTable items={state.data.items} />
          <Pager
            page={state.data.page}
            pageSize={pageSize}
            totalCount={state.data.totalCount}
            totalPages={state.data.totalPages}
            onPageChange={(next) => go(next, pageSize)}
            onPageSizeChange={(size) => go(1, size)}
          />
        </>
      )}
    </main>
  )
}
