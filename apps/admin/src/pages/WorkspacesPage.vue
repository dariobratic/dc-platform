<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { DcSpinner, DcAlert } from '@dc-platform/ui-kit'
import { getOrganizations, getWorkspacesByOrganization } from '@dc-platform/api-client'
import type { OrganizationSummary, WorkspaceResponse } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import EmptyState from '@/components/EmptyState.vue'

const client = useApiClient()
const organizations = ref<OrganizationSummary[]>([])
const allWorkspaces = ref<Array<WorkspaceResponse & { organizationName: string }>>([])
const loading = ref(true)
const error = ref<string | null>(null)
const selectedOrgId = ref<string>('')

const filteredWorkspaces = computed(() => {
  if (!selectedOrgId.value) return allWorkspaces.value
  return allWorkspaces.value.filter(ws => ws.organizationId === selectedOrgId.value)
})

onMounted(async () => {
  await loadData()
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const orgs = await getOrganizations(client)
    organizations.value = orgs

    const workspacesPromises = orgs.map(async (org) => {
      const workspaces = await getWorkspacesByOrganization(client, org.id)
      return workspaces.map(ws => ({ ...ws, organizationName: org.name }))
    })

    const workspacesArrays = await Promise.all(workspacesPromises)
    allWorkspaces.value = workspacesArrays.flat()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load workspaces'
  } finally {
    loading.value = false
  }
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}
</script>

<template>
  <div class="p-6">
    <PageHeader title="Workspaces" description="View all workspaces across organizations" />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-1">Filter by Organization</label>
        <select
          v-model="selectedOrgId"
          class="block w-full max-w-xs rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-0"
        >
          <option value="">All Organizations</option>
          <option v-for="org in organizations" :key="org.id" :value="org.id">
            {{ org.name }}
          </option>
        </select>
      </div>

      <div v-if="filteredWorkspaces.length === 0">
        <EmptyState
          title="No workspaces found"
          description="Workspaces are created within organizations."
        />
      </div>

      <div v-else class="overflow-hidden border border-gray-200 rounded-lg bg-white">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Organization
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Created
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="workspace in filteredWorkspaces" :key="workspace.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="font-medium text-gray-900">{{ workspace.name }}</div>
                <div class="text-sm text-gray-500">{{ workspace.slug }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ workspace.organizationName }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <StatusBadge :status="workspace.status" />
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(workspace.createdAt) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>
