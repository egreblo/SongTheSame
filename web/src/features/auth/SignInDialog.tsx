import { LogIn } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { signInWithEmail, signInWithProvider } from '@/shared/auth/authStore'
import { authProviders } from '@/shared/auth/supabaseClient'

const providerLabels: Record<string, string> = {
  google: 'Google',
  apple: 'Apple',
  github: 'GitHub',
  facebook: 'Facebook',
  discord: 'Discord',
  twitter: 'X',
}

/** Sign in with a social provider or a magic link sent by e-mail. */
export function SignInDialog({ trigger }: { trigger?: React.ReactElement }) {
  const { t } = useTranslation()
  const [email, setEmail] = useState('')
  const [status, setStatus] = useState<'idle' | 'sent' | string>('idle')

  const sendLink = async () => {
    const error = await signInWithEmail(email.trim())
    setStatus(error ?? 'sent')
  }

  return (
    <Dialog>
      <DialogTrigger render={trigger ?? <Button variant="secondary" size="sm" />}>
        <LogIn className="size-4" aria-hidden />
        {t('auth.signIn')}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('auth.title')}</DialogTitle>
          <DialogDescription>{t('auth.description')}</DialogDescription>
        </DialogHeader>
        <div className="space-y-2">
          {authProviders.map((provider) => (
            <Button key={provider} variant="outline" size="lg" className="w-full" onClick={() => void signInWithProvider(provider)}>
              {t('auth.continueWith', { provider: providerLabels[provider] ?? provider })}
            </Button>
          ))}
        </div>
        <form
          className="space-y-2"
          onSubmit={(event) => {
            event.preventDefault()
            void sendLink()
          }}
        >
          <label className="text-sm text-muted-foreground" htmlFor="signin-email">
            {t('auth.emailLabel')}
          </label>
          <Input id="signin-email" type="email" required value={email} onChange={(event) => setEmail(event.target.value)} />
          <Button type="submit" className="w-full" disabled={!email.trim()}>
            {t('auth.sendLink')}
          </Button>
          {status === 'sent' && <p className="text-sm text-success">{t('auth.linkSent')}</p>}
          {status !== 'idle' && status !== 'sent' && <p className="text-sm text-destructive">{status}</p>}
        </form>
      </DialogContent>
    </Dialog>
  )
}
