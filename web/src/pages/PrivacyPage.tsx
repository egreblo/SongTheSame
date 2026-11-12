import { useTranslation } from 'react-i18next'
import { PageShell } from '@/shared/ui/PageShell'

const SECTIONS = ['what', 'storage', 'accounts', 'thirdParties', 'rights'] as const

export function PrivacyPage() {
  const { t } = useTranslation()

  return (
    <PageShell>
      <h1 className="text-2xl font-semibold">{t('privacy.title')}</h1>
      <p className="-mt-3 text-sm text-muted-foreground">{t('privacy.updated')}</p>
      <p>{t('privacy.intro')}</p>
      {SECTIONS.map((section) => (
        <section key={section} className="space-y-2">
          <h2 className="text-lg font-semibold">{t(`privacy.${section}.title`)}</h2>
          <p className="text-sm leading-relaxed text-muted-foreground">{t(`privacy.${section}.body`)}</p>
        </section>
      ))}
    </PageShell>
  )
}
