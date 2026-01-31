import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Organization, Workspace } from '@/types'

export const useTenantStore = defineStore('tenant', () => {
  const organizationId = ref<string | null>(null)
  const workspaceId = ref<string | null>(null)
  const currentOrganization = ref<Organization | null>(null)
  const currentWorkspace = ref<Workspace | null>(null)

  const hasOrganization = computed(() => !!organizationId.value)
  const hasWorkspace = computed(() => !!workspaceId.value)

  function setOrganization(id: string, organization?: Organization): void {
    organizationId.value = id
    workspaceId.value = null
    currentWorkspace.value = null
    if (organization) {
      currentOrganization.value = organization
    }
  }

  function setWorkspace(id: string, workspace?: Workspace): void {
    if (!organizationId.value) {
      throw new Error('Cannot set workspace without organization')
    }
    workspaceId.value = id
    if (workspace) {
      currentWorkspace.value = workspace
    }
  }

  function clearOrganization(): void {
    organizationId.value = null
    workspaceId.value = null
    currentOrganization.value = null
    currentWorkspace.value = null
  }

  function clearWorkspace(): void {
    workspaceId.value = null
    currentWorkspace.value = null
  }

  return {
    organizationId,
    workspaceId,
    currentOrganization,
    currentWorkspace,
    hasOrganization,
    hasWorkspace,
    setOrganization,
    setWorkspace,
    clearOrganization,
    clearWorkspace,
  }
})
