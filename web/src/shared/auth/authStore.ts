import type { Provider } from '@supabase/supabase-js'
import { create } from 'zustand'
import { setAccessToken } from './accessToken'
import { supabase } from './supabaseClient'

export interface AuthUser {
  id: string
  email?: string
  name?: string
}

interface AuthState {
  /** False until Supabase has told us whether someone is signed in. */
  ready: boolean
  user: AuthUser | null
}

export const useAuthStore = create<AuthState>(() => ({ ready: !supabase, user: null }))

/**
 * Keeps the store and the API token in step with Supabase. `onSignedIn` runs once after a fresh sign-in
 * (not on page load), which is when anonymous games should be moved to the account.
 */
export function startAuthSync(onSignedIn: () => void): () => void {
  if (!supabase) return () => undefined

  const apply = (session: { access_token: string; user: { id: string; email?: string; user_metadata?: Record<string, unknown> } } | null) => {
    setAccessToken(session?.access_token ?? null)
    const metadata = session?.user.user_metadata
    const name = typeof metadata?.full_name === 'string' ? metadata.full_name : undefined
    useAuthStore.setState({
      ready: true,
      user: session ? { id: session.user.id, email: session.user.email, name } : null,
    })
  }

  void supabase.auth.getSession().then(({ data }) => apply(data.session))
  const { data } = supabase.auth.onAuthStateChange((event, session) => {
    apply(session)
    if (event === 'SIGNED_IN') onSignedIn()
  })
  return () => data.subscription.unsubscribe()
}

export async function signInWithProvider(provider: string): Promise<void> {
  await supabase?.auth.signInWithOAuth({ provider: provider as Provider, options: { redirectTo: window.location.href } })
}

export async function signInWithEmail(email: string): Promise<string | null> {
  const result = await supabase?.auth.signInWithOtp({ email, options: { emailRedirectTo: window.location.href } })
  return result?.error?.message ?? null
}

export async function signOut(): Promise<void> {
  await supabase?.auth.signOut()
}
