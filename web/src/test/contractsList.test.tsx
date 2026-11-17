// S1 Contracts list: US6.3 and safe text (unit-test-instructions.md).
import { screen, waitFor, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import * as fake from '@/test/fakeApi'
import { renderAt } from '@/test/render'

vi.mock('@/api/contracts', () => import('@/test/fakeApi'))

beforeEach(() => fake.resetFakeApi())

describe('Contracts list', () => {
  it('shows name, start and end; the page-size select offers 10, 20, 50, 100 with default 10 (AC6.3.1)', async () => {
    fake.listContracts.mockResolvedValue({
      kind: 'ok',
      page: {
        items: [
          fake.summary(1, { name: 'Office lease HQ', startDate: '2027-01-01', endDate: '2029-12-31' }),
          fake.summary(2, { name: 'Cloud hosting agreement', startDate: '2027-03-15', endDate: '2028-03-14' }),
        ],
        page: 1,
        pageSize: 10,
        totalCount: 2,
        totalPages: 1,
      },
    })
    const { user } = renderAt('/contracts')
    const table = await screen.findByTestId('contracts-table')
    expect(fake.listContracts).toHaveBeenCalledWith(1, 10, expect.any(AbortSignal))

    expect(within(table).getAllByRole('columnheader').map((h) => h.textContent)).toEqual(['Name', 'Start', 'End'])
    const row = screen.getByTestId(`contract-row-${fake.summary(1).id}`)
    expect(within(row).getByRole('link', { name: 'Office lease HQ' })).toHaveAttribute(
      'href',
      `/contracts/${fake.summary(1).id}`,
    )
    expect(row).toHaveTextContent('2027-01-01')
    expect(row).toHaveTextContent('2029-12-31')

    const select = screen.getByTestId('page-size-select')
    expect(select).toHaveTextContent('10')
    await user.click(select)
    const options = await screen.findAllByRole('option')
    expect(options.map((o) => o.textContent)).toEqual(['10', '20', '50', '100'])

    // Changing the page size goes back to page 1 with the new size.
    await user.click(screen.getByRole('option', { name: '20' }))
    await waitFor(() => expect(fake.listContracts).toHaveBeenLastCalledWith(1, 20, expect.any(AbortSignal)))
  })

  it('shows "No contracts yet." for an empty list (AC6.3.2)', async () => {
    fake.listContracts.mockResolvedValue({ kind: 'ok', page: fake.contractPage(0) })
    renderAt('/contracts')
    expect(await screen.findByText('No contracts yet.')).toBeInTheDocument()
    expect(screen.getByTestId('new-contract-empty')).toHaveAttribute('href', '/contracts/new')
    expect(screen.queryByTestId('contracts-table')).not.toBeInTheDocument()
  })

  it('shows a contract name with <script> text as plain text (NFR3.5)', async () => {
    const name = '<script>alert("x")</script><b>bold</b>'
    fake.listContracts.mockResolvedValue({
      kind: 'ok',
      page: { items: [fake.summary(1, { name })], page: 1, pageSize: 10, totalCount: 1, totalPages: 1 },
    })
    const { container } = renderAt('/contracts')
    expect(await screen.findByRole('link', { name })).toBeInTheDocument()
    expect(container.querySelector('script')).toBeNull()
    expect(container.querySelector('b')).toBeNull()
  })
})
