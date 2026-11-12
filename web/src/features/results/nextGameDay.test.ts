import { describe, expect, it } from 'vitest'
import { formatCountdown, secondsUntilNextGameDay } from './nextGameDay'

describe('secondsUntilNextGameDay', () => {
  it('counts to Zagreb midnight (UTC+1 in winter)', () => {
    // 22:30 UTC is 23:30 in Zagreb, so 30 minutes remain.
    expect(secondsUntilNextGameDay(new Date('2026-11-12T22:30:00Z'))).toBe(30 * 60)
  })

  it('counts to Zagreb midnight (UTC+2 in summer)', () => {
    // 22:30 UTC is 00:30 the next day in Zagreb, so 23h30m remain.
    expect(secondsUntilNextGameDay(new Date('2026-07-01T22:30:00Z'))).toBe(23 * 3600 + 30 * 60)
  })
})

describe('formatCountdown', () => {
  it('formats as HH:MM:SS', () => {
    expect(formatCountdown(3661)).toBe('01:01:01')
  })
})
