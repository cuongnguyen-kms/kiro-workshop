// S2 New contract: US1.1, US2.1, US3.1, US6.1 (unit-test-instructions.md).
import { screen, waitFor, within } from '@testing-library/react'
import type { UserEvent } from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { CreateContractResult } from '@/api/types'
import * as fake from '@/test/fakeApi'
import { renderAt } from '@/test/render'

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

async function chooseFrequency(user: UserEvent, index: number, label: string) {
  await user.click(screen.getByTestId(`paymentStreams.${index}.frequency-input`))
  await user.click(await screen.findByRole('option', { name: label }))
}

async function fillValidContract(user: UserEvent, frequency = 'Monthly') {
  await typeInto(user, 'name', 'Office lease HQ')
  await typeInto(user, 'startDate', '2027-01-01')
  await typeInto(user, 'endDate', '2027-12-31')
  await typeInto(user, 'paymentStreams.0.amount', '1000')
  await chooseFrequency(user, 0, frequency)
  await typeInto(user, 'paymentStreams.0.startDate', '2027-01-15')
  if (frequency !== 'One-time') await typeInto(user, 'paymentStreams.0.endDate', '2027-12-15')
  await user.click(screen.getByTestId('expense-method-AllAtOnce'))
  await typeInto(user, 'expense.date', '2027-03-01')
}

describe('New contract form', () => {
  it('starts empty with one stream and None selected (AC1.1.1)', async () => {
    await openForm()
    expect(input('name').value).toBe('')
    expect(input('startDate').value).toBe('')
    expect(input('endDate').value).toBe('')
    expect(screen.getByTestId('stream-card-0')).toBeInTheDocument()
    expect(screen.queryByTestId('stream-card-1')).not.toBeInTheDocument()
    expect(input('paymentStreams.0.amount').value).toBe('')
    expect(screen.getByRole('radio', { name: 'None' })).toBeChecked()
  })

  it('shows "Contract name is required." for a blank name (AC1.1.2)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'name', '   ')
    expect(await screen.findByText('Contract name is required.')).toBeInTheDocument()
    expect(input('name')).toHaveAttribute('aria-invalid', 'true')
  })

  it('shows the end date message when the end is before the start (AC1.1.3)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'startDate', '2027-06-01')
    await typeInto(user, 'endDate', '2027-01-01')
    expect(await screen.findByText('End date must not be before the start date.')).toBeInTheDocument()
  })

  it('adds a stream card and focuses its Amount (AC2.1.1)', async () => {
    const { user } = await openForm()
    await user.click(screen.getByTestId('add-stream'))
    expect(screen.getByTestId('stream-card-1')).toBeInTheDocument()
    expect(within(screen.getByTestId('stream-card-1')).getByText('Payment stream 2')).toBeInTheDocument()
    await waitFor(() => expect(input('paymentStreams.1.amount')).toHaveFocus())
  })

  it('removes a stream card, and Remove is disabled on the last one (AC2.1.2)', async () => {
    const { user } = await openForm()
    expect(screen.getByTestId('remove-stream-0')).toBeDisabled()
    await user.click(screen.getByTestId('add-stream'))
    expect(screen.getByTestId('remove-stream-0')).toBeEnabled()
    await user.click(screen.getByTestId('remove-stream-1'))
    expect(screen.queryByTestId('stream-card-1')).not.toBeInTheDocument()
    expect(screen.getByTestId('remove-stream-0')).toBeDisabled()
    expect(screen.getByTestId('add-stream')).toHaveFocus()
  })

  it('shows "Amount must be greater than zero." for a zero amount (AC2.1.3)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'paymentStreams.0.amount', '0')
    expect(await screen.findByText('Amount must be greater than zero.')).toBeInTheDocument()
  })

  it('shows the stream dates message for a stream outside the contract (AC2.1.4)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'startDate', '2027-01-01')
    await typeInto(user, 'endDate', '2027-12-31')
    await typeInto(user, 'paymentStreams.0.startDate', '2026-12-01')
    expect(await screen.findByText('Stream dates must be within the contract dates.')).toBeInTheDocument()
  })

  it('hides the end date for One-time and sends the start date as the end date (AC2.1.5)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'paymentStreams.0.endDate', '2027-12-15')
    await chooseFrequency(user, 0, 'One-time')
    expect(screen.queryByTestId('paymentStreams.0.endDate-input')).not.toBeInTheDocument()
    expect(screen.getByLabelText('Payment date *')).toBeInTheDocument()

    await fillValidContract(user, 'One-time')
    await user.click(screen.getByTestId('save-contract'))
    await waitFor(() => expect(fake.createContract).toHaveBeenCalledTimes(1))
    const request = fake.createContract.mock.calls[0][0]
    expect(request.paymentStreams[0]).toEqual({
      amount: 1000,
      frequency: 'OneTime',
      startDate: '2027-01-15',
      endDate: '2027-01-15',
    })
  })

  it('offers exactly Weekly, Monthly, Quarterly, Annually and One-time (AC2.1.6)', async () => {
    const { user } = await openForm()
    await user.click(screen.getByTestId('paymentStreams.0.frequency-input'))
    const options = await screen.findAllByRole('option')
    expect(options.map((o) => o.textContent)).toEqual(['Weekly', 'Monthly', 'Quarterly', 'Annually', 'One-time'])
  })

  it('shows two dates for Overtime, one for All at once, none for None (AC3.1.1, AC3.1.2, AC3.1.3)', async () => {
    const { user } = await openForm()
    const present = (name: string) => screen.queryByTestId(`${name}-input`) !== null

    await user.click(screen.getByTestId('expense-method-Overtime'))
    expect([present('expense.startDate'), present('expense.endDate'), present('expense.date')]).toEqual([
      true,
      true,
      false,
    ])

    await typeInto(user, 'expense.startDate', '2027-02-01')
    await user.click(screen.getByTestId('expense-method-AllAtOnce'))
    expect([present('expense.startDate'), present('expense.endDate'), present('expense.date')]).toEqual([
      false,
      false,
      true,
    ])

    await user.click(screen.getByTestId('expense-method-None'))
    expect([present('expense.startDate'), present('expense.endDate'), present('expense.date')]).toEqual([
      false,
      false,
      false,
    ])
    expect(screen.getByText('No expense schedule will be created.')).toBeInTheDocument()

    // A hidden value is cleared, so it is never sent.
    await user.click(screen.getByTestId('expense-method-Overtime'))
    expect(input('expense.startDate').value).toBe('')
  })

  it('shows the expense dates message for an expense date outside the contract (AC3.1.4)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'startDate', '2027-01-01')
    await typeInto(user, 'endDate', '2027-12-31')
    await user.click(screen.getByTestId('expense-method-AllAtOnce'))
    await typeInto(user, 'expense.date', '2028-01-01')
    expect(await screen.findByText('Expense dates must be within the contract dates.')).toBeInTheDocument()
  })

  it('shows the Overtime end date message when it is before its start (AC3.1.5)', async () => {
    const { user } = await openForm()
    await typeInto(user, 'startDate', '2027-01-01')
    await typeInto(user, 'endDate', '2027-12-31')
    await user.click(screen.getByTestId('expense-method-Overtime'))
    await typeInto(user, 'expense.startDate', '2027-05-01')
    await typeInto(user, 'expense.endDate', '2027-04-01')
    expect(await screen.findByText('Expense end date must not be before the start date.')).toBeInTheDocument()
  })

  it('sends one POST, shows saving, then opens the detail with "Contract saved." (AC6.1.1, NFR5.2, NFR5.4)', async () => {
    let finish: (result: CreateContractResult) => void = () => {}
    fake.createContract.mockImplementation(
      () =>
        new Promise<CreateContractResult>((resolve) => {
          finish = resolve
        }),
    )
    const { user } = await openForm()
    await fillValidContract(user)
    await user.click(screen.getByTestId('save-contract'))

    const save = screen.getByTestId('save-contract')
    expect(save).toBeDisabled()
    expect(save).toHaveTextContent('Saving')
    expect(input('name')).toHaveAttribute('readonly')
    expect(fake.createContract).toHaveBeenCalledTimes(1)
    expect(fake.createContract.mock.calls[0][0]).toEqual({
      name: 'Office lease HQ',
      startDate: '2027-01-01',
      endDate: '2027-12-31',
      paymentStreams: [{ amount: 1000, frequency: 'Monthly', startDate: '2027-01-15', endDate: '2027-12-15' }],
      expense: { method: 'AllAtOnce', date: '2027-03-01' },
    })

    finish({ kind: 'ok', contract: fake.contractFromRequest(fake.createContract.mock.calls[0][0]) })
    expect(await screen.findByTestId('contract-detail-page')).toBeInTheDocument()
    expect(within(screen.getByRole('status')).getByText('Contract saved.')).toBeInTheDocument()
    expect(fake.createContract).toHaveBeenCalledTimes(1)
  })

  it('keeps entries, shows the not-saved alert and does not resend on failure (AC6.1.2, NFR5.4)', async () => {
    fake.createContract.mockResolvedValue({ kind: 'failure' })
    const { user } = await openForm()
    await fillValidContract(user)
    await user.click(screen.getByTestId('save-contract'))

    expect(
      await screen.findByText('The contract was not saved. Nothing was stored. Try again.'),
    ).toBeInTheDocument()
    expect(screen.getByTestId('save-failed-alert')).toHaveAttribute('role', 'alert')
    expect(input('name').value).toBe('Office lease HQ')
    expect(input('paymentStreams.0.amount').value).toBe('1000.00')
    expect(input('expense.date').value).toBe('2027-03-01')
    expect(screen.getByTestId('save-contract')).toBeEnabled()
    await new Promise((r) => setTimeout(r, 50))
    expect(fake.createContract).toHaveBeenCalledTimes(1)
  })

  it('shows API 400 errors under the matching fields and in the summary (NFR6.3)', async () => {
    fake.createContract.mockResolvedValue(
      fake.validationResult({
        name: ['Contract name is required.'],
        'paymentStreams[0].amount': ['Amount must be greater than zero.'],
        contract: ['A contract with this name already exists.'],
      }),
    )
    const { user } = await openForm()
    await fillValidContract(user)
    await user.click(screen.getByTestId('save-contract'))

    const summary = await screen.findByTestId('error-summary')
    await waitFor(() => expect(summary).toHaveFocus())
    expect(within(summary).getByText('Contract name is required.')).toBeInTheDocument()
    expect(within(summary).getByText('Amount must be greater than zero.')).toBeInTheDocument()
    expect(within(summary).getByText('A contract with this name already exists.')).toBeInTheDocument()

    expect(document.getElementById('field-name-error')).toHaveTextContent('Contract name is required.')
    expect(document.getElementById('field-paymentStreams-0-amount-error')).toHaveTextContent(
      'Amount must be greater than zero.',
    )
    expect(input('name')).toHaveAttribute('aria-describedby', 'field-name-error')
  })
})
