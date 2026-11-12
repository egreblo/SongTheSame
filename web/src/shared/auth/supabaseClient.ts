import { createClient } from '@supabase/supabase-js'

const url = import.meta.env.VITE_SUPABASE_URL
const anonKey = import.meta.env.VITE_SUPABASE_ANON_KEY

/** Null until VITE_SUPABASE_URL and VITE_SUPABASE_ANON_KEY are set; the game then works without accounts. */
export const supabase = url && anonKey ? createClient(url, anonKey) : null

export const authEnabled = supabase !== null

/** Sign-in providers to offer, from VITE_AUTH_PROVIDERS (comma separated, default "google"). */
export const authProviders: string[] = (import.meta.env.VITE_AUTH_PROVIDERS ?? 'google')
  .split(',')
  .map((provider) => provider.trim().toLowerCase())
  .filter(Boolean)
