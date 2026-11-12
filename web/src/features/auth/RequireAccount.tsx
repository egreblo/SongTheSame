import { useTranslation } from 'react-i18next'
import { useAuthStore } from '@/shared/auth/authStore'
import { authEnabled } from '@/shared/auth/supabaseClient'
import { SignInDialog } from './SignInDialog'

/** Shows its children to signed-in players, and a sign-in prompt to everyone else. */
export function RequireAccount({ children }: { children: React.ReactNode }) {
  const { t } = useTranslation()
  const { ready, user } = useAuthStore()

  if (!ready) return <p className="text-center text-muted-foreground">{t('game.loading')}</p>
  if (user) return children

  return (
    <div className="space-y-4 rounded-xl border bg-card p-6 text-center">
      <p>{authEnabled ? t('auth.required') : t('auth.notConfigured')}</p>
      {authEnabled && <SignInDialog trigger={<button type="button" className="rounded-md bg-primary px-4 py-2 text-primary-foreground" />} />}
    </div>
  )
}
