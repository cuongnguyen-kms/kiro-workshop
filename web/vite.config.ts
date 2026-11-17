/// <reference types="vitest/config" />
import path from 'node:path'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  build: {
    // The size check (scripts/size-check.mjs) reads the manifest to find the list screen's chunks.
    manifest: true,
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    css: false,
    // Form tests type many fields; this is the per-test time budget, not a product target.
    testTimeout: 20000,
    projects: [
      {
        extends: true,
        test: {
          name: 'unit',
          include: ['src/**/*.test.{ts,tsx}'],
          exclude: ['src/**/*.perf.test.{ts,tsx}'],
          sequence: { groupOrder: 0 },
        },
      },
      {
        // The NFR5.3 timing test runs on its own after the other tests, so parallel test
        // files do not compete for the CPU while it measures. The 1-second target is unchanged.
        extends: true,
        test: {
          name: 'performance',
          include: ['src/**/*.perf.test.{ts,tsx}'],
          sequence: { groupOrder: 1 },
        },
      },
    ],
  },
})
