// NFR5.3: a 5,200-row payment schedule renders in under 1 second. The target is not loosened.
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { PaymentEntry } from '@/api/types'
import { SCHEDULE_PAGE_SIZE } from '@/components/ScheduleTable'
import { ScheduleTabs } from '@/contracts/ScheduleTabs'
import { STREAM_ID } from '@/test/fakeApi'

const ROWS = 5200
const LIMIT_MS = 1000

describe('Schedule rendering performance', () => {
  it('renders a 5,200-row payment schedule in under 1 second (NFR5.3)', () => {
    const entries: PaymentEntry[] = Array.from({ length: ROWS }, (_, i) => ({
      streamId: STREAM_ID,
      dueDate: `2027-01-${String((i % 28) + 1).padStart(2, '0')}`,
      amount: 10,
    }))
    const props = {
      paymentSchedule: { entries, total: 52000 },
      expenseSchedule: { entries: [], total: 0 },
      expenseMethod: 'None' as const,
      streams: [{ id: STREAM_ID, amount: 10, frequency: 'Weekly' as const, startDate: '2027-01-01', endDate: '2127-01-01' }],
    }

    const start = performance.now()
    render(<ScheduleTabs {...props} />)
    const elapsed = performance.now() - start

    // The long schedule is shown a page at a time, with the full total always visible.
    const table = screen.getByTestId('schedule-table-payment')
    expect(table.querySelectorAll('tbody tr')).toHaveLength(SCHEDULE_PAGE_SIZE)
    expect(screen.getByTestId('schedule-pager-payment')).toHaveTextContent(`Rows 1–100 of ${ROWS}`)
    expect(screen.getByTestId('schedule-total-payment')).toHaveTextContent('52000.00')
    console.info(`5,200-row payment schedule rendered in ${elapsed.toFixed(0)} ms`)
    expect(elapsed).toBeLessThan(LIMIT_MS)
  })

  it('pages through a long schedule with Next and Previous (NFR5.3)', async () => {
    const user = userEvent.setup()
    const entries: PaymentEntry[] = Array.from({ length: 250 }, (_, i) => ({
      streamId: STREAM_ID,
      dueDate: '2027-01-01',
      amount: i + 1,
    }))
    render(
      <ScheduleTabs
        paymentSchedule={{ entries, total: 31375 }}
        expenseSchedule={{ entries: [], total: 0 }}
        expenseMethod="None"
        streams={[{ id: STREAM_ID, amount: 1, frequency: 'Weekly', startDate: '2027-01-01', endDate: '2032-01-01' }]}
      />,
    )

    const pager = screen.getByTestId('schedule-pager-payment')
    expect(screen.getByTestId('schedule-prev-payment')).toBeDisabled()
    await user.click(screen.getByTestId('schedule-next-payment'))
    expect(pager).toHaveTextContent('Rows 101–200 of 250')
    await user.click(screen.getByTestId('schedule-next-payment'))
    expect(pager).toHaveTextContent('Rows 201–250 of 250')
    expect(screen.getByTestId('schedule-table-payment').querySelectorAll('tbody tr')).toHaveLength(50)
    expect(screen.getByTestId('schedule-next-payment')).toBeDisabled()
    expect(screen.getByTestId('schedule-total-payment')).toHaveTextContent('31375.00')
  })
})
