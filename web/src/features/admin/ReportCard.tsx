import { useQueryClient } from '@tanstack/react-query'
import { CheckCircle2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { ListenRow } from '@/features/game/components/ListenRow'
import { getListReportsQueryKey, useReviewReport } from '@/shared/api/generated/admin/admin'
import type { ReportItem } from '@/shared/api/generated/models'
import { useSnippetPlayer } from '@/shared/audio/useSnippetPlayer'

const GUESS_PLAY_SECONDS = 10

/** One flagged score with everything needed to judge it by ear, and the review form. */
export function ReportCard({ report }: { report: ReportItem }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const player = useSnippetPlayer()
  const [note, setNote] = useState('')

  const review = useReviewReport({
    mutation: {
      onSuccess: () => {
        toast.success(t('admin.reviewed'))
        void queryClient.invalidateQueries({ queryKey: getListReportsQueryKey().slice(0, 1) })
      },
      onError: (error: Error) => toast.error(error.message),
    },
  })

  return (
    <article className="space-y-3 rounded-xl border bg-card p-4">
      <header className="flex flex-wrap items-center gap-2 text-sm">
        <Badge variant="secondary">{report.day}</Badge>
        <Badge variant="secondary">{t('admin.round', { number: report.roundIndex + 1 })}</Badge>
        <Badge>{t('admin.scoreShown', { score: report.score })}</Badge>
        <span className="ml-auto text-xs text-muted-foreground">{new Date(report.createdAt).toLocaleString()}</span>
      </header>

      {report.playerNote && <p className="rounded-md bg-muted/50 p-3 text-sm italic">“{report.playerNote}”</p>}

      <ul className="space-y-1.5">
        <ListenRow
          id="target"
          song={report.target}
          caption={t('admin.targetSegment')}
          startSeconds={report.segmentStartSeconds}
          durationSeconds={report.segmentSeconds}
          player={player}
        />
        <ListenRow
          id="guess"
          song={report.guess}
          caption={t('admin.playersPick')}
          startSeconds={report.guessMatchStartSeconds}
          durationSeconds={GUESS_PLAY_SECONDS}
          player={player}
        />
        {report.bestMatches.map((match) => (
          <ListenRow
            key={match.song.id}
            id={`twin-${match.song.id}`}
            song={match.song}
            caption={t('admin.bestTwin')}
            startSeconds={match.startSeconds}
            durationSeconds={GUESS_PLAY_SECONDS}
            player={player}
            trailing={<span className="text-sm font-semibold tabular-nums">{match.score}</span>}
          />
        ))}
      </ul>

      {report.status === 'Reviewed' ? (
        <p className="flex items-center gap-2 text-sm text-success">
          <CheckCircle2 className="size-4" aria-hidden />
          {report.adminNote ?? t('admin.reviewedNoNote')}
        </p>
      ) : (
        <div className="space-y-2">
          <Textarea value={note} maxLength={500} placeholder={t('admin.notePlaceholder')} onChange={(event) => setNote(event.target.value)} />
          <Button
            disabled={review.isPending}
            onClick={() => {
              player.stop()
              review.mutate({ reportId: report.id, data: { note: note.trim() || null } })
            }}
          >
            {t('admin.markReviewed')}
          </Button>
        </div>
      )}
    </article>
  )
}
