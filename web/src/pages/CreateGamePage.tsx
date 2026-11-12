import { useTranslation } from 'react-i18next'
import { RequireAccount } from '@/features/auth/RequireAccount'
import { GameEditor } from '@/features/editor/GameEditor'
import { MyGames } from '@/features/editor/MyGames'
import { PageShell } from '@/shared/ui/PageShell'

export function CreateGamePage() {
  const { t } = useTranslation()

  return (
    <PageShell>
      <h1 className="text-2xl font-semibold">{t('nav.create')}</h1>
      <p className="-mt-3 text-sm text-muted-foreground">{t('editor.intro')}</p>
      <RequireAccount>
        <GameEditor />
        <MyGames />
      </RequireAccount>
    </PageShell>
  )
}
