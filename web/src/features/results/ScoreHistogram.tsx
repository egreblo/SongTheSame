import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'
import { useGetDailyStats } from '@/shared/api/generated/daily/daily'

interface ScoreHistogramProps {
  sessionId: string
}

/** How today's players scored, with the current player's bar highlighted. */
export function ScoreHistogram({ sessionId }: ScoreHistogramProps) {
  const { t } = useTranslation()
  const { data, isLoading } = useGetDailyStats(sessionId)
  const stats = data?.data

  if (isLoading) return <p className="text-sm text-muted-foreground">{t('results.loadingStats')}</p>
  if (!stats) return null

  const tallest = Math.max(1, ...stats.histogram.map((bin) => bin.count))

  return (
    <div className="space-y-3">
      <p className="text-sm">
        {stats.beatPercent === null || stats.beatPercent === undefined
          ? t('results.firstPlayer')
          : t('results.beatPercent', { percent: stats.beatPercent, count: stats.playerCount })}
      </p>
      <div className="flex h-28 items-end gap-1.5" role="img" aria-label={t('results.histogramLabel')}>
        {stats.histogram.map((bin) => {
          const mine = stats.yourPoints >= bin.from && stats.yourPoints <= bin.to
          return (
            <div key={bin.from} className="flex h-full flex-1 flex-col justify-end gap-1">
              <div
                className={cn('min-h-1 rounded-t-sm', mine ? 'bg-primary' : 'bg-muted-foreground/40')}
                style={{ height: `${(bin.count / tallest) * 100}%` }}
                title={`${bin.from}-${bin.to}: ${bin.count}`}
              />
              <span className="text-center text-[10px] text-muted-foreground tabular-nums">{bin.from}</span>
            </div>
          )
        })}
      </div>
      <p className="text-xs text-muted-foreground">{t('results.average', { average: Math.round(stats.averagePoints) })}</p>
    </div>
  )
}
