import { describe, expect, it } from 'vitest'
import { siteUrl } from './siteUrl'

describe('siteUrl', () => {
  it('builds absolute links to pages of this site', () => {
    expect(siteUrl('g/ABCD2345')).toBe(`${window.location.origin}/g/ABCD2345`)
    expect(siteUrl()).toBe(`${window.location.origin}/`)
  })

  it('ignores a leading slash', () => {
    expect(siteUrl('/leagues')).toBe(`${window.location.origin}/leagues`)
  })
})
