<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { DcSpinner, DcAlert, DcButton, DcInput, DcModal } from '@dc-platform/ui-kit'
import { getOrganizations, createOrganization, deleteOrganization } from '@dc-platform/api-client'
import type { OrganizationSummary, CreateOrganizationRequest } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import EmptyState from '@/components/EmptyState.vue'

const client = useApiClient()
const router = useRouter()
const organizations = ref<OrganizationSummary[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const searchQuery = ref('')

const showCreateModal = ref(false)
const createForm = ref<CreateOrganizationRequest>({ name: '', slug: '' })
const createLoading = ref(false)
const createError = ref<string | null>(null)

const showDeleteDialog = ref(false)
const deleteTarget = ref<OrganizationSummary | null>(null)
const deleteLoading = ref(false)

const filteredOrganizations = computed(() => {
  if (!searchQuery.value) return organizations.value
  const query = searchQuery.value.toLowerCase()
  return organizations.value.filter(org =>
    org.name.toLowerCase().includes(query) ||
    org.slug.toLowerCase().includes(query)
  )
})

onMounted(async () => {
  await loadOrganizations()
})

async function loadOrganizations() {
  loading.value = true
  error.value = null
  try {
    organizations.value = await getOrganizations(client)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load organizations'
  } finally {
    loading.value = false
  }
}

async function handleCreate() {
  createLoading.value = true
  createError.value = null
  try {
    await createOrganization(client, createForm.value)
    showCreateModal.value = false
    createForm.value = { name: '', slug: '' }
    await loadOrganizations()
  } catch (e) {
    createError.value = e instanceof Error ? e.message : 'Failed to create organization'
  } finally {
    createLoading.value = false
  }
}

function openDeleteDialog(org: OrganizationSummary) {
  deleteTarget.value = org
  showDeleteDialog.value = true
}

async function handleDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await deleteOrganization(client, deleteTarget.value.id)
    showDeleteDialog.value = false
    deleteTarget.value = null
    await loadOrganizations()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to delete organization'
  } finally {
    deleteLoading.value = false
  }
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}
</script>

<template>
  <div class="p-6">
    <PageHeader title="Organizations" description="Manage platform organizations">
      <template #actions>
        <DcButton @click="showCreateModal = true">
          Create Organization
        </DcButton>
      </template>
    </PageHeader>

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <div class="mb-4">
        <DcInput
          v-model="searchQuery"
          placeholder="Search by name or slug..."
          class="max-w-md"
        />
      </div>

      <div v-if="filteredOrganizations.length === 0">
        <EmptyState
          title="No organizations found"
          description="Get started by creating your first organization."
        >
          <template #action>
            <DcButton @click="showCreateModal = true">
              Create Organization
            </DcButton>
          </template>
        </EmptyState>
      </div>

      <div v-else class="overflow-hidden border border-gray-200 rounded-lg bg-white">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Slug
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Created
              </th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="org in filteredOrganizations" :key="org.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="font-medium text-gray-900">{{ org.name }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ org.slug }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <StatusBadge :status="org.status" />
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(org.createdAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                <DcButton
                  size="sm"
                  variant="secondary"
                  @click="router.push(`/admin/organizations/${org.id}`)"
                >
                  View
                </DcButton>
                <DcButton
                  size="sm"
                  variant="danger"
                  @click="openDeleteDialog(org)"
                >
                  Delete
                </DcButton>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <DcModal
      :open="showCreateModal"
      title="Create Organization"
      @close="showCreateModal = false"
    >
      <div class="space-y-4">
        <DcAlert v-if="createError" variant="error" :title="createError" />
        <DcInput
          v-model="createForm.name"
          label="Name"
          placeholder="Organization name"
        />
        <DcInput
          v-model="createForm.slug"
          label="Slug"
          placeholder="organization-slug"
        />
      </div>
      <template #footer>
        <div class="flex justify-end space-x-3">
          <DcButton variant="secondary" @click="showCreateModal = false">
            Cancel
          </DcButton>
          <DcButton :loading="createLoading" @click="handleCreate">
            Create
          </DcButton>
        </div>
      </template>
    </DcModal>

    <ConfirmDialog
      :open="showDeleteDialog"
      title="Delete Organization"
      :message="`Are you sure you want to delete ${deleteTarget?.name}? This action cannot be undone.`"
      confirm-label="Delete"
      variant="danger"
      :loading="deleteLoading"
      @confirm="handleDelete"
      @cancel="showDeleteDialog = false"
    />
  </div>
</template>
