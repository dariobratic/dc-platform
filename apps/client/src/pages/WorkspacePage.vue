<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { DcSpinner, DcAlert, DcCard, DcBadge } from '@dc-platform/ui-kit'
import type { WorkspaceResponse, MembershipResponse } from '@dc-platform/shared-types'
import { getWorkspace, getWorkspaceMembers } from '@dc-platform/api-client'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'

const route = useRoute()
const client = useApiClient()
const workspace = ref<WorkspaceResponse | null>(null)
const members = ref<MembershipResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const activeTab = ref<'overview' | 'members'>('overview')

const workspaceId = computed(() => route.params.id as string)

onMounted(async () => {
  try {
    const [workspaceData, membersData] = await Promise.all([
      getWorkspace(client, workspaceId.value),
      getWorkspaceMembers(client, workspaceId.value),
    ])
    workspace.value = workspaceData
    members.value = membersData
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load workspace'
  } finally {
    loading.value = false
  }
})

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}
</script>

<template>
  <div class="p-6">
    <PageHeader
      :title="workspace?.name ?? 'Workspace'"
      :description="workspace ? `Slug: ${workspace.slug}` : undefined"
    />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else-if="workspace">
      <div class="border-b border-gray-200 mb-6">
        <nav class="-mb-px flex space-x-8">
          <button
            class="border-b-2 py-2 px-1 text-sm font-medium transition-colors"
            :class="activeTab === 'overview' ? 'border-indigo-500 text-indigo-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
            @click="activeTab = 'overview'"
          >
            Overview
          </button>
          <button
            class="border-b-2 py-2 px-1 text-sm font-medium transition-colors"
            :class="activeTab === 'members' ? 'border-indigo-500 text-indigo-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
            @click="activeTab = 'members'"
          >
            Members
          </button>
        </nav>
      </div>

      <div v-if="activeTab === 'overview'" class="space-y-6">
        <DcCard>
          <h3 class="text-base font-semibold text-gray-900 mb-4">Workspace Details</h3>
          <dl class="space-y-3">
            <div>
              <dt class="text-sm font-medium text-gray-500">Name</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ workspace.name }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Slug</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ workspace.slug }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Status</dt>
              <dd class="mt-1">
                <DcBadge
                  :variant="workspace.status === 'Active' ? 'success' : workspace.status === 'Suspended' ? 'warning' : 'error'"
                  size="sm"
                  dot
                >
                  {{ workspace.status }}
                </DcBadge>
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Created</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ formatDate(workspace.createdAt) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Members</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ members.length }}</dd>
            </div>
          </dl>
        </DcCard>

        <DcCard>
          <h3 class="text-base font-semibold text-gray-900 mb-4">Content Management</h3>
          <p class="text-sm text-gray-500">
            Document and task management features coming soon.
          </p>
        </DcCard>
      </div>

      <div v-if="activeTab === 'members'">
        <DcCard>
          <h3 class="text-base font-semibold text-gray-900 mb-4">Workspace Members</h3>
          <div v-if="members.length === 0" class="text-center py-8">
            <p class="text-sm text-gray-500">No members found</p>
          </div>
          <div v-else class="overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
              <thead>
                <tr>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    User ID
                  </th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Role
                  </th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Joined
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-for="member in members" :key="member.id" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900">{{ member.userId }}</td>
                  <td class="px-4 py-3 text-sm">
                    <DcBadge variant="info" size="sm">{{ member.role }}</DcBadge>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-500">{{ formatDate(member.joinedAt) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </DcCard>
      </div>
    </template>
  </div>
</template>
