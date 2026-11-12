import { getAccessToken } from '@/shared/auth/accessToken'
import { getPlayerKey } from './playerKey'

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

/** Custom fetch used by the generated API client (see orval.config.ts). */
export async function apiFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  const token = getAccessToken()
  const response = await fetch(`${baseUrl}${url}`, {
    ...options,
    headers: {
      Accept: 'application/json',
      'X-Player-Key': getPlayerKey(),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })

  const body: unknown = response.status === 204 ? undefined : await readJson(response)
  if (!response.ok) throw new ApiError(response.status, problemTitle(body) ?? `Request to ${url} failed`)

  return { data: body, status: response.status, headers: response.headers } as T
}

async function readJson(response: Response): Promise<unknown> {
  try {
    return await response.json()
  } catch {
    return undefined
  }
}

function problemTitle(body: unknown): string | undefined {
  if (typeof body === 'object' && body !== null && 'title' in body && typeof body.title === 'string') return body.title
  return undefined
}
