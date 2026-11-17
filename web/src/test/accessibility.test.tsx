// NFR7.4: axe finds no serious or critical issues on S1, S2 and S3.
import { screen } from '@testing-library/react'
import { axe } from 'vitest-axe'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import * as fake from '@/test/fakeApi'
import { renderAt, seriousViolations } from '@/test/render'

vi.mock('@/api/contracts', () => import('@/test/fakeApi'))

beforeEach(() => fake.resetFakeApi())

describe('Accessibility (axe)', () => {
  it('S1 Contracts list has no serious or critical issues', async () => {
    const { container } = renderAt('/contracts')
    await screen.findByTestId('contracts-table')
    expect(seriousViolations(await axe(container))).toEqual([])
  })

  it('S2 New contract has no serious or critical issues, including with errors shown', async () => {
    const { container, user } = renderAt('/contracts/new')
    await screen.findByTestId('contract-form')
    expect(seriousViolations(await axe(container))).toEqual([])

    await user.click(screen.getByTestId('expense-method-Overtime'))
    await user.click(screen.getByTestId('save-contract'))
    await screen.findByTestId('error-summary')
    expect(seriousViolations(await axe(container))).toEqual([])
  })

  it('S3 Contract detail has no serious or critical issues', async () => {
    const { container } = renderAt(`/contracts/${fake.CONTRACT_ID}`, { saved: true })
    await screen.findByTestId('contract-name')
    expect(seriousViolations(await axe(container))).toEqual([])
  })
})
