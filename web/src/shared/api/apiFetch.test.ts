import { afterEach, describe, expect, it, vi } from 'vitest'
import { setAccessToken } from '@/shared/auth/accessToken'
import { ApiError, apiFetch } from './apiFetch'

function respond(body: unknown, status = 200) {
  return vi.fn().mockResolvedValue(
    new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }),
  )
}

describe('apiFetch', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    setAccessToken(null)
  })

  it('always sends the anonymous player key', async () => {
    const fetchMock = respond({ ok: true })
    vi.stubGlobal('fetch', fetchMock)

    await apiFetch('/api/health')

    const headers = (fetchMock.mock.calls[0][1] as RequestInit).headers as Record<string, string>
    expect(headers['X-Player-Key']).toMatch(/^[0-9a-f-]{36}$/)
    expect(headers.Authorization).toBeUndefined()
  })

  it('adds the bearer token once signed in', async () => {
    const fetchMock = respond({ ok: true })
    vi.stubGlobal('fetch', fetchMock)
    setAccessToken('abc')

    await apiFetch('/api/me')

    const headers = (fetchMock.mock.calls[0][1] as RequestInit).headers as Record<string, string>
    expect(headers.Authorization).toBe('Bearer abc')
  })

  it('turns problem responses into ApiError with the server message', async () => {
    vi.stubGlobal('fetch', respond({ title: 'Finish the previous rounds first.' }, 409))

    await expect(apiFetch('/x')).rejects.toMatchObject({ status: 409, message: 'Finish the previous rounds first.' })
    await expect(apiFetch('/x')).rejects.toBeInstanceOf(ApiError)
  })
})
