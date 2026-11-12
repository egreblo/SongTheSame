import { Copy, Play } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import type { CustomGameSummary } from '@/shared/api/generated/models'

/** A created game with its share link, a copy button and a play button. */
export function CustomGameLink({ game }: { game: CustomGameSummary }) {
  const { t } = useTranslation()
  const url = `${window.location.origin}/g/${game.code}`

  const copy = async () => {
    await navigator.clipboard.writeText(url)
    toast.success(t('share.copied'))
  }

  return (
    <div className="space-y-3 rounded-xl border bg-card p-4">
      <p className="font-semibold">{game.title}</p>
      <p className="truncate rounded-md bg-muted px-3 py-2 font-mono text-xs">{url}</p>
      <div className="flex gap-2">
        <Button variant="secondary" onClick={() => void copy()}>
          <Copy className="size-4" aria-hidden />
          {t('share.copy')}
        </Button>
        <Button nativeButton={false} render={<Link to={`/g/${game.code}`} />}>
          <Play className="size-4" aria-hidden />
          {t('editor.playIt')}
        </Button>
      </div>
    </div>
  )
}
