import { toPng } from 'html-to-image'
import { Copy, Image as ImageIcon, Mail, MessageCircle, Share2 } from 'lucide-react'
import { useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import type { SessionView } from '@/shared/api/generated/models'
import { buildShareText } from './buildShareText'
import { ShareCard } from './ShareCard'

interface ShareActionsProps {
  session: SessionView
  /** Where the shared text points: the site, or the share link of a custom game. */
  url: string
}

async function renderPng(node: HTMLElement): Promise<File> {
  const dataUrl = await toPng(node, { pixelRatio: 2, cacheBust: true })
  const blob = await (await fetch(dataUrl)).blob()
  return new File([blob], 'songthesame.png', { type: 'image/png' })
}

function download(file: File) {
  const link = document.createElement('a')
  link.href = URL.createObjectURL(file)
  link.download = file.name
  link.click()
  URL.revokeObjectURL(link.href)
}

/** Copy, native share sheet (messages, mail, ...), image, and direct mail / WhatsApp links. */
export function ShareActions({ session, url }: ShareActionsProps) {
  const { t } = useTranslation()
  const cardRef = useRef<HTMLDivElement>(null)
  const text = buildShareText(session, url)

  const copy = async () => {
    await navigator.clipboard.writeText(text)
    toast.success(t('share.copied'))
  }

  const shareNative = async () => {
    try {
      await navigator.share({ text })
    } catch {
      // The user closed the share sheet.
    }
  }

  const shareImage = async () => {
    if (!cardRef.current) return
    const file = await renderPng(cardRef.current)
    if (navigator.canShare?.({ files: [file] })) {
      try {
        await navigator.share({ files: [file], text })
      } catch {
        // Cancelled by the user.
      }
    } else {
      download(file)
    }
  }

  const encoded = encodeURIComponent(text)

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        <Button variant="secondary" onClick={() => void copy()}>
          <Copy className="size-4" aria-hidden />
          {t('share.copy')}
        </Button>
        {'share' in navigator && (
          <Button variant="secondary" onClick={() => void shareNative()}>
            <Share2 className="size-4" aria-hidden />
            {t('share.share')}
          </Button>
        )}
        <Button variant="secondary" onClick={() => void shareImage()}>
          <ImageIcon className="size-4" aria-hidden />
          {t('share.image')}
        </Button>
        <Button variant="secondary" nativeButton={false} render={<a href={`https://wa.me/?text=${encoded}`} target="_blank" rel="noreferrer" />}>
          <MessageCircle className="size-4" aria-hidden />
          WhatsApp
        </Button>
        <Button variant="secondary" nativeButton={false} render={<a href={`mailto:?subject=SongTheSame ${session.day}&body=${encoded}`} />}>
          <Mail className="size-4" aria-hidden />
          {t('share.email')}
        </Button>
      </div>
      <div aria-hidden className="pointer-events-none fixed top-0 -left-[9999px]">
        <ShareCard ref={cardRef} session={session} />
      </div>
    </div>
  )
}
