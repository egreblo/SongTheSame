import { useTranslation } from 'react-i18next'
import { RequireAccount } from '@/features/auth/RequireAccount'
import { LeagueList } from '@/features/leagues/LeagueList'
import { PageShell } from '@/shared/ui/PageShell'

export function LeaguesPage() {
  const { t } = useTranslation()

  return (
    <PageShell>
      <h1 className="text-2xl font-semibold">{t('nav.leagues')}</h1>
      <p className="-mt-3 text-sm text-muted-foreground">{t('leagues.intro')}</p>
      <RequireAccount>
        <LeagueList />
      </RequireAccount>
    </PageShell>
  )
}
