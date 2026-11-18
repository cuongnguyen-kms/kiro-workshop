import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Loader2 } from 'lucide-react'
import { previewSchedule } from '@/api/contracts'
import type { PreviewKind, PreviewRequest, PreviewScheduleResult } from '@/api/types'
import { ScheduleTable, type ScheduleColumn, type ScheduleRow } from '@/components/ScheduleTable'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { formatDate, formatMoney } from '@/contracts/display'
import { EXPENSE_BALANCE_COLUMNS, expenseBalanceRows } from '@/contracts/expenseBalance'
import {
  expensePreviewReason,
  paymentPreviewReason,
  toPreviewRequest,
  type ContractFormValues,
} from '@/contracts/validation'

export const PREVIEW_FAILED_MESSAGE = 'The schedule could not be calculated. Try again.'

type PreviewState =
  | { status: 'loading' }
  | { status: 'done'; result: PreviewScheduleResult }

const PAYMENT_COLUMNS: ScheduleColumn[] = [
  { key: 'dueDate', label: 'Due date' },
  { key: 'stream', label: 'Stream' },
  { key: 'amount', label: 'Amount', align: 'right' },
]

interface SchedulePreviewProps {
  /** The live form values; the preview reads them at the moment a button is pressed (FR3.6). */
  values: ContractFormValues
}

/** Two buttons that show the schedules of the values entered so far in a popup. Nothing is stored (FR2.2). */
export function SchedulePreview({ values }: SchedulePreviewProps) {
  const [openKind, setOpenKind] = useState<PreviewKind | null>(null)
  const [state, setState] = useState<PreviewState>({ status: 'loading' })
  const requestRef = useRef<{ request: PreviewRequest; kind: PreviewKind; method: ContractFormValues['expense']['method'] } | null>(null)
  const abortRef = useRef<AbortController | null>(null)
  const buttonRefs = useRef<Record<PreviewKind, HTMLButtonElement | null>>({ payment: null, expense: null })

  const paymentReason = paymentPreviewReason(values)
  const expenseReason = expensePreviewReason(values)

  const load = useCallback(async () => {
    const current = requestRef.current
    if (!current) return
    abortRef.current?.abort()
    const controller = new AbortController()
    abortRef.current = controller
    setState({ status: 'loading' })
    const result = await previewSchedule(current.request, current.kind, controller.signal)
    if (controller.signal.aborted) return
    setState({ status: 'done', result })
  }, [])

  const open = (kind: PreviewKind) => {
    requestRef.current = { request: toPreviewRequest(values, kind), kind, method: values.expense.method }
    setOpenKind(kind)
    void load()
  }

  const close = () => {
    abortRef.current?.abort()
    setOpenKind(null)
  }

  useEffect(() => () => abortRef.current?.abort(), [])

  return (
    <div className="grid gap-1" data-testid="schedule-preview">
      <div className="flex flex-wrap gap-3">
        <Button
          ref={(el) => {
            buttonRefs.current.payment = el
          }}
          type="button"
          variant="outline"
          disabled={paymentReason !== undefined}
          aria-describedby={paymentReason ? 'preview-payments-reason' : undefined}
          onClick={() => open('payment')}
          data-testid="preview-payments"
        >
          Payment schedule
        </Button>
        <Button
          ref={(el) => {
            buttonRefs.current.expense = el
          }}
          type="button"
          variant="outline"
          disabled={expenseReason !== undefined}
          aria-describedby={expenseReason ? 'preview-expense-reason' : undefined}
          onClick={() => open('expense')}
          data-testid="preview-expense"
        >
          Expense schedule
        </Button>
      </div>
      {paymentReason && (
        <p id="preview-payments-reason" className="text-muted-foreground text-sm" data-testid="preview-payments-reason">
          {paymentReason}
        </p>
      )}
      {expenseReason && (
        <p id="preview-expense-reason" className="text-muted-foreground text-sm" data-testid="preview-expense-reason">
          {expenseReason}
        </p>
      )}

      <Dialog open={openKind !== null} onOpenChange={(next) => !next && close()}>
        <DialogContent
          data-testid="preview-dialog"
          className={requestRef.current?.kind === 'expense' ? 'sm:max-w-[80vw]' : undefined}
          onCloseAutoFocus={(event) => {
            const kind = requestRef.current?.kind
            if (kind && buttonRefs.current[kind]) {
              event.preventDefault()
              buttonRefs.current[kind]?.focus()
            }
          }}
        >
          <DialogHeader>
            <DialogTitle>{openKind === 'expense' ? 'Expense schedule preview' : 'Payment schedule preview'}</DialogTitle>
            <DialogDescription>
              A preview of the values entered so far. Nothing is saved.
            </DialogDescription>
          </DialogHeader>
          <PreviewBody
            state={state}
            kind={openKind ?? 'payment'}
            onRetry={() => void load()}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" data-testid="preview-close">
                Close
              </Button>
            </DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

function PreviewBody({
  state,
  kind,
  onRetry,
}: {
  state: PreviewState
  kind: PreviewKind
  onRetry: () => void
}) {
  const result = state.status === 'done' ? state.result : undefined
  const payment = useMemo<ScheduleRow[]>(
    () =>
      result?.kind === 'ok'
        ? result.preview.paymentSchedule.entries.map((e) => ({
            dueDate: formatDate(e.dueDate),
            stream: `Stream ${e.streamNumber}`,
            amount: formatMoney(e.amount),
          }))
        : [],
    [result],
  )
  const balanceRows = result?.kind === 'ok' ? result.preview.expenseSchedule.balanceRows : undefined
  const expense = useMemo<ScheduleRow[]>(() => expenseBalanceRows(balanceRows), [balanceRows])

  if (!result) {
    return (
      <p className="text-muted-foreground flex items-center gap-2 py-4" role="status" data-testid="preview-loading">
        <Loader2 aria-hidden="true" className="animate-spin" /> Loading the preview…
      </p>
    )
  }
  if (result.kind === 'validation') {
    const messages = Object.values(result.errors).flat()
    return (
      <ul className="list-disc pl-5 text-sm" role="alert" data-testid="preview-errors">
        {messages.map((m) => (
          <li key={m}>{m}</li>
        ))}
      </ul>
    )
  }
  if (result.kind === 'failure') {
    return (
      <div className="grid justify-items-start gap-3" role="alert" data-testid="preview-failed">
        <p>{PREVIEW_FAILED_MESSAGE}</p>
        <Button type="button" onClick={onRetry} data-testid="preview-retry">
          Retry
        </Button>
      </div>
    )
  }
  if (kind === 'payment') {
    return (
      <ScheduleTable
        name="preview-payment"
        caption="Payment schedule preview"
        columns={PAYMENT_COLUMNS}
        rows={payment}
        total={formatMoney(result.preview.paymentSchedule.total)}
      />
    )
  }
  return (
    <ScheduleTable
      name="preview-expense"
      caption="Expense schedule preview"
      columns={EXPENSE_BALANCE_COLUMNS}
      rows={expense}
      rowHeader
      stickyFirstColumn
      regionLabel="Expense schedule preview, scrolls sideways"
      maxHeightClass="max-h-[60vh]"
    />
  )
}
