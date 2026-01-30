<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DcSpinner, DcAlert, DcCard } from '@dc-platform/ui-kit'
import type { WorkspaceResponse, AuditEntryResponse } from '@dc-platform/shared-types'
import { getWorkspacesByOrganization, getAuditEntries } from '@dc-platform/api-client'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import WorkspaceCard from '@/components/WorkspaceCard.vue'
import ActivityFeed from '@/components/ActivityFeed.vue'
import QuickActions from '@/components/QuickActions.vue'

const client = useApiClient()
const workspaces = ref<WorkspaceResponse[]>([])
const recentActivity = ref<AuditEntryResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const userName = ref<string>('User')

function getOidcUser(): { name: string; email: string; sub: string } | null {
  const storageKey = Object.keys(sessionStorage).find(k => k.startsWith('oidc.user:'))
  if (!storageKey) return null
  try {
    const userData = JSON.parse(sessionStorage.getItem(storageKey) || '{}')
    const profile = userData.profile || {}
    return {
      name: profile.preferred_username || profile.name || profile.sub || 'User',
      email: profile.email || '',
      sub: profile.sub || '',
    }
  } catch {
    return null
  }
}

onMounted(async () => {
  const user = getOidcUser()
  if (user) {
    userName.value = user.name
  }

  const orgId = sessionStorage.getItem('organizationId')
  if (!orgId) {
    error.value = 'No organization selected'
    loading.value = false
    return
  }

  try {
    const [workspacesData, auditData] = await Promise.all([
      getWorkspacesByOrganization(client, orgId),
      getAuditEntries(client, { organizationId: orgId, take: 10 }),
    ])
    workspaces.value = workspacesData
    recentActivity.value = auditData.items || []
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load dashboard'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="p-6">
    <PageHeader :title="`Welcome back, ${userName}`" description="Your workspace overview" />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <div class="space-y-6">
        <section>
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
          <QuickActions />
        </section>

        <section>
          <h2 class="text-lg font-semibold text-gray-900 mb-4">My Workspaces</h2>
          <div v-if="workspaces.length === 0" class="bg-white border border-gray-200 rounded-lg p-8">
            <p class="text-sm text-gray-500 text-center">No workspaces found</p>
          </div>
          <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <WorkspaceCard v-for="workspace in workspaces" :key="workspace.id" :workspace="workspace" />
          </div>
        </section>

        <section>
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Recent Activity</h2>
          <DcCard>
            <ActivityFeed :entries="recentActivity" />
          </DcCard>
        </section>
      </div>
    </template>
  </div>
</template>
