const STORAGE_KEY = 'songthesame.playerKey'
const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

/** Anonymous player id, created once and kept in localStorage so a player keeps their daily progress. */
export function getPlayerKey(): string {
  try {
    const existing = localStorage.getItem(STORAGE_KEY)
    if (existing && GUID_PATTERN.test(existing)) return existing
  } catch {
    // Storage can be blocked (private mode); fall through to a session-only id.
  }

  const created = crypto.randomUUID()
  try {
    localStorage.setItem(STORAGE_KEY, created)
  } catch {
    // Ignore: the id simply lasts until the page is closed.
  }
  return created
}
