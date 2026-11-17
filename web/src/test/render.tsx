import { render } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { AppRoutes } from '@/App'

/** Renders the app's routes at an address, with a user-event instance. */
export function renderAt(path: string, state?: unknown) {
  const user = userEvent.setup()
  const result = render(
    <MemoryRouter initialEntries={[state === undefined ? path : { pathname: path, state }]}>
      <AppRoutes />
    </MemoryRouter>,
  )
  return { user, ...result }
}

/** axe violations that are serious or critical (NFR7.4). */
export function seriousViolations(results: { violations: { id: string; impact?: string | null }[] }) {
  return results.violations
    .filter((v) => v.impact === 'serious' || v.impact === 'critical')
    .map((v) => `${v.id} (${v.impact})`)
}
