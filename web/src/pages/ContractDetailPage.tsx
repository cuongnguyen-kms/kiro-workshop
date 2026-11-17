import { useCallback, useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { CheckCircle2, X } from 'lucide-react'
import { getContract } from '@/api/contracts'
import type { ContractDetail } from '@/api/types'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ScheduleTabs } from '@/contracts/ScheduleTabs'
import { StreamsSummary } from '@/contracts/StreamsSummary'
import { EXPENSE_METHOD_LABELS, formatDate } from '@/contracts/display'

type LoadState =
  | { kind: 'loading' }
  | { kind: 'loaded'; contract: ContractDetail }
  | { kind: 'notFound' }
  | { kind: 'error' }

export const CONTRACT_SAVED_MESSAGE = 'Contract saved.'
export const CONTRACT_NOT_FOUND_MESSAGE = 'Contract not found.'
export const CONTRACT_LOAD_FAILED_MESSAGE = 'Could not load this contract.'

/** S3: loads one contract and shows it (workflow W2). */
export default function ContractDetailPage() {
  const { id = '' } = useParams()
  const location = useLocation()
  const justSaved = (location.state as { saved?: boolean } | null)?.saved === true
  const [bannerDismissed, setBannerDismissed] = useState(false)
  const [state, setState] = useState<LoadState>({ kind: 'loading' })
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()
    setState({ kind: 'loading' })
    void getContract(id, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (result.kind === 'ok') setState({ kind: 'loaded', contract: result.contract })
      else if (result.kind === 'notFound') setState({ kind: 'notFound' })
      else setState({ kind: 'error' })
    })
    return () => controller.abort()
  }, [id, attempt])

  const retry = useCallback(() => setAttempt((n) => n + 1), [])

  return (
    <main className="mx-auto grid max-w-5xl gap-6 p-6" data-testid="contract-detail-page">
      <nav aria-label="Breadcrumb">
        <Link to="/contracts" className="text-sm underline underline-offset-2">
          &lt; Contracts
        </Link>
      </nav>

      {justSaved && !bannerDismissed && (
        <Alert role="status" className="flex items-center justify-between" data-testid="saved-banner">
          <AlertDescription className="text-foreground flex flex-row items-center gap-2">
            <CheckCircle2 aria-hidden="true" className="size-4" />
            {CONTRACT_SAVED_MESSAGE}
          </AlertDescription>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            aria-label="Dismiss"
            onClick={() => setBannerDismissed(true)}
            data-testid="dismiss-saved-banner"
          >
            <X aria-hidden="true" />
          </Button>
        </Alert>
      )}

      {state.kind === 'loading' && (
        <div className="grid gap-4" aria-busy="true" aria-label="Loading contract">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-5 w-80" />
          <Skeleton className="h-40 w-full" />
        </div>
      )}

      {state.kind === 'notFound' && (
        <div className="grid gap-2">
          <h1 className="text-2xl font-semibold">{CONTRACT_NOT_FOUND_MESSAGE}</h1>
          <Link to="/contracts" className="underline underline-offset-2">
            Back to contracts
          </Link>
        </div>
      )}

      {state.kind === 'error' && (
        <>
          <h1 className="text-2xl font-semibold">Contract</h1>
          <Alert variant="destructive" role="alert" className="flex items-center justify-between">
            <AlertDescription>{CONTRACT_LOAD_FAILED_MESSAGE}</AlertDescription>
            <Button type="button" variant="outline" onClick={retry} data-testid="retry">
              Retry
            </Button>
          </Alert>
        </>
      )}

      {state.kind === 'loaded' && (
        <>
          <header className="grid gap-1">
            <h1 className="text-2xl font-semibold" data-testid="contract-name">
              {state.contract.name}
            </h1>
            <p data-testid="contract-period">
              Period: {formatDate(state.contract.startDate)} to {formatDate(state.contract.endDate)}
            </p>
            <p data-testid="contract-expense-method">
              Expense method: {EXPENSE_METHOD_LABELS[state.contract.expense.method]}
            </p>
          </header>
          <StreamsSummary streams={state.contract.paymentStreams} />
          <ScheduleTabs
            paymentSchedule={state.contract.paymentSchedule}
            expenseSchedule={state.contract.expenseSchedule}
            expenseMethod={state.contract.expense.method}
            streams={state.contract.paymentStreams}
          />
        </>
      )}
    </main>
  )
}
