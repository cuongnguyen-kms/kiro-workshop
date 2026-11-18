// U3: the 8-column expense balance table on the Expense schedule tab and in the preview popup
// (US1.1, US2.1, US2.2, US2.3, US4.1, SEC2, NFR1.1, NFR4.1 to NFR4.3).
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import type { UserEvent } from '@testing-library/user-event'
import { axe } from 'vitest-axe'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { BalanceRow, ExpenseSchedule } from '@/api/types'
import { SCHEDULE_PAGE_SIZE } from '@/components/ScheduleTable'
import { ScheduleTabs } from '@/contracts/ScheduleTabs'
import * as fake from '@/test/fakeApi'
import { renderAt, seriousViolations } from '@/test/render'

vi.mock('@/api/contracts', () => import('@/test/fakeApi'))

beforeEach(() => fake.resetFakeApi())

const HEADERS = [
  'Period',
  'Expense',
  '(Accrual)/Prepaid',
  'Cumulative Balance',
  'ST Accrued Liability',
  'LT Accrued Liability',
  'ST Prepaid Asset',
  'LT Prepaid Asset',
]

const expenseSchedule = (balanceRows: BalanceRow[]): ExpenseSchedule => ({ entries: [], total: 0, balanceRows })

function renderTabs(balanceRows: BalanceRow[] | undefined, method: 'Overtime' | 'AllAtOnce' | 'None' = 'Overtime') {
  const schedule = balanceRows === undefined ? ({ entries: [], total: 0 } as unknown as ExpenseSchedule) : expenseSchedule(balanceRows)
  return render(
    <ScheduleTabs
      paymentSchedule={{ entries: [], total: 0 }}
      expenseSchedule={schedule}
      expenseMethod={method}
      streams={[]}
    />,
  )
}

/** Opens the Expense tab. Radix tabs activate on mouse down. */
function openExpenseTab() {
  fireEvent.mouseDown(screen.getByTestId('tab-expense'), { button: 0 })
  return screen.getByTestId('schedule-table-expense')
}

const headerTexts = (table: HTMLElement) => within(table).getAllByRole('columnheader').map((h) => h.textContent)
const bodyText = (table: HTMLElement) => Array.from(table.querySelectorAll('tbody tr')).map((r) => r.textContent)

describe('Expense schedule tab', () => {
  it('shows the 8 headers in order and one row per balance row with formatted values (AC1.1.1 to AC2.2.6, NFR3.2)', () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const table = openExpenseTab()
    expect(headerTexts(table)).toEqual(HEADERS)
    expect(bodyText(table)).toEqual([
      'Jan 20271,000.0011,000.0011,000.000.000.0011,000.000.00',
      'Feb 20271,000.00(1,000.00)10,000.000.000.0010,000.000.00',
    ])
  })

  it('shows signed values with brackets and zero as 0.00 (AC3.1.1, AC3.1.3)', () => {
    renderTabs([
      {
        periodStart: '2027-12-01',
        periodEnd: '2027-12-31',
        expense: 0,
        accrualPrepaid: -1234.5,
        cumulativeBalance: -1234.5,
        stAccruedLiability: 1234.5,
        ltAccruedLiability: 0,
        stPrepaidAsset: 0,
        ltPrepaidAsset: 0,
      },
    ])
    const table = openExpenseTab()
    expect(bodyText(table)).toEqual(['Dec 20270.00(1,234.50)(1,234.50)1,234.500.000.000.00'])
  })

  it('has no total row and no total test id (AC2.3.1)', () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const table = openExpenseTab()
    expect(table.querySelector('tfoot')).toBeNull()
    expect(screen.queryByTestId('schedule-total-expense')).not.toBeInTheDocument()
  })

  it('uses the Period cell as the row header of each row (AC4.1.3)', () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const table = openExpenseTab()
    const rowHeaders = within(table).getAllByRole('rowheader')
    expect(rowHeaders.map((h) => h.textContent)).toEqual(['Jan 2027', 'Feb 2027'])
    expect(rowHeaders.every((h) => h.getAttribute('scope') === 'row')).toBe(true)
    expect(table.querySelectorAll('tbody td')).toHaveLength(14)
  })

  it('scrolls in a focusable, named region that differs from the caption (NFR4.2)', () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const table = openExpenseTab()
    const region = screen.getByRole('region', { name: 'Expense schedule, scrolls sideways' })
    expect(region).toHaveAttribute('tabindex', '0')
    expect(region).toContainElement(table)
    expect(within(region).getByRole('table', { name: 'Expense schedule' })).toBe(table)
  })

  it('has the sticky header, sticky first column, nowrap headers and a height limit (NFR4.3)', () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const table = openExpenseTab()
    const region = screen.getByRole('region', { name: 'Expense schedule, scrolls sideways' })
    expect(region).toHaveClass('overflow-auto', 'max-h-[70vh]')
    expect(table).toHaveClass('[&_tbody_th]:sticky', '[&_tbody_th]:left-0', '[&_tbody_th]:bg-background')
    const headers = within(table).getAllByRole('columnheader')
    for (const h of headers) expect(h).toHaveClass('sticky', 'top-0', 'whitespace-nowrap')
    expect(headers[0]).toHaveClass('left-0', 'z-30')
    expect(headers[1]).toHaveClass('z-20')
    for (const h of headers.slice(1)) expect(h).not.toHaveClass('left-0')
  })

  it('aligns the amount columns to the right through table-level classes (NFR1.2)', () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const table = openExpenseTab()
    expect(table).toHaveClass('[&_tbody_td]:text-right', '[&_tbody_td]:tabular-nums')
    expect(table.querySelector('tbody td')).not.toHaveAttribute('class')
    expect(table.querySelector('tbody td')).not.toHaveAttribute('style')
  })

  it('shows the headers and no rows when balanceRows is missing or empty (SEC2)', () => {
    renderTabs(undefined)
    let table = openExpenseTab()
    expect(headerTexts(table)).toEqual(HEADERS)
    expect(table.querySelectorAll('tbody tr')).toHaveLength(0)
    cleanup()

    renderTabs([])
    table = openExpenseTab()
    expect(headerTexts(table)).toEqual(HEADERS)
    expect(table.querySelectorAll('tbody tr')).toHaveLength(0)
  })

  it('shows the message and no table for method None (AC1.1.4)', () => {
    renderTabs([], 'None')
    fireEvent.mouseDown(screen.getByTestId('tab-expense'), { button: 0 })
    expect(screen.getByText('This contract has no expense schedule.')).toBeInTheDocument()
    expect(screen.queryByTestId('schedule-table-expense')).not.toBeInTheDocument()
  })

  it('has no serious or critical axe issues (NFR4.1)', async () => {
    const { container } = renderTabs(fake.OVERTIME_BALANCE_ROWS)
    openExpenseTab()
    expect(seriousViolations(await axe(container))).toEqual([])
  })

  it('renders 120 rows, first page shown, in under 200 ms in jsdom (NFR1.1)', () => {
    const rows: BalanceRow[] = Array.from({ length: 120 }, (_, i) => ({
      periodStart: `${2027 + Math.floor(i / 12)}-${String((i % 12) + 1).padStart(2, '0')}-01`,
      periodEnd: `${2027 + Math.floor(i / 12)}-${String((i % 12) + 1).padStart(2, '0')}-28`,
      expense: 1000,
      accrualPrepaid: i % 2 === 0 ? 1234.56 : -1234.56,
      cumulativeBalance: 100000 - i * 100,
      stAccruedLiability: 0,
      ltAccruedLiability: 0,
      stPrepaidAsset: 1000,
      ltPrepaidAsset: 2000,
    }))
    renderTabs(rows)
    const start = performance.now()
    const table = openExpenseTab()
    const elapsed = performance.now() - start
    expect(table.querySelectorAll('tbody tr')).toHaveLength(SCHEDULE_PAGE_SIZE)
    expect(screen.getByTestId('schedule-pager-expense')).toHaveTextContent('Rows 1–100 of 120')
    expect(elapsed).toBeLessThan(200)
  })
})

describe('Expense schedule preview popup', () => {
  async function openForm() {
    const view = renderAt('/contracts/new')
    await screen.findByTestId('contract-form')
    return view
  }

  async function typeInto(user: UserEvent, name: string, text: string) {
    const el = screen.getByTestId(`${name}-input`) as HTMLInputElement
    await user.clear(el)
    await user.type(el, text)
    await user.tab()
  }

  async function fillOvertime(user: UserEvent) {
    await typeInto(user, 'startDate', '2027-01-01')
    await typeInto(user, 'endDate', '2027-12-31')
    await typeInto(user, 'paymentStreams.0.amount', '1000')
    await user.click(screen.getByTestId('paymentStreams.0.frequency-input'))
    await user.click(await screen.findByRole('option', { name: 'Monthly' }))
    await typeInto(user, 'paymentStreams.0.startDate', '2027-01-15')
    await typeInto(user, 'paymentStreams.0.endDate', '2027-03-15')
    await user.click(screen.getByTestId('expense-method-Overtime'))
    await typeInto(user, 'expense.startDate', '2027-01-01')
    await typeInto(user, 'expense.endDate', '2027-03-31')
    await waitFor(() => expect(screen.getByTestId('preview-expense')).toBeEnabled())
  }

  function popupWith(rows: BalanceRow[]) {
    fake.previewSchedule.mockResolvedValueOnce({
      kind: 'ok',
      preview: { paymentSchedule: { entries: [], total: 0 }, expenseSchedule: expenseSchedule(rows) },
    })
  }

  it('shows the same table and values as the tab for the same data, with no total row (AC2.1.4, AC2.3.2)', async () => {
    renderTabs(fake.OVERTIME_BALANCE_ROWS)
    const tabTable = openExpenseTab()
    const tabHeaders = headerTexts(tabTable)
    const tabRows = bodyText(tabTable)
    cleanup()

    const { user } = await openForm()
    await fillOvertime(user)
    popupWith(fake.OVERTIME_BALANCE_ROWS)
    await user.click(screen.getByTestId('preview-expense'))

    const table = await screen.findByTestId('schedule-table-preview-expense')
    expect(headerTexts(table)).toEqual(tabHeaders)
    expect(bodyText(table)).toEqual(tabRows)
    expect(table.querySelector('tfoot')).toBeNull()
    expect(screen.queryByTestId('schedule-total-preview-expense')).not.toBeInTheDocument()
    const region = screen.getByRole('region', { name: 'Expense schedule preview, scrolls sideways' })
    expect(region).toHaveClass('max-h-[60vh]')
    expect(within(table).getAllByRole('rowheader')).toHaveLength(2)
  })

  it('opens up to 80% wide for the expense popup and keeps the default width for the payment popup (M2)', async () => {
    const { user } = await openForm()
    await fillOvertime(user)
    popupWith(fake.OVERTIME_BALANCE_ROWS)
    await user.click(screen.getByTestId('preview-expense'))
    await screen.findByTestId('schedule-table-preview-expense')
    expect(screen.getByTestId('preview-dialog')).toHaveClass('sm:max-w-[80vw]')
    await user.click(screen.getByTestId('preview-close'))
    await waitFor(() => expect(screen.queryByTestId('preview-dialog')).not.toBeInTheDocument())

    await user.click(screen.getByTestId('preview-payments'))
    await screen.findByTestId('schedule-table-preview-payment')
    expect(screen.getByTestId('preview-dialog')).not.toHaveClass('sm:max-w-[80vw]')
  })

  it('shows the headers and no rows when the response has no balanceRows (SEC2)', async () => {
    const { user } = await openForm()
    await fillOvertime(user)
    fake.previewSchedule.mockResolvedValueOnce({
      kind: 'ok',
      preview: {
        paymentSchedule: { entries: [], total: 0 },
        expenseSchedule: { entries: [], total: 0 } as unknown as ExpenseSchedule,
      },
    })
    await user.click(screen.getByTestId('preview-expense'))
    const table = await screen.findByTestId('schedule-table-preview-expense')
    expect(headerTexts(table)).toEqual(HEADERS)
    expect(table.querySelectorAll('tbody tr')).toHaveLength(0)
  })

  it('has no serious or critical axe issues with the popup open (NFR4.1)', async () => {
    const { user } = await openForm()
    await fillOvertime(user)
    popupWith(fake.OVERTIME_BALANCE_ROWS)
    await user.click(screen.getByTestId('preview-expense'))
    await screen.findByTestId('schedule-table-preview-expense')
    expect(seriousViolations(await axe(document.body))).toEqual([])
  })
})
