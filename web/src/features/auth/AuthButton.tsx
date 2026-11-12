import { LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { useGetMe } from '@/shared/api/generated/me/me'
import { signOut, useAuthStore } from '@/shared/auth/authStore'
import { authEnabled } from '@/shared/auth/supabaseClient'
import { SignInDialog } from './SignInDialog'

/** "Sign in" for guests, name + sign out for players with an account. Hidden when sign-in is not configured. */
export function AuthButton() {
  const { t } = useTranslation()
  const user = useAuthStore((state) => state.user)
  const me = useGetMe({ query: { enabled: user !== null, staleTime: 60_000 } })

  if (!authEnabled) return null
  if (!user) return <SignInDialog />

  return (
    <div className="flex items-center gap-1">
      <span className="max-w-32 truncate text-sm">{me.data?.data.displayName ?? user.name ?? user.email}</span>
      <Button variant="ghost" size="icon" aria-label={t('auth.signOut')} onClick={() => void signOut()}>
        <LogOut className="size-4" aria-hidden />
      </Button>
    </div>
  )
}
