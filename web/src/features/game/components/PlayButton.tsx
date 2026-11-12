import { Play, Square } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'

interface PlayButtonProps {
  playing: boolean
  progress: number
  onPlay: () => void
  onStop: () => void
  label?: string
  disabled?: boolean
}

const BARS = [0.5, 0.9, 0.6, 1, 0.7, 0.85, 0.55]

/** The big round button of the game: play/stop with a progress ring and a small equaliser while playing. */
export function PlayButton({ playing, progress, onPlay, onStop, label, disabled }: PlayButtonProps) {
  const { t } = useTranslation()
  const degrees = Math.round(progress * 360)

  return (
    <div className="flex flex-col items-center gap-3">
      <button
        type="button"
        disabled={disabled}
        onClick={playing ? onStop : onPlay}
        aria-label={playing ? t('game.stop') : (label ?? t('game.play'))}
        className="group relative grid size-28 place-items-center rounded-full p-1.5 transition-transform hover:scale-105 active:scale-95 disabled:opacity-50"
        style={{ background: `conic-gradient(var(--primary) ${degrees}deg, oklch(1 0 0 / 14%) 0deg)` }}
      >
        <span className="grid size-full place-items-center rounded-full bg-card shadow-[inset_0_0_30px_oklch(0.82_0.17_75/0.15)]">
          {playing ? <Square className="size-9 fill-primary text-primary" aria-hidden /> : <Play className="size-10 translate-x-0.5 fill-primary text-primary" aria-hidden />}
        </span>
      </button>
      <div className="flex h-6 items-end gap-1" aria-hidden>
        {BARS.map((height, index) => (
          <span
            key={index}
            className={cn('w-1 origin-bottom rounded-full bg-primary/70', !playing && 'scale-y-[0.2] opacity-40')}
            style={{
              height: `${height * 24}px`,
              animation: playing ? `equalizer ${0.6 + index * 0.07}s ease-in-out ${index * 0.05}s infinite` : undefined,
            }}
          />
        ))}
      </div>
    </div>
  )
}
