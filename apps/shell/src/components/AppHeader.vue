<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useTenantStore } from '@/stores/tenant'
import { http } from '@/plugins/http'
import UserMenu from '@/components/UserMenu.vue'
import type { Workspace } from '@/types'

defineEmits<{
  toggleSidebar: []
}>()

const tenantStore = useTenantStore()
const workspaces = ref<Workspace[]>([])
const loadingWorkspaces = ref(false)

const organizationName = computed(() => {
  return tenantStore.currentOrganization?.name ?? 'No Organization'
})

const currentWorkspaceId = computed(() => tenantStore.workspaceId)

async function fetchWorkspaces() {
  const orgId = tenantStore.organizationId
  if (!orgId) {
    workspaces.value = []
    return
  }
  loadingWorkspaces.value = true
  try {
    const { data } = await http.get<Workspace[]>(`/api/v1/organizations/${orgId}/workspaces`)
    workspaces.value = data
  } catch {
    workspaces.value = []
  } finally {
    loadingWorkspaces.value = false
  }
}

function onWorkspaceChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const id = target.value
  if (!id) return
  const workspace = workspaces.value.find(w => w.id === id)
  if (workspace) {
    tenantStore.setWorkspace(id, workspace)
    sessionStorage.setItem('workspaceId', id)
  }
}

onMounted(() => {
  fetchWorkspaces()
})

watch(() => tenantStore.organizationId, () => {
  fetchWorkspaces()
})
</script>

<template>
  <header class="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6">
    <div class="flex items-center space-x-4">
      <button
        @click="$emit('toggleSidebar')"
        class="lg:hidden p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
      >
        <span class="text-xl">☰</span>
      </button>

      <div class="flex items-center space-x-3">
        <div class="flex items-center space-x-2">
          <span class="text-sm font-medium text-gray-700">Organization:</span>
          <div class="px-3 py-1 bg-gray-100 rounded-md text-sm font-medium text-gray-900">
            {{ organizationName }}
          </div>
        </div>

        <div class="h-6 w-px bg-gray-300"></div>

        <div class="flex items-center space-x-2">
          <span class="text-sm font-medium text-gray-700">Workspace:</span>
          <select
            :value="currentWorkspaceId ?? ''"
            @change="onWorkspaceChange"
            :disabled="loadingWorkspaces || workspaces.length === 0"
            class="text-sm font-medium text-gray-900 bg-gray-100 border-0 rounded-md px-3 py-1 focus:ring-2 focus:ring-indigo-500 focus:outline-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <option value="" disabled>Select Workspace</option>
            <option v-for="ws in workspaces" :key="ws.id" :value="ws.id">
              {{ ws.name }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <UserMenu />
  </header>
</template>
