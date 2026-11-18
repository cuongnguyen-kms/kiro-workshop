import { useMemo } from 'react'
import type { ExpenseMethod, ExpenseSchedule, PaymentEntry, PaymentStream, Schedule } from '@/api/types'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { ScheduleTable, type ScheduleColumn, type ScheduleRow } from '@/components/ScheduleTable'
import { FREQUENCY_LABELS, formatDate, formatMoney } from '@/contracts/display'
import { EXPENSE_BALANCE_COLUMNS, expenseBalanceRows } from '@/contracts/expenseBalance'

interface ScheduleTabsProps {
  paymentSchedule: Schedule<PaymentEntry>
  expenseSchedule: ExpenseSchedule
  expenseMethod: ExpenseMethod
  streams: PaymentStream[]
}

export const NO_EXPENSE_MESSAGE = 'This contract has no expense schedule.'

const PAYMENT_COLUMNS: ScheduleColumn[] = [
  { key: 'dueDate', label: 'Due date' },
  { key: 'stream', label: 'Stream' },
  { key: 'amount', label: 'Amount', align: 'right' },
]

/** Builds display rows once per data change (NFR5.3). Streams have no names, so the label is frequency and amount. */
export function paymentRows(entries: PaymentEntry[], streams: PaymentStream[]): ScheduleRow[] {
  const labels = new Map(streams.map((s) => [s.id, `${FREQUENCY_LABELS[s.frequency]} ${formatMoney(s.amount)}`]))
  return entries.map((e) => ({
    dueDate: formatDate(e.dueDate),
    stream: labels.get(e.streamId) ?? '',
    amount: formatMoney(e.amount),
  }))
}

/** Payment schedule and Expense schedule tabs; Payment opens first. */
export function ScheduleTabs({ paymentSchedule, expenseSchedule, expenseMethod, streams }: ScheduleTabsProps) {
  const payment = useMemo(() => paymentRows(paymentSchedule.entries, streams), [paymentSchedule, streams])
  const expense = useMemo(() => expenseBalanceRows(expenseSchedule.balanceRows), [expenseSchedule.balanceRows])

  return (
    <Tabs defaultValue="payment" className="gap-4" data-testid="schedule-tabs">
      <TabsList>
        <TabsTrigger value="payment" data-testid="tab-payment">
          Payment schedule
        </TabsTrigger>
        <TabsTrigger value="expense" data-testid="tab-expense">
          Expense schedule
        </TabsTrigger>
      </TabsList>
      <TabsContent value="payment">
        <ScheduleTable
          name="payment"
          caption="Payment schedule"
          columns={PAYMENT_COLUMNS}
          rows={payment}
          total={formatMoney(paymentSchedule.total)}
        />
      </TabsContent>
      <TabsContent value="expense">
        {expenseMethod === 'None' ? (
          <p className="text-muted-foreground py-4" data-testid="no-expense-schedule">
            {NO_EXPENSE_MESSAGE}
          </p>
        ) : (
          <ScheduleTable
            name="expense"
            caption="Expense schedule"
            columns={EXPENSE_BALANCE_COLUMNS}
            rows={expense}
            rowHeader
            stickyFirstColumn
            regionLabel="Expense schedule, scrolls sideways"
            maxHeightClass="max-h-[70vh]"
          />
        )}
      </TabsContent>
    </Tabs>
  )
}
