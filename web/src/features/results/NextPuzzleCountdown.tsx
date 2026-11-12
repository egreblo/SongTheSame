import { Clock } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { formatCountdown, secondsUntilNextGameDay } from './nextGameDay'

export function NextPuzzleCountdown() {
  const { t } = useTranslation()
  const [seconds, setSeconds] = useState(() => secondsUntilNextGameDay())

  useEffect(() => {
    const timer = setInterval(() => setSeconds(secondsUntilNextGameDay()), 1000)
    return () => clearInterval(timer)
  }, [])

  return (
    <p className="flex items-center justify-center gap-2 text-sm text-muted-foreground">
      <Clock className="size-4" aria-hidden />
      {t('results.nextPuzzle')} <span className="font-mono tabular-nums text-foreground">{formatCountdown(seconds)}</span>
    </p>
  )
}
