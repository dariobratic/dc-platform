import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useTenantStore } from '../tenant'

describe('tenant store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('has null organizationId initially', () => {
    const store = useTenantStore()
    expect(store.organizationId).toBeNull()
  })

  it('has null workspaceId initially', () => {
    const store = useTenantStore()
    expect(store.workspaceId).toBeNull()
  })

  it('hasOrganization returns false initially', () => {
    const store = useTenantStore()
    expect(store.hasOrganization).toBe(false)
  })

  it('hasWorkspace returns false initially', () => {
    const store = useTenantStore()
    expect(store.hasWorkspace).toBe(false)
  })

  it('setOrganization sets organizationId and clears workspaceId', () => {
    const store = useTenantStore()
    store.setOrganization('org-1')
    expect(store.organizationId).toBe('org-1')
    expect(store.workspaceId).toBeNull()
    expect(store.hasOrganization).toBe(true)
  })

  it('setOrganization stores organization object when provided', () => {
    const store = useTenantStore()
    const org = { id: 'org-1', name: 'Test Org' }
    store.setOrganization('org-1', org as any)
    expect(store.currentOrganization).toEqual(org)
  })

  it('setWorkspace sets workspaceId', () => {
    const store = useTenantStore()
    store.setOrganization('org-1')
    store.setWorkspace('ws-1')
    expect(store.workspaceId).toBe('ws-1')
    expect(store.hasWorkspace).toBe(true)
  })

  it('setWorkspace throws without organization', () => {
    const store = useTenantStore()
    expect(() => store.setWorkspace('ws-1')).toThrow('Cannot set workspace without organization')
  })

  it('clearOrganization clears all state', () => {
    const store = useTenantStore()
    store.setOrganization('org-1', { id: 'org-1', name: 'Org' } as any)
    store.setWorkspace('ws-1')
    store.clearOrganization()

    expect(store.organizationId).toBeNull()
    expect(store.workspaceId).toBeNull()
    expect(store.currentOrganization).toBeNull()
    expect(store.hasOrganization).toBe(false)
    expect(store.hasWorkspace).toBe(false)
  })

  it('clearWorkspace clears only workspace', () => {
    const store = useTenantStore()
    store.setOrganization('org-1')
    store.setWorkspace('ws-1')
    store.clearWorkspace()

    expect(store.organizationId).toBe('org-1')
    expect(store.workspaceId).toBeNull()
    expect(store.hasOrganization).toBe(true)
    expect(store.hasWorkspace).toBe(false)
  })
})
