import '@testing-library/jest-dom/vitest'
import { cleanup, configure } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// Screens are lazy-loaded, so the first render of a screen waits for its module to transform.
// This is a wait budget for the test tool, not a product target.
configure({ asyncUtilTimeout: 15000 })

afterEach(() => {
  cleanup()
})

// jsdom lacks a few browser APIs that the Radix UI primitives use.
if (!('ResizeObserver' in globalThis)) {
  class ResizeObserverStub {
    observe() {}
    unobserve() {}
    disconnect() {}
  }
  vi.stubGlobal('ResizeObserver', ResizeObserverStub)
}

const elementProto = window.HTMLElement.prototype as unknown as Record<string, unknown>
for (const name of ['hasPointerCapture', 'setPointerCapture', 'releasePointerCapture']) {
  if (!(name in elementProto)) elementProto[name] = () => false
}
if (!('scrollIntoView' in elementProto)) elementProto.scrollIntoView = () => {}

if (!window.matchMedia) {
  window.matchMedia = (query: string) =>
    ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => false,
    }) as MediaQueryList
}
