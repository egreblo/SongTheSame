/** Absolute URL of a page of this site, honouring the base path the app is deployed under (e.g. /SongTheSame/). */
export function siteUrl(path = ''): string {
  const base = new URL(import.meta.env.BASE_URL, window.location.origin)
  return new URL(path.replace(/^\//, ''), base).toString()
}
