import { ArrowRight, Headphones, SkipForward } from 'lucide-react'
import { motion } from 'motion/react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import type { RoundView } from '@/shared/api/generated/models'
import { cn } from '@/lib/utils'
import { useSnippetPlayer } from '@/shared/audio/useSnippetPlayer'
import { ListenRow } from './ListenRow'
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

/**
 * Shows how phase 2 went: the similarity score, and a "listen" list so the player can hear for themselves why a
 * song counts as similar. Each row starts the song at the part that matches the revealed segment best.
 */
export function RoundResultCard({ sessionId, round, isLastRound, onNext }: RoundResultCardProps) {
  const { t } = useTranslation()
  const player = useSnippetPlayer()
  const { phase2, reveal } = round
  if (!phase2 || !reveal) return null

  const duration = Math.max(8, reveal.segmentSeconds + 2)
  const originalUrl = round.audioUrl
  const guess = phase2.guess

  return (
    <motion.section initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="flex flex-col gap-5">
      <div className="rounded-xl border bg-card p-5 text-center">
        <p className="text-sm text-muted-foreground">{t('game.similarityScore')}</p>
        {phase2.skipped ? (
          <>
            <p className="my-3 flex items-center justify-center gap-2 text-2xl font-semibold text-muted-foreground">
              <SkipForward className="size-6" aria-hidden />
              {t('game.skippedTwin')}
            </p>
            <p className="text-sm text-muted-foreground">{t('game.roundPoints', { points: round.points, phase1: round.phase1Points, phase2: 0 })}</p>
          </>
        ) : (
          <>
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
          </>
        )}
        {guess && (
          <div className="mt-4 flex items-center gap-3 rounded-lg bg-muted/60 p-3 text-left">
            <SongCover url={guess.coverUrl} size="md" />
            <div className="min-w-0">
              <p className="text-xs text-muted-foreground">{t('game.yourPick')}</p>
              <p className="truncate font-medium">{guess.title}</p>
              <p className="truncate text-sm text-muted-foreground">{guess.artist}</p>
            </div>
          </div>
        )}
      </div>

      <div>
        <h3 className="mb-1 flex items-center gap-2 text-sm font-semibold text-muted-foreground">
          <Headphones className="size-4" aria-hidden />
          {t('game.listen')}
        </h3>
        <p className="mb-2 text-xs text-muted-foreground">{t('game.listenHint')}</p>
        <ul className="space-y-1.5">
          <ListenRow
            id="original"
            song={reveal.song}
            caption={t('game.original')}
            startSeconds={reveal.segmentStartSeconds}
            durationSeconds={reveal.segmentSeconds}
            player={player}
            resolveUrl={originalUrl ? () => Promise.resolve(originalUrl) : undefined}
          />
          {guess && (
            <ListenRow
              id="guess"
              song={guess}
              caption={t('game.yourPick')}
              startSeconds={phase2.guessMatchStartSeconds}
              durationSeconds={duration}
              player={player}
              trailing={<span className="text-sm font-semibold tabular-nums">{phase2.score}</span>}
            />
          )}
        </ul>

        <h3 className="mb-2 mt-4 text-sm font-semibold text-muted-foreground">{t('game.bestTwins')}</h3>
        <ul className="space-y-1.5">
          {phase2.bestMatches.map((match) => (
            <ListenRow
              key={match.song.id}
              id={`twin-${match.song.id}`}
              song={match.song}
              startSeconds={match.startSeconds}
              durationSeconds={duration}
              player={player}
              trailing={<span className="text-sm font-semibold tabular-nums">{match.score}</span>}
            />
          ))}
        </ul>
      </div>

      <div className="flex items-center justify-between gap-3">
        {guess ? <ReportDialog sessionId={sessionId} roundIndex={round.index} /> : <span />}
        <Button
          size="lg"
          onClick={() => {
            player.stop()
            onNext()
          }}
        >
          {isLastRound ? t('game.seeResults') : t('game.nextRound')}
          <ArrowRight className="size-4" aria-hidden />
        </Button>
      </div>
    </motion.section>
  )
}
