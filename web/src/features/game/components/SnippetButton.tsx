import { Loader2, Play, Square } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'

interface SnippetButtonProps {
  active: boolean
  loading: boolean
  progress: number
  onToggle: () => void
  label: string
  className?: string
}

/** Small round play/stop button with a progress ring, for listening to one part of one song. */
export function SnippetButton({ active, loading, progress, onToggle, label, className }: SnippetButtonProps) {
  const { t } = useTranslation()
  const degrees = active && !loading ? Math.round(progress * 360) : 0

  return (
    <button
      type="button"
      onClick={onToggle}
      aria-label={active ? t('game.stop') : label}
      title={active ? t('game.stop') : label}
      className={cn('grid size-10 shrink-0 place-items-center rounded-full p-0.5 transition-transform hover:scale-105 active:scale-95', className)}
      style={{ background: `conic-gradient(var(--primary) ${degrees}deg, oklch(1 0 0 / 16%) 0deg)` }}
    >
      <span className="grid size-full place-items-center rounded-full bg-card">
        {loading ? (
          <Loader2 className="size-4 animate-spin text-primary" aria-hidden />
        ) : active ? (
          <Square className="size-3.5 fill-primary text-primary" aria-hidden />
        ) : (
          <Play className="size-4 translate-x-px fill-primary text-primary" aria-hidden />
        )}
      </span>
    </button>
  )
}
