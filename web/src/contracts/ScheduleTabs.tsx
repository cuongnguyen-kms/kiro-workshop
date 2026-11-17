import { useMemo } from 'react'
import type { ExpenseEntry, ExpenseMethod, PaymentEntry, PaymentStream, Schedule } from '@/api/types'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { ScheduleTable, type ScheduleColumn, type ScheduleRow } from '@/components/ScheduleTable'
import { FREQUENCY_LABELS, formatDate, formatMoney, formatMonth } from '@/contracts/display'

interface ScheduleTabsProps {
  paymentSchedule: Schedule<PaymentEntry>
  expenseSchedule: Schedule<ExpenseEntry>
  expenseMethod: ExpenseMethod
  streams: PaymentStream[]
}

export const NO_EXPENSE_MESSAGE = 'This contract has no expense schedule.'

const PAYMENT_COLUMNS: ScheduleColumn[] = [
  { key: 'dueDate', label: 'Due date' },
  { key: 'stream', label: 'Stream' },
  { key: 'amount', label: 'Amount', align: 'right' },
]
const OVERTIME_COLUMNS: ScheduleColumn[] = [
  { key: 'month', label: 'Month' },
  { key: 'amount', label: 'Amount', align: 'right' },
]
const ALL_AT_ONCE_COLUMNS: ScheduleColumn[] = [
  { key: 'date', label: 'Date' },
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
  const expense = useMemo<ScheduleRow[]>(
    () =>
      expenseSchedule.entries.map((e): ScheduleRow =>
        expenseMethod === 'Overtime'
          ? { month: formatMonth(e.periodStart), amount: formatMoney(e.amount) }
          : { date: formatDate(e.periodStart), amount: formatMoney(e.amount) },
      ),
    [expenseSchedule, expenseMethod],
  )

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
            columns={expenseMethod === 'Overtime' ? OVERTIME_COLUMNS : ALL_AT_ONCE_COLUMNS}
            rows={expense}
            total={formatMoney(expenseSchedule.total)}
          />
        )}
      </TabsContent>
    </Tabs>
  )
}
