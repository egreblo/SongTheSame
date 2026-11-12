let accessToken: string | null = null

/** The signed-in user's Supabase access token, attached to API calls as a Bearer header. */
export function getAccessToken(): string | null {
  return accessToken
}

export function setAccessToken(token: string | null): void {
  accessToken = token
}
