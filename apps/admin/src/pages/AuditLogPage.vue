<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DcSpinner, DcAlert, DcInput, DcButton } from '@dc-platform/ui-kit'
import { getAuditEntries } from '@dc-platform/api-client'
import type { AuditQueryParams } from '@dc-platform/api-client'
import type { AuditEntryResponse, PagedResponse } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'

const client = useApiClient()
const auditEntries = ref<AuditEntryResponse[]>([])
const totalCount = ref(0)
const loading = ref(true)
const error = ref<string | null>(null)

const filters = ref<AuditQueryParams>({
  skip: 0,
  take: 50,
})

const entityTypes = ['Organization', 'Workspace', 'Membership', 'Role', 'RoleAssignment']
const serviceNames = ['Directory', 'AccessControl', 'Audit', 'Authentication', 'Gateway']

onMounted(async () => {
  await loadAuditEntries()
})

async function loadAuditEntries() {
  loading.value = true
  error.value = null
  try {
    const response: PagedResponse<AuditEntryResponse> = await getAuditEntries(client, filters.value)
    auditEntries.value = response.items
    totalCount.value = response.totalCount
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load audit entries'
  } finally {
    loading.value = false
  }
}

async function applyFilters() {
  filters.value.skip = 0
  await loadAuditEntries()
}

async function previousPage() {
  if (filters.value.skip! > 0) {
    filters.value.skip = Math.max(0, filters.value.skip! - filters.value.take!)
    await loadAuditEntries()
  }
}

async function nextPage() {
  if (filters.value.skip! + filters.value.take! < totalCount.value) {
    filters.value.skip = filters.value.skip! + filters.value.take!
    await loadAuditEntries()
  }
}

function clearFilters() {
  filters.value = {
    skip: 0,
    take: 50,
  }
  loadAuditEntries()
}

function formatDateTime(dateString: string): string {
  return new Date(dateString).toLocaleString()
}
</script>

<template>
  <div class="p-6">
    <PageHeader
      title="Audit Log"
      description="View platform activity and security events"
    />

    <div class="mb-6 bg-white border border-gray-200 rounded-lg p-4">
      <h3 class="text-sm font-medium text-gray-700 mb-4">Filters</h3>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <DcInput
          v-model="filters.action"
          label="Action"
          placeholder="e.g., organization.created"
        />
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Entity Type</label>
          <select
            v-model="filters.entityType"
            class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-0"
          >
            <option value="">All Types</option>
            <option v-for="type in entityTypes" :key="type" :value="type">
              {{ type }}
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Service</label>
          <select
            v-model="filters.serviceName"
            class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-0"
          >
            <option value="">All Services</option>
            <option v-for="service in serviceNames" :key="service" :value="service">
              {{ service }}
            </option>
          </select>
        </div>
        <DcInput
          v-model="filters.userId"
          label="User ID"
          placeholder="Filter by user"
        />
      </div>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">From Date</label>
          <input
            v-model="filters.from"
            type="date"
            class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-0"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">To Date</label>
          <input
            v-model="filters.to"
            type="date"
            class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-0"
          />
        </div>
      </div>
      <div class="flex justify-end gap-2 mt-4">
        <DcButton variant="secondary" @click="clearFilters">
          Clear Filters
        </DcButton>
        <DcButton @click="applyFilters">
          Apply Filters
        </DcButton>
      </div>
    </div>

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <div v-if="auditEntries.length === 0">
        <EmptyState
          title="No audit entries found"
          description="Try adjusting your filters or check back later."
        />
      </div>

      <div v-else>
        <div class="mb-4 flex items-center justify-between">
          <p class="text-sm text-gray-600">
            Showing {{ filters.skip! + 1 }}-{{ Math.min(filters.skip! + filters.take!, totalCount) }} of {{ totalCount }} entries
          </p>
          <div class="flex gap-2">
            <DcButton
              variant="secondary"
              size="sm"
              :disabled="filters.skip === 0"
              @click="previousPage"
            >
              Previous
            </DcButton>
            <DcButton
              variant="secondary"
              size="sm"
              :disabled="filters.skip! + filters.take! >= totalCount"
              @click="nextPage"
            >
              Next
            </DcButton>
          </div>
        </div>

        <div class="overflow-hidden border border-gray-200 rounded-lg bg-white">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Timestamp
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Action
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Entity
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  User
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Service
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="entry in auditEntries" :key="entry.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatDateTime(entry.timestamp) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm font-medium text-gray-900">{{ entry.action }}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-900">{{ entry.entityType }}</div>
                  <div class="text-xs text-gray-500">{{ entry.entityId.substring(0, 8) }}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-500">{{ entry.userEmail || '—' }}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ entry.serviceName }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="mt-4 flex justify-end">
          <div class="flex gap-2">
            <DcButton
              variant="secondary"
              size="sm"
              :disabled="filters.skip === 0"
              @click="previousPage"
            >
              Previous
            </DcButton>
            <DcButton
              variant="secondary"
              size="sm"
              :disabled="filters.skip! + filters.take! >= totalCount"
              @click="nextPage"
            >
              Next
            </DcButton>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
