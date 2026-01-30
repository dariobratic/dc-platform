<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { DcSpinner, DcAlert, DcButton, DcInput, DcModal, DcCard } from '@dc-platform/ui-kit'
import { getOrganization, getWorkspacesByOrganization, createWorkspace } from '@dc-platform/api-client'
import type { OrganizationResponse, WorkspaceResponse, CreateWorkspaceRequest } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const route = useRoute()
const client = useApiClient()

const orgId = computed(() => route.params.id as string)
const organization = ref<OrganizationResponse | null>(null)
const workspaces = ref<WorkspaceResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const activeTab = ref<'overview' | 'workspaces' | 'members'>('overview')

const showCreateWorkspaceModal = ref(false)
const createWorkspaceForm = ref<CreateWorkspaceRequest>({ name: '', slug: '' })
const createWorkspaceLoading = ref(false)
const createWorkspaceError = ref<string | null>(null)

onMounted(async () => {
  await loadOrganization()
})

async function loadOrganization() {
  loading.value = true
  error.value = null
  try {
    const [orgData, workspacesData] = await Promise.all([
      getOrganization(client, orgId.value),
      getWorkspacesByOrganization(client, orgId.value),
    ])
    organization.value = orgData
    workspaces.value = workspacesData
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load organization'
  } finally {
    loading.value = false
  }
}

async function handleCreateWorkspace() {
  createWorkspaceLoading.value = true
  createWorkspaceError.value = null
  try {
    await createWorkspace(client, orgId.value, createWorkspaceForm.value)
    showCreateWorkspaceModal.value = false
    createWorkspaceForm.value = { name: '', slug: '' }
    await loadOrganization()
  } catch (e) {
    createWorkspaceError.value = e instanceof Error ? e.message : 'Failed to create workspace'
  } finally {
    createWorkspaceLoading.value = false
  }
}

function formatDate(dateString: string | null): string {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString()
}
</script>

<template>
  <div class="p-6">
    <PageHeader
      :title="organization?.name ?? 'Organization'"
      description="View and manage organization details"
    />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else-if="organization">
      <div class="mb-6 border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'overview'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
            @click="activeTab = 'overview'"
          >
            Overview
          </button>
          <button
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'workspaces'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
            @click="activeTab = 'workspaces'"
          >
            Workspaces ({{ workspaces.length }})
          </button>
          <button
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'members'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
            @click="activeTab = 'members'"
          >
            Members
          </button>
        </nav>
      </div>

      <div v-if="activeTab === 'overview'" class="space-y-6">
        <DcCard>
          <template #header>
            <h2 class="text-lg font-semibold text-gray-900">Organization Details</h2>
          </template>
          <dl class="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-gray-500">Name</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ organization.name }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Slug</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ organization.slug }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Status</dt>
              <dd class="mt-1">
                <StatusBadge :status="organization.status" />
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Created</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ formatDate(organization.createdAt) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Updated</dt>
              <dd class="mt-1 text-sm text-gray-900">{{ formatDate(organization.updatedAt) }}</dd>
            </div>
          </dl>
        </DcCard>

        <DcCard v-if="Object.keys(organization.settings).length > 0">
          <template #header>
            <h2 class="text-lg font-semibold text-gray-900">Settings</h2>
          </template>
          <dl class="space-y-2">
            <div v-for="(value, key) in organization.settings" :key="key" class="flex items-center justify-between">
              <dt class="text-sm font-medium text-gray-500">{{ key }}</dt>
              <dd class="text-sm text-gray-900">{{ value }}</dd>
            </div>
          </dl>
        </DcCard>
      </div>

      <div v-if="activeTab === 'workspaces'">
        <div class="mb-4 flex justify-end">
          <DcButton @click="showCreateWorkspaceModal = true">
            Create Workspace
          </DcButton>
        </div>

        <div v-if="workspaces.length === 0" class="text-center py-12">
          <p class="text-gray-500">No workspaces yet</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <DcCard v-for="workspace in workspaces" :key="workspace.id">
            <div class="space-y-2">
              <div class="flex items-start justify-between">
                <div>
                  <h3 class="font-semibold text-gray-900">{{ workspace.name }}</h3>
                  <p class="text-sm text-gray-500">{{ workspace.slug }}</p>
                </div>
                <StatusBadge :status="workspace.status" />
              </div>
              <p class="text-xs text-gray-500">Created {{ formatDate(workspace.createdAt) }}</p>
            </div>
          </DcCard>
        </div>
      </div>

      <div v-if="activeTab === 'members'">
        <p class="text-center py-12 text-gray-500">
          Member management coming soon
        </p>
      </div>
    </template>

    <DcModal
      :open="showCreateWorkspaceModal"
      title="Create Workspace"
      @close="showCreateWorkspaceModal = false"
    >
      <div class="space-y-4">
        <DcAlert v-if="createWorkspaceError" variant="error" :title="createWorkspaceError" />
        <DcInput
          v-model="createWorkspaceForm.name"
          label="Name"
          placeholder="Workspace name"
        />
        <DcInput
          v-model="createWorkspaceForm.slug"
          label="Slug"
          placeholder="workspace-slug"
        />
      </div>
      <template #footer>
        <div class="flex justify-end space-x-3">
          <DcButton variant="secondary" @click="showCreateWorkspaceModal = false">
            Cancel
          </DcButton>
          <DcButton :loading="createWorkspaceLoading" @click="handleCreateWorkspace">
            Create
          </DcButton>
        </div>
      </template>
    </DcModal>
  </div>
</template>
