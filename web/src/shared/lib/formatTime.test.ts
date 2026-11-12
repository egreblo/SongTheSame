import { describe, expect, it } from 'vitest'
import { formatTime } from './formatTime'

describe('formatTime', () => {
  it('formats seconds as m:ss', () => {
    expect(formatTime(0)).toBe('0:00')
    expect(formatTime(12.4)).toBe('0:12')
    expect(formatTime(75.6)).toBe('1:16')
  })

  it('never goes negative', () => {
    expect(formatTime(-3)).toBe('0:00')
  })
})
