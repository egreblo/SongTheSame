import { cn } from '@/lib/utils'

interface ClipTimelineProps {
  clipSeconds: number[]
  /** Number of attempts already used; the clip at this index is the one currently unlocked. */
  attempts: number
}

/** Shows how much of the song each attempt unlocks (1s, 2s, 4s, ...), highlighting the current clip. */
export function ClipTimeline({ clipSeconds, attempts }: ClipTimelineProps) {
  const current = Math.min(attempts, clipSeconds.length - 1)

  return (
    <ol className="flex w-full gap-1.5" aria-label="clip lengths">
      {clipSeconds.map((seconds, index) => {
        const previous = index === 0 ? 0 : clipSeconds[index - 1]
        return (
          <li key={seconds} className="flex flex-col items-center gap-1.5" style={{ flexGrow: seconds - previous }}>
            <span
              className={cn(
                'h-2.5 w-full rounded-full transition-colors',
                index < current && 'bg-destructive/60',
                index === current && 'bg-primary shadow-[0_0_12px_var(--primary)]',
                index > current && 'bg-muted',
              )}
            />
            <span className={cn('text-xs tabular-nums', index === current ? 'font-semibold text-primary' : 'text-muted-foreground')}>
              {seconds}s
            </span>
          </li>
        )
      })}
    </ol>
  )
}
