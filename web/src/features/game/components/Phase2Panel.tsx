import { SkipForward } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import type { RoundView, SongSummary } from '@/shared/api/generated/models'
import { useClipPlayer } from '@/shared/audio/useClipPlayer'
import { PlayButton } from './PlayButton'
import { SongCombobox } from './SongCombobox'
import { SongCover } from './SongCover'

interface Phase2PanelProps {
  round: RoundView
  busy: boolean
  onSubmit: (songId: string) => void
  onSkip: () => void
}

/** Phase 2: the song is revealed; the player names another song that contains the most similar part. */
export function Phase2Panel({ round, busy, onSubmit, onSkip }: Phase2PanelProps) {
  const { t } = useTranslation()
  const reveal = round.reveal
  const player = useClipPlayer(round.audioUrl)
  const [selected, setSelected] = useState<SongSummary | null>(null)
  const [confirmingSkip, setConfirmingSkip] = useState(false)
  if (!reveal) return null

  return (
    <section className="flex flex-col items-center gap-6">
      <div className="flex w-full items-center gap-4 rounded-xl border bg-card p-4">
        <SongCover url={reveal.song.coverUrl} size="lg" />
        <div className="min-w-0">
          <Badge variant={reveal.solved ? 'default' : 'secondary'}>
            {reveal.solved ? t('game.solvedIn', { count: round.attempts }) : t('game.notSolved')}
          </Badge>
          <p className="mt-2 truncate text-lg font-semibold">{reveal.song.title}</p>
          <p className="truncate text-sm text-muted-foreground">{reveal.song.artist}</p>
        </div>
      </div>

      <div className="text-center">
        <h2 className="text-2xl font-semibold">{t('game.phase2.title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('game.phase2.hint')}</p>
      </div>

      <PlayButton
        playing={player.playing}
        progress={player.progress}
        disabled={!round.audioUrl}
        label={t('game.playSegment')}
        onPlay={() => player.play(reveal.segmentStartSeconds, reveal.segmentSeconds)}
        onStop={player.stop}
      />

      <div className="w-full space-y-3">
        <SongCombobox value={selected} onChange={setSelected} excludeIds={[reveal.song.id]} disabled={busy} />
        <Button
          size="lg"
          className="w-full"
          disabled={!selected || busy}
          onClick={() => {
            if (!selected) return
            player.stop()
            onSubmit(selected.id)
          }}
        >
          {t('game.phase2.submit')}
        </Button>
        <p className="text-center text-xs text-muted-foreground">{t('game.phase2.oneTry')}</p>

        {confirmingSkip ? (
          <div className="flex items-center justify-between gap-2 rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm">
            <span>{t('game.phase2.skipConfirm')}</span>
            <span className="flex gap-1">
              <Button size="sm" variant="ghost" disabled={busy} onClick={() => setConfirmingSkip(false)}>
                {t('game.phase2.skipCancel')}
              </Button>
              <Button
                size="sm"
                variant="destructive"
                disabled={busy}
                onClick={() => {
                  player.stop()
                  onSkip()
                }}
              >
                {t('game.phase2.skipYes')}
              </Button>
            </span>
          </div>
        ) : (
          <Button variant="ghost" className="w-full text-muted-foreground" disabled={busy} onClick={() => setConfirmingSkip(true)}>
            <SkipForward className="size-4" aria-hidden />
            {t('game.phase2.skip')}
          </Button>
        )}
      </div>
    </section>
  )
}
