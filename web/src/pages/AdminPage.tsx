import { useTranslation } from 'react-i18next'
import { RequireAccount } from '@/features/auth/RequireAccount'
import { ReportsList } from '@/features/admin/ReportsList'
import { useGetMe } from '@/shared/api/generated/me/me'
import { PageShell } from '@/shared/ui/PageShell'

function AdminContent() {
  const { t } = useTranslation()
  const me = useGetMe()

  if (me.isLoading) return <p className="text-muted-foreground">{t('game.loading')}</p>
  if (!me.data?.data.isAdmin) return <p className="rounded-xl border bg-card p-6 text-center">{t('admin.forbidden')}</p>
  return <ReportsList />
}

export function AdminPage() {
  const { t } = useTranslation()

  return (
    <PageShell>
      <h1 className="text-2xl font-semibold">{t('admin.title')}</h1>
      <p className="-mt-3 text-sm text-muted-foreground">{t('admin.intro')}</p>
      <RequireAccount>
        <AdminContent />
      </RequireAccount>
    </PageShell>
  )
}
