import { FastForward, X } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import type { RoundView, SongSummary } from '@/shared/api/generated/models'
import { useClipPlayer } from '@/shared/audio/useClipPlayer'
import { ClipTimeline } from './ClipTimeline'
import { PlayButton } from './PlayButton'
import { SongCombobox } from './SongCombobox'

interface Phase1PanelProps {
  round: RoundView
  busy: boolean
  onGuess: (songId: string) => void
  onSkip: () => void
}

/** Phase 1: listen to a short clip, name the song, or skip to unlock a longer clip. */
export function Phase1Panel({ round, busy, onGuess, onSkip }: Phase1PanelProps) {
  const { t } = useTranslation()
  const player = useClipPlayer(round.audioUrl)
  const [selected, setSelected] = useState<SongSummary | null>(null)

  const guessedIds = round.guesses.flatMap((guess) => (guess.song ? [guess.song.id] : []))
  const nextClip = round.clipSeconds[Math.min(round.attempts + 1, round.clipSeconds.length - 1)]
  const isLastAttempt = round.attempts + 1 >= round.maxAttempts

  const submit = () => {
    if (!selected) return
    player.stop()
    onGuess(selected.id)
    setSelected(null)
  }

  return (
    <section className="flex flex-col items-center gap-6">
      <div className="text-center">
        <h2 className="text-2xl font-semibold">{t('game.phase1.title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('game.phase1.hint')}</p>
      </div>

      <PlayButton
        playing={player.playing}
        progress={player.progress}
        disabled={!round.audioUrl}
        onPlay={() => player.play(0, round.unlockedClipSeconds)}
        onStop={player.stop}
      />
      <ClipTimeline clipSeconds={round.clipSeconds} attempts={round.attempts} />

      {round.guesses.length > 0 && (
        <ul className="w-full space-y-1.5">
          {round.guesses.map((guess, index) => (
            <li key={index} className="flex items-center gap-2 rounded-md bg-muted/60 px-3 py-2 text-sm">
              <X className="size-4 text-destructive" aria-hidden />
              {guess.song ? (
                <span className="truncate">
                  {guess.song.title} <span className="text-muted-foreground">· {guess.song.artist}</span>
                </span>
              ) : (
                <span className="text-muted-foreground">{t('game.skipped')}</span>
              )}
            </li>
          ))}
        </ul>
      )}

      <div className="w-full space-y-3">
        <SongCombobox value={selected} onChange={setSelected} excludeIds={guessedIds} disabled={busy} />
        <div className="flex gap-3">
          <Button size="lg" className="flex-1" disabled={!selected || busy} onClick={submit}>
            {t('game.guess')}
          </Button>
          <Button
            size="lg"
            variant="secondary"
            disabled={busy}
            onClick={() => {
              player.stop()
              onSkip()
            }}
          >
            <FastForward className="size-4" aria-hidden />
            {isLastAttempt ? t('game.giveUp') : t('game.skip', { seconds: nextClip })}
          </Button>
        </div>
      </div>
    </section>
  )
}
