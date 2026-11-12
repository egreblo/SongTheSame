const SECONDS_PER_DAY = 24 * 60 * 60

/** Seconds until midnight in Europe/Zagreb, when the next daily puzzle starts. */
export function secondsUntilNextGameDay(now: Date = new Date()): number {
  const parts = new Intl.DateTimeFormat('en-GB', {
    timeZone: 'Europe/Zagreb',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hourCycle: 'h23',
  }).formatToParts(now)

  const value = (type: string) => Number(parts.find((part) => part.type === type)?.value ?? 0)
  return SECONDS_PER_DAY - (value('hour') * 3600 + value('minute') * 60 + value('second'))
}

export function formatCountdown(totalSeconds: number): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  return `${pad(hours)}:${pad(minutes)}:${pad(totalSeconds % 60)}`
}
