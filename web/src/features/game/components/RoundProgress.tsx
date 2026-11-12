import { Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { RoundView } from '@/shared/api/generated/models'
import { RoundStatus } from '@/shared/api/generated/models'

interface RoundProgressProps {
  rounds: RoundView[]
  viewIndex: number
  totalPoints: number
}

/** Five dots for the five rounds of the day, plus the running point total. */
export function RoundProgress({ rounds, viewIndex, totalPoints }: RoundProgressProps) {
  return (
    <div className="flex items-center justify-between gap-4">
      <ol className="flex items-center gap-2">
        {rounds.map((round) => (
          <li
            key={round.index}
            aria-current={round.index === viewIndex ? 'step' : undefined}
            className={cn(
              'grid size-8 place-items-center rounded-full border text-xs font-semibold transition-colors',
              round.status === RoundStatus.Done && 'border-primary bg-primary text-primary-foreground',
              round.index === viewIndex && round.status !== RoundStatus.Done && 'border-primary text-primary',
              round.index !== viewIndex && round.status !== RoundStatus.Done && 'text-muted-foreground',
            )}
          >
            {round.status === RoundStatus.Done ? <Check className="size-4" aria-hidden /> : round.index + 1}
          </li>
        ))}
      </ol>
      <p className="font-heading text-lg font-semibold tabular-nums">
        {totalPoints} <span className="text-sm font-normal text-muted-foreground">pts</span>
      </p>
    </div>
  )
}
