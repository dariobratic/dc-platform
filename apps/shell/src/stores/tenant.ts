import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Organization } from '@/types'

export const useTenantStore = defineStore('tenant', () => {
  const organizationId = ref<string | null>(null)
  const workspaceId = ref<string | null>(null)
  const currentOrganization = ref<Organization | null>(null)

  const hasOrganization = computed(() => !!organizationId.value)
  const hasWorkspace = computed(() => !!workspaceId.value)

  function setOrganization(id: string, organization?: Organization): void {
    organizationId.value = id
    workspaceId.value = null
    if (organization) {
      currentOrganization.value = organization
    }
  }

  function setWorkspace(id: string): void {
    if (!organizationId.value) {
      throw new Error('Cannot set workspace without organization')
    }
    workspaceId.value = id
  }

  function clearOrganization(): void {
    organizationId.value = null
    workspaceId.value = null
    currentOrganization.value = null
  }

  function clearWorkspace(): void {
    workspaceId.value = null
  }

  return {
    organizationId,
    workspaceId,
    currentOrganization,
    hasOrganization,
    hasWorkspace,
    setOrganization,
    setWorkspace,
    clearOrganization,
    clearWorkspace,
  }
})
