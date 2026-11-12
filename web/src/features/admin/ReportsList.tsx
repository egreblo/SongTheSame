import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { useListReports } from '@/shared/api/generated/admin/admin'
import { ReportCard } from './ReportCard'

type Tab = 'Open' | 'Reviewed'

/** Admin inbox of flagged scores, with Open / Reviewed tabs. */
export function ReportsList() {
  const { t } = useTranslation()
  const [tab, setTab] = useState<Tab>('Open')
  const { data, isLoading, isError } = useListReports({ status: tab })
  const reports = data?.data ?? []

  return (
    <div className="space-y-4">
      <div className="flex gap-2">
        {(['Open', 'Reviewed'] as const).map((value) => (
          <Button key={value} size="sm" variant={tab === value ? 'default' : 'secondary'} onClick={() => setTab(value)}>
            {t(`admin.tab${value}`)}
          </Button>
        ))}
      </div>

      {isLoading && <p className="text-sm text-muted-foreground">{t('game.loading')}</p>}
      {isError && <p className="text-sm text-destructive">{t('admin.forbidden')}</p>}
      {!isLoading && !isError && reports.length === 0 && <p className="text-sm text-muted-foreground">{t('admin.empty')}</p>}
      {reports.map((report) => (
        <ReportCard key={report.id} report={report} />
      ))}
    </div>
  )
}
