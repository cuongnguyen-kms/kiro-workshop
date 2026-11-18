// US3.1 and US1.1: money and period formatting (NFR3.1, AC3.1.1 to AC3.1.3, SEC2).
import { describe, expect, it } from 'vitest'
import { formatMoney, formatPeriod } from '@/contracts/display'

describe('formatMoney', () => {
  it.each([
    [1000, '1,000.00'],
    [1234567.5, '1,234,567.50'],
    [-1000, '(1,000.00)'],
    [-0.004, '0.00'],
    [0, '0.00'],
    [12, '12.00'],
    [0.5, '0.50'],
    [-0.5, '(0.50)'],
    [123456789.99, '123,456,789.99'],
    [-1234567.5, '(1,234,567.50)'],
  ])('formats %s as %s', (input, expected) => {
    expect(formatMoney(input)).toBe(expected)
  })

  it('shows 0.00 for negative zero, with no sign or brackets (AC3.1.3)', () => {
    expect(formatMoney(-0)).toBe('0.00')
  })

  it('rounds half away from zero on the digit string, not on the float (NFR3.1)', () => {
    expect(formatMoney(1.005)).toBe('1.01')
    expect(formatMoney(-1.005)).toBe('(1.01)')
    expect(formatMoney(999.995)).toBe('1,000.00')
    expect(formatMoney(999.999)).toBe('1,000.00')
    expect(formatMoney(0.004)).toBe('0.00')
    expect(formatMoney(0.005)).toBe('0.01')
    expect(formatMoney(5e-7)).toBe('0.00')
  })

  it('handles very large amounts without exponent form', () => {
    expect(formatMoney(1e15)).toBe('1,000,000,000,000,000.00')
    expect(formatMoney(-1e15)).toBe('(1,000,000,000,000,000.00)')
  })

  it('returns an empty string for a value that is not a finite number (SEC2)', () => {
    expect(formatMoney(NaN)).toBe('')
    expect(formatMoney(Infinity)).toBe('')
    expect(formatMoney(-Infinity)).toBe('')
    expect(formatMoney(null)).toBe('')
    expect(formatMoney(undefined)).toBe('')
  })
})

describe('formatPeriod', () => {
  it.each([
    ['2027-01-01', 'Jan 2027'],
    ['2027-12-31', 'Dec 2027'],
    ['2028-02-29', 'Feb 2028'],
    ['2027-03-15', 'Mar 2027'],
  ])('shows %s as %s', (input, expected) => {
    expect(formatPeriod(input)).toBe(expected)
  })

  it('returns a malformed value unchanged (SEC2)', () => {
    expect(formatPeriod('not a date')).toBe('not a date')
    expect(formatPeriod('2027-13-01')).toBe('2027-13-01')
    expect(formatPeriod('')).toBe('')
  })
})
