import { describe, it, expect } from 'vitest'

function hasPermission(permissions: string[], perm: string): boolean {
  return permissions.includes(perm)
}

describe('hasPermission', () => {
  it('returns true when permission exists', () => {
    expect(hasPermission(['user:list', 'user:create'], 'user:list')).toBe(true)
  })

  it('returns false when permission does not exist', () => {
    expect(hasPermission(['user:list'], 'user:delete')).toBe(false)
  })
})
