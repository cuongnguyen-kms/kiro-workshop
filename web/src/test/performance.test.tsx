// NFR5.3: a 5,200-row payment schedule renders in under 1 second. The target is not loosened.
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { PaymentEntry } from '@/api/types'
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

    const table = screen.getByTestId('schedule-table-payment')
    expect(table.querySelectorAll('tbody tr')).toHaveLength(ROWS)
    expect(screen.getByTestId('schedule-total-payment')).toHaveTextContent('52000.00')
    console.info(`5,200-row payment schedule rendered in ${elapsed.toFixed(0)} ms`)
    expect(elapsed).toBeLessThan(LIMIT_MS)
  })
})
