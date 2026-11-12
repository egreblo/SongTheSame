import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

const STORAGE_KEY = 'songthesame.noticeDismissed'

function wasDismissed(): boolean {
  try {
    return localStorage.getItem(STORAGE_KEY) === '1'
  } catch {
    return false
  }
}

/** One-time notice that the site keeps a few small settings in the browser, with a link to the privacy page. */
export function StorageNotice() {
  const { t } = useTranslation()
  const [visible, setVisible] = useState(() => !wasDismissed())
  if (!visible) return null

  const dismiss = () => {
    try {
      localStorage.setItem(STORAGE_KEY, '1')
    } catch {
      // Without storage the notice simply shows again next visit.
    }
    setVisible(false)
  }

  return (
    <div role="region" aria-label={t('privacy.noticeLabel')} className="fixed inset-x-0 bottom-0 z-40 p-3">
      <div className="mx-auto flex max-w-md items-center gap-3 rounded-xl border bg-popover p-3 text-sm shadow-lg">
        <p className="flex-1 text-muted-foreground">
          {t('privacy.notice')}{' '}
          {/* A plain link: this component renders outside the router. */}
          <a href={`${import.meta.env.BASE_URL}privacy`} className="underline">
            {t('privacy.learnMore')}
          </a>
        </p>
        <Button size="sm" onClick={dismiss}>
          OK
        </Button>
      </div>
    </div>
  )
}
