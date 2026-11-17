// S3 Contract detail: US6.1, US6.2 and the None expense message (unit-test-instructions.md).
import { screen, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import * as fake from '@/test/fakeApi'
import { renderAt } from '@/test/render'

vi.mock('@/api/contracts', () => import('@/test/fakeApi'))

beforeEach(() => fake.resetFakeApi())

describe('Contract detail', () => {
  it('shows name, dates, streams, method and both schedules with totals (AC6.2.1)', async () => {
    const { user } = renderAt(`/contracts/${fake.CONTRACT_ID}`)
    expect(await screen.findByTestId('contract-name')).toHaveTextContent('Office lease HQ')
    expect(screen.getByTestId('contract-period')).toHaveTextContent('Period: 2027-01-01 to 2027-12-31')
    expect(screen.getByTestId('contract-expense-method')).toHaveTextContent('Expense method: All at once')

    const streams = screen.getByTestId('streams-summary')
    expect(within(streams).getByText('Monthly')).toBeInTheDocument()
    expect(within(streams).getByText('1000.00')).toBeInTheDocument()
    expect(within(streams).getByText('2027-01-15 to 2027-12-15')).toBeInTheDocument()

    // The Payment schedule opens first.
    expect(screen.getByTestId('tab-payment')).toHaveAttribute('aria-selected', 'true')
    const payment = screen.getByTestId('schedule-table-payment')
    expect(within(payment).getAllByRole('columnheader').map((h) => h.textContent)).toEqual([
      'Due date',
      'Stream',
      'Amount',
    ])
    const bodyRows = payment.querySelectorAll('tbody tr')
    expect(bodyRows).toHaveLength(12)
    expect(bodyRows[0]).toHaveTextContent('2027-01-15Monthly 1000.001000.00')
    expect(screen.getByTestId('schedule-total-payment')).toHaveTextContent('12000.00')

    await user.click(screen.getByTestId('tab-expense'))
    const expense = await screen.findByTestId('schedule-table-expense')
    expect(within(expense).getAllByRole('columnheader').map((h) => h.textContent)).toEqual(['Date', 'Amount'])
    expect(expense.querySelectorAll('tbody tr')).toHaveLength(1)
    expect(expense.querySelector('tbody tr')).toHaveTextContent('2027-03-0112000.00')
    expect(screen.getByTestId('schedule-total-expense')).toHaveTextContent('12000.00')
  })

  it('shows "Contract not found." for an unknown contract (AC6.2.2)', async () => {
    renderAt('/contracts/00000000-0000-4000-8000-000000000999')
    expect(await screen.findByText('Contract not found.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back to contracts' })).toHaveAttribute('href', '/contracts')
  })

  it('shows "This contract has no expense schedule." for the None method (AC5.1.5 on screen)', async () => {
    fake.getContract.mockResolvedValue({
      kind: 'ok',
      contract: fake.sampleContract({
        expense: { method: 'None' },
        expenseSchedule: { entries: [], total: 0 },
      }),
    })
    const { user } = renderAt(`/contracts/${fake.CONTRACT_ID}`)
    await screen.findByTestId('contract-name')
    await user.click(screen.getByTestId('tab-expense'))
    expect(await screen.findByText('This contract has no expense schedule.')).toBeInTheDocument()
    expect(screen.queryByTestId('schedule-table-expense')).not.toBeInTheDocument()
  })
})
