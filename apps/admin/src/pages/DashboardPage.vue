<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DcSpinner, DcAlert, DcCard } from '@dc-platform/ui-kit'
import { getDashboard, getOrganizations, getRecentAuditEntries } from '@dc-platform/api-client'
import type { DashboardResponse, OrganizationSummary, AuditEntrySummary } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import StatsCard from '@/components/StatsCard.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const client = useApiClient()
const dashboard = ref<DashboardResponse | null>(null)
const recentOrgs = ref<OrganizationSummary[]>([])
const recentAudit = ref<AuditEntrySummary[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    const [dashData, orgsData, auditData] = await Promise.all([
      getDashboard(client),
      getOrganizations(client),
      getRecentAuditEntries(client, 10),
    ])
    dashboard.value = dashData
    recentOrgs.value = orgsData.slice(0, 5)
    recentAudit.value = auditData
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load dashboard data'
  } finally {
    loading.value = false
  }
})

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleString()
}
</script>

<template>
  <div class="p-6">
    <PageHeader title="Admin Dashboard" description="System overview and recent activity" />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <StatsCard
          title="Total Organizations"
          :value="dashboard?.organizationCount ?? 0"
        />
        <StatsCard
          title="Active Users"
          :value="0"
        />
        <StatsCard
          title="Audit Events"
          :value="dashboard?.auditEntryCount ?? 0"
        />
        <StatsCard
          title="System Health"
          value="Healthy"
        />
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <DcCard>
          <template #header>
            <h2 class="text-lg font-semibold text-gray-900">Recent Organizations</h2>
          </template>
          <div class="divide-y divide-gray-200">
            <div
              v-for="org in recentOrgs"
              :key="org.id"
              class="py-3 flex items-center justify-between"
            >
              <div>
                <p class="font-medium text-gray-900">{{ org.name }}</p>
                <p class="text-sm text-gray-500">{{ org.slug }}</p>
              </div>
              <StatusBadge :status="org.status" />
            </div>
            <div v-if="recentOrgs.length === 0" class="py-8 text-center text-gray-500">
              No organizations yet
            </div>
          </div>
        </DcCard>

        <DcCard>
          <template #header>
            <h2 class="text-lg font-semibold text-gray-900">Recent Audit Log</h2>
          </template>
          <div class="divide-y divide-gray-200">
            <div
              v-for="entry in recentAudit"
              :key="entry.id"
              class="py-3"
            >
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <p class="text-sm font-medium text-gray-900">{{ entry.action }}</p>
                  <p class="text-xs text-gray-500">
                    {{ entry.entityType }} {{ entry.entityId ? `· ${entry.entityId.substring(0, 8)}` : '' }}
                  </p>
                </div>
                <p class="text-xs text-gray-500">{{ formatDate(entry.timestamp) }}</p>
              </div>
            </div>
            <div v-if="recentAudit.length === 0" class="py-8 text-center text-gray-500">
              No audit entries yet
            </div>
          </div>
        </DcCard>
      </div>
    </template>
  </div>
</template>
