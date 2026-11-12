import confetti from 'canvas-confetti'
import { animate, motion } from 'motion/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { SessionView } from '@/shared/api/generated/models'
import { SongCover } from '@/features/game/components/SongCover'
import { phase1Square } from './buildShareText'
import { NextPuzzleCountdown } from './NextPuzzleCountdown'
import { ScoreHistogram } from './ScoreHistogram'
import { ShareActions } from './ShareActions'

const CONFETTI_THRESHOLD = 0.6

function useCountUp(target: number): number {
  const [value, setValue] = useState(target)
  useEffect(() => {
    if (document.hidden) return
    const controls = animate(0, target, { duration: 1.2, ease: 'easeOut', onUpdate: (latest) => setValue(Math.round(latest)) })
    return () => controls.stop()
  }, [target])
  return value
}

/** End-of-day summary: total, per-round breakdown, comparison with other players and sharing. */
export function ResultsPanel({ session, shareUrl }: { session: SessionView; shareUrl: string }) {
  const { t } = useTranslation()
  const total = useCountUp(session.totalPoints)

  useEffect(() => {
    if (session.totalPoints / session.maxPoints >= CONFETTI_THRESHOLD)
      void confetti({ particleCount: 140, spread: 80, origin: { y: 0.3 } })
  }, [session.totalPoints, session.maxPoints])

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex flex-col gap-8">
      <header className="text-center">
        <p className="text-sm text-muted-foreground">{t('results.title', { day: session.day })}</p>
        <p className="font-heading text-8xl font-bold tabular-nums text-primary">{total}</p>
        <p className="text-muted-foreground">
          {t('results.outOf', { max: session.maxPoints })}
        </p>
      </header>

      <ul className="space-y-2">
        {session.rounds.map((round) => (
          <li key={round.index} className="flex items-center gap-3 rounded-lg border bg-card p-3">
            <SongCover url={round.reveal?.song.coverUrl} size="md" />
            <div className="min-w-0 flex-1">
              <p className="truncate font-medium">{round.reveal?.song.title}</p>
              <p className="truncate text-sm text-muted-foreground">{round.reveal?.song.artist}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">
                🎧 {phase1Square(round)} {round.phase1Points} · 🎯 {round.phase2?.score ?? 0} → {round.phase2?.points ?? 0}
              </p>
            </div>
            <span className="font-heading text-2xl font-semibold tabular-nums">{round.points}</span>
          </li>
        ))}
      </ul>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">{t('results.compare')}</h2>
        <ScoreHistogram sessionId={session.sessionId} />
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">{t('results.share')}</h2>
        <ShareActions session={session} url={shareUrl} />
      </section>

      <NextPuzzleCountdown />
    </motion.div>
  )
}
