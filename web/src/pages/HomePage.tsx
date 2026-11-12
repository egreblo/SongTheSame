import { Music } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { AppNav } from '@/shared/ui/AppNav'
import { SiteFooter } from '@/shared/ui/SiteFooter'

export function HomePage() {
  const { t } = useTranslation()

  return (
    <main className="mx-auto flex min-h-dvh max-w-md flex-col gap-8 px-4 py-6">
      <AppNav />
      <header className="space-y-4 text-center">
        <span className="mx-auto grid size-20 place-items-center rounded-full bg-primary/15 ring-1 ring-primary/40">
          <Music className="size-9 text-primary" aria-hidden />
        </span>
        <h1 className="text-5xl font-bold">{t('app.title')}</h1>
        <p className="text-lg text-muted-foreground">{t('app.tagline')}</p>
      </header>

      <Button size="lg" className="h-14 text-base" nativeButton={false} render={<Link to="/play" />}>
        {t('home.play')}
      </Button>

      <section className="space-y-3 rounded-xl border bg-card/70 p-5 text-sm text-muted-foreground">
        <h2 className="text-base font-semibold text-foreground">{t('home.howTitle')}</h2>
        <p>1. {t('home.how1')}</p>
        <p>2. {t('home.how2')}</p>
        <p>3. {t('home.how3')}</p>
      </section>
      <SiteFooter />
    </main>
  )
}
