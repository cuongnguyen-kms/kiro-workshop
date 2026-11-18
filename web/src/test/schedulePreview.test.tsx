// Schedule preview on S2 New contract: FR1 to FR3, NFR4 (unit-test-instructions.md).
import { screen, waitFor, within } from '@testing-library/react'
import type { UserEvent } from '@testing-library/user-event'
import { axe } from 'vitest-axe'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { PreviewScheduleResult } from '@/api/types'
import * as fake from '@/test/fakeApi'
import { renderAt, seriousViolations } from '@/test/render'

vi.mock('@/api/contracts', () => import('@/test/fakeApi'))

beforeEach(() => fake.resetFakeApi())

const input = (name: string) => screen.getByTestId(`${name}-input`) as HTMLInputElement

async function openForm() {
  const view = renderAt('/contracts/new')
  await screen.findByTestId('contract-form')
  return view
}

async function typeInto(user: UserEvent, name: string, text: string) {
  const el = input(name)
  await user.clear(el)
  await user.type(el, text)
  await user.tab()
}

/** Contract dates and one Monthly stream. No name: a preview does not need it. */
async function fillStream(user: UserEvent) {
  await typeInto(user, 'startDate', '2027-01-01')
  await typeInto(user, 'endDate', '2027-12-31')
  await typeInto(user, 'paymentStreams.0.amount', '1000')
  await user.click(screen.getByTestId('paymentStreams.0.frequency-input'))
  await user.click(await screen.findByRole('option', { name: 'Monthly' }))
  await typeInto(user, 'paymentStreams.0.startDate', '2027-01-15')
  await typeInto(user, 'paymentStreams.0.endDate', '2027-03-15')
}

const twoStreams: PreviewScheduleResult = {
  kind: 'ok',
  preview: {
    paymentSchedule: {
      entries: [
        { streamNumber: 1, dueDate: '2027-01-15', amount: 1000 },
        { streamNumber: 2, dueDate: '2027-02-01', amount: 250.5 },
      ],
      total: 1250.5,
    },
    expenseSchedule: { entries: [], total: 0, balanceRows: [] },
  },
}

describe('Schedule preview buttons', () => {
  it('payment button is disabled until dates and streams are valid, and needs no name (FR1.2, FR2.4, FR1.6)', async () => {
    const { user } = await openForm()
    expect(screen.getByTestId('preview-payments')).toBeDisabled()
    expect(screen.getByTestId('preview-payments')).toHaveAttribute('aria-describedby', 'preview-payments-reason')

    await fillStream(user)
    expect(input('name').value).toBe('')
    await waitFor(() => expect(screen.getByTestId('preview-payments')).toBeEnabled())
  })

  it('expense button needs a method and valid dates; None keeps it disabled with the hint (FR1.3, FR1.4, FR1.5)', async () => {
    const { user } = await openForm()
    await fillStream(user)
    expect(screen.getByTestId('preview-expense')).toBeDisabled()
    expect(screen.getByTestId('preview-expense-reason')).toHaveTextContent('This contract has no expense schedule.')

    await user.click(screen.getByTestId('expense-method-AllAtOnce'))
    expect(screen.getByTestId('preview-expense')).toBeDisabled()
    expect(screen.getByTestId('preview-expense-reason')).not.toHaveTextContent('no expense schedule')

    await typeInto(user, 'expense.date', '2027-03-01')
    await waitFor(() => expect(screen.getByTestId('preview-expense')).toBeEnabled())
  })
})

describe('Schedule preview popups', () => {
  it('payment popup shows loading, then Stream rows and the total (FR3.1, FR3.3, FR3.4)', async () => {
    const { user } = await openForm()
    await fillStream(user)
    let release: (r: PreviewScheduleResult) => void = () => {}
    fake.previewSchedule.mockImplementationOnce(() => new Promise((resolve) => (release = resolve)))

    await user.click(screen.getByTestId('preview-payments'))
    expect(await screen.findByTestId('preview-loading')).toBeInTheDocument()
    release(twoStreams)

    const table = await screen.findByTestId('schedule-table-preview-payment')
    expect(within(table).getByText('Stream 1')).toBeInTheDocument()
    expect(within(table).getByText('Stream 2')).toBeInTheDocument()
    expect(screen.getByTestId('schedule-total-preview-payment')).toHaveTextContent('1,250.50')
    expect(fake.previewSchedule.mock.calls[0][1]).toBe('payment')
  })

  it('expense popup shows the balance table from balanceRows with no total row (FR3.2, AC2.3.2)', async () => {
    const { user } = await openForm()
    await fillStream(user)
    await user.click(screen.getByTestId('expense-method-Overtime'))
    await typeInto(user, 'expense.startDate', '2027-01-01')
    await typeInto(user, 'expense.endDate', '2027-03-31')
    fake.previewSchedule.mockResolvedValueOnce({
      kind: 'ok',
      preview: {
        paymentSchedule: { entries: [], total: 0 },
        expenseSchedule: {
          entries: [
            { periodStart: '2027-01-01', periodEnd: '2027-01-31', amount: 400 },
            { periodStart: '2027-02-01', periodEnd: '2027-02-28', amount: 350 },
          ],
          total: 750,
          balanceRows: fake.OVERTIME_BALANCE_ROWS,
        },
      },
    })

    await waitFor(() => expect(screen.getByTestId('preview-expense')).toBeEnabled())
    await user.click(screen.getByTestId('preview-expense'))

    const table = await screen.findByTestId('schedule-table-preview-expense')
    expect(within(table).getByText('Jan 2027')).toBeInTheDocument()
    expect(within(table).getByText('Feb 2027')).toBeInTheDocument()
    expect(table.querySelectorAll('tbody tr')).toHaveLength(2)
    expect(table.querySelectorAll('tbody tr')[1]).toHaveTextContent(
      'Feb 20271,000.00(1,000.00)10,000.000.000.0010,000.000.00',
    )
    expect(table.querySelector('tfoot')).toBeNull()
    expect(screen.queryByTestId('schedule-total-preview-expense')).not.toBeInTheDocument()
  })

  it('shows server messages without a table, and a failure offers Retry (FR2.5, FR2.6)', async () => {
    const { user } = await openForm()
    await fillStream(user)
    fake.previewSchedule.mockResolvedValueOnce({
      kind: 'validation',
      errors: { paymentStreams: ['This contract would create more than 10,000 schedule rows.'] },
    })
    await user.click(screen.getByTestId('preview-payments'))
    expect(await screen.findByText('This contract would create more than 10,000 schedule rows.')).toBeInTheDocument()
    expect(screen.queryByTestId('schedule-table-preview-payment')).not.toBeInTheDocument()
    await user.click(screen.getByTestId('preview-close'))

    fake.previewSchedule.mockResolvedValueOnce({ kind: 'failure' })
    await user.click(screen.getByTestId('preview-payments'))
    expect(await screen.findByTestId('preview-failed')).toHaveTextContent('The schedule could not be calculated.')
    const calls = fake.previewSchedule.mock.calls.length

    await user.click(screen.getByTestId('preview-retry'))
    expect(await screen.findByTestId('schedule-table-preview-payment')).toBeInTheDocument()
    expect(fake.previewSchedule.mock.calls.length).toBe(calls + 1)
  })

  it('Escape closes the popup, focus returns, the form is unchanged, and a reopen sends the new values (FR3.5, FR3.6, FR3.7)', async () => {
    const { user } = await openForm()
    await fillStream(user)
    await user.click(screen.getByTestId('preview-payments'))
    await screen.findByTestId('schedule-table-preview-payment')

    await user.keyboard('{Escape}')
    await waitFor(() => expect(screen.queryByTestId('preview-dialog')).not.toBeInTheDocument())
    expect(screen.getByTestId('preview-payments')).toHaveFocus()
    expect(input('paymentStreams.0.amount').value).toBe('1000.00')
    expect(input('startDate').value).toBe('2027-01-01')
    expect(fake.createContract).not.toHaveBeenCalled()

    await typeInto(user, 'paymentStreams.0.amount', '2000')
    await user.click(screen.getByTestId('preview-payments'))
    await screen.findByTestId('schedule-table-preview-payment')
    expect(fake.previewSchedule).toHaveBeenCalledTimes(2)
    expect(fake.previewSchedule.mock.calls[0][0].paymentStreams[0].amount).toBe(1000)
    expect(fake.previewSchedule.mock.calls[1][0].paymentStreams[0].amount).toBe(2000)
    expect('name' in fake.previewSchedule.mock.calls[1][0]).toBe(false)
  })

  it('has no serious or critical axe issues with the popup open (NFR4)', async () => {
    const { user } = await openForm()
    await fillStream(user)
    fake.previewSchedule.mockResolvedValueOnce(twoStreams)
    await user.click(screen.getByTestId('preview-payments'))
    await screen.findByTestId('schedule-table-preview-payment')

    expect(seriousViolations(await axe(document.body))).toEqual([])
  })
})
