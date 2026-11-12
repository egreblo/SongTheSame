import { ArrowRight, Trophy } from 'lucide-react'
import { motion } from 'motion/react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import type { RoundView } from '@/shared/api/generated/models'
import { cn } from '@/lib/utils'
import { ReportDialog } from './ReportDialog'
import { SongCover } from './SongCover'

interface RoundResultCardProps {
  sessionId: string
  round: RoundView
  isLastRound: boolean
  onNext: () => void
}

function scoreTone(score: number) {
  if (score >= 75) return 'text-success'
  if (score >= 40) return 'text-warning'
  return 'text-destructive'
}

/** Shows how phase 2 went: the similarity score, what the best twins would have been, and the report flag. */
export function RoundResultCard({ sessionId, round, isLastRound, onNext }: RoundResultCardProps) {
  const { t } = useTranslation()
  const { phase2, reveal } = round
  if (!phase2 || !reveal) return null

  return (
    <motion.section
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex flex-col gap-5"
    >
      <div className="rounded-xl border bg-card p-5 text-center">
        <p className="text-sm text-muted-foreground">{t('game.similarityScore')}</p>
        <motion.p
          initial={{ scale: 0.6 }}
          animate={{ scale: 1 }}
          transition={{ type: 'spring', stiffness: 260, damping: 14 }}
          className={cn('font-heading text-7xl font-bold tabular-nums', scoreTone(phase2.score))}
        >
          {phase2.score}
        </motion.p>
        <p className="mt-1 text-sm text-muted-foreground">
          {t('game.roundPoints', { points: round.points, phase1: round.phase1Points, phase2: phase2.points })}
        </p>
        <div className="mt-4 flex items-center gap-3 rounded-lg bg-muted/60 p-3 text-left">
          <SongCover url={phase2.guess.coverUrl} size="md" />
          <div className="min-w-0">
            <p className="text-xs text-muted-foreground">{t('game.yourPick')}</p>
            <p className="truncate font-medium">{phase2.guess.title}</p>
            <p className="truncate text-sm text-muted-foreground">{phase2.guess.artist}</p>
          </div>
        </div>
      </div>

      <div>
        <h3 className="mb-2 flex items-center gap-2 text-sm font-semibold text-muted-foreground">
          <Trophy className="size-4" aria-hidden />
          {t('game.bestTwins')}
        </h3>
        <ul className="space-y-1.5">
          {phase2.bestMatches.map((match) => (
            <li key={match.song.id} className="flex items-center gap-3 rounded-md bg-muted/40 px-3 py-2">
              <SongCover url={match.song.coverUrl} size="sm" />
              <span className="min-w-0 flex-1 truncate text-sm">
                {match.song.title} <span className="text-muted-foreground">· {match.song.artist}</span>
              </span>
              <span className="text-sm font-semibold tabular-nums">{match.score}</span>
            </li>
          ))}
        </ul>
      </div>

      <div className="flex items-center justify-between gap-3">
        <ReportDialog sessionId={sessionId} roundIndex={round.index} />
        <Button size="lg" onClick={onNext}>
          {isLastRound ? t('game.seeResults') : t('game.nextRound')}
          <ArrowRight className="size-4" aria-hidden />
        </Button>
      </div>
    </motion.section>
  )
}
