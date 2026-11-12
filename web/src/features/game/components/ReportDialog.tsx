import { Flag } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { Textarea } from '@/components/ui/textarea'
import { useReportRound } from '@/shared/api/generated/daily/daily'

interface ReportDialogProps {
  sessionId: string
  roundIndex: number
}

/** Lets a player flag a phase 2 score they think is wrong (the flag icon next to a round's result). */
export function ReportDialog({ sessionId, roundIndex }: ReportDialogProps) {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)
  const [note, setNote] = useState('')
  const [reported, setReported] = useState(false)

  const report = useReportRound({
    mutation: {
      onSuccess: (response) => {
        setReported(true)
        setOpen(false)
        toast.success(response.data ? t('report.thanks') : t('report.already'))
      },
      onError: (error: Error) => toast.error(error.message),
    },
  })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger
        render={<Button variant="ghost" size="sm" disabled={reported} aria-label={t('report.open')} className="text-muted-foreground" />}
      >
        <Flag className="size-4" aria-hidden />
        {reported ? t('report.sent') : t('report.open')}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('report.title')}</DialogTitle>
          <DialogDescription>{t('report.description')}</DialogDescription>
        </DialogHeader>
        <Textarea
          value={note}
          maxLength={500}
          placeholder={t('report.placeholder')}
          onChange={(event) => setNote(event.target.value)}
        />
        <Button
          disabled={report.isPending}
          onClick={() => report.mutate({ sessionId, round: roundIndex, data: { note: note.trim() || null } })}
        >
          {t('report.send')}
        </Button>
      </DialogContent>
    </Dialog>
  )
}
