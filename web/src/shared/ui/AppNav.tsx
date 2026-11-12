import { Music } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink } from 'react-router'
import { AuthButton } from '@/features/auth/AuthButton'
import { cn } from '@/lib/utils'
import { useGetMe } from '@/shared/api/generated/me/me'
import { useAuthStore } from '@/shared/auth/authStore'
import { LanguageSwitch } from '@/shared/i18n/LanguageSwitch'
import { SoundToggle } from './SoundToggle'

const links = [
  { to: '/play', label: 'nav.play' },
  { to: '/leagues', label: 'nav.leagues' },
  { to: '/create', label: 'nav.create' },
] as const

/** Top bar shared by the pages around the game: logo, main sections, sound, language and account. */
export function AppNav() {
  const { t } = useTranslation()
  const user = useAuthStore((state) => state.user)
  const me = useGetMe({ query: { enabled: user !== null, staleTime: 60_000 } })
  const items = me.data?.data.isAdmin ? [...links, { to: '/admin', label: 'nav.admin' }] : links

  return (
    <header className="flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
      <Link to="/" className="flex items-center gap-2 font-heading text-lg font-semibold">
        <Music className="size-5 text-primary" aria-hidden />
        {t('app.title')}
      </Link>
      <div className="flex items-center gap-1">
        <SoundToggle />
        <LanguageSwitch />
        <AuthButton />
      </div>
      <nav className="flex w-full gap-1 text-sm">
        {items.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            className={({ isActive }) =>
              cn('rounded-md px-3 py-1.5 text-muted-foreground transition-colors hover:text-foreground', isActive && 'bg-secondary text-foreground')
            }
          >
            {t(link.label)}
          </NavLink>
        ))}
      </nav>
    </header>
  )
}
