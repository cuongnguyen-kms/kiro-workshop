// Smoke test: the app renders and lands on the contracts list.
import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from '@/App'
import * as fake from '@/test/fakeApi'

vi.mock('@/api/contracts', () => import('@/test/fakeApi'))

beforeEach(() => fake.resetFakeApi())

describe('App', () => {
  it('renders and redirects / to the contracts list', async () => {
    render(<App />)
    expect(screen.getByTestId('app')).toBeInTheDocument()
    expect(await screen.findByTestId('contracts-list-page')).toBeInTheDocument()
    expect(window.location.pathname).toBe('/contracts')
  })
})
