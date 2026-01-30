<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DcSpinner, DcAlert, DcButton, DcInput, DcModal } from '@dc-platform/ui-kit'
import { getRolesByScope, createRole, deleteRole } from '@dc-platform/api-client'
import { getOrganizations } from '@dc-platform/api-client'
import type { RoleResponse, CreateRoleRequest, OrganizationSummary } from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

const client = useApiClient()
const roles = ref<RoleResponse[]>([])
const organizations = ref<OrganizationSummary[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const showCreateModal = ref(false)
const createForm = ref<CreateRoleRequest>({
  name: '',
  description: '',
  scopeId: '',
  scopeType: 'Organization',
  permissions: [],
})
const permissionInput = ref('')
const createLoading = ref(false)
const createError = ref<string | null>(null)

const showDeleteDialog = ref(false)
const deleteTarget = ref<RoleResponse | null>(null)
const deleteLoading = ref(false)

onMounted(async () => {
  await loadData()
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const orgs = await getOrganizations(client)
    organizations.value = orgs

    if (orgs.length > 0) {
      const rolesPromises = orgs.map(org => getRolesByScope(client, org.id, 'Organization'))
      const rolesArrays = await Promise.all(rolesPromises)
      roles.value = rolesArrays.flat()
    }
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load roles'
  } finally {
    loading.value = false
  }
}

function addPermission() {
  if (permissionInput.value && !createForm.value.permissions.includes(permissionInput.value)) {
    createForm.value.permissions.push(permissionInput.value)
    permissionInput.value = ''
  }
}

function removePermission(permission: string) {
  createForm.value.permissions = createForm.value.permissions.filter(p => p !== permission)
}

async function handleCreate() {
  createLoading.value = true
  createError.value = null
  try {
    await createRole(client, createForm.value)
    showCreateModal.value = false
    createForm.value = {
      name: '',
      description: '',
      scopeId: '',
      scopeType: 'Organization',
      permissions: [],
    }
    await loadData()
  } catch (e) {
    createError.value = e instanceof Error ? e.message : 'Failed to create role'
  } finally {
    createLoading.value = false
  }
}

function openDeleteDialog(role: RoleResponse) {
  deleteTarget.value = role
  showDeleteDialog.value = true
}

async function handleDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await deleteRole(client, deleteTarget.value.id)
    showDeleteDialog.value = false
    deleteTarget.value = null
    await loadData()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to delete role'
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
    <PageHeader title="Roles & Permissions" description="Manage access control roles">
      <template #actions>
        <DcButton @click="showCreateModal = true">
          Create Role
        </DcButton>
      </template>
    </PageHeader>

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else>
      <div v-if="roles.length === 0">
        <EmptyState
          title="No roles found"
          description="Create roles to manage access control across organizations."
        >
          <template #action>
            <DcButton @click="showCreateModal = true">
              Create Role
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
                Description
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Scope
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Permissions
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
            <tr v-for="role in roles" :key="role.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="font-medium text-gray-900">{{ role.name }}</div>
              </td>
              <td class="px-6 py-4">
                <div class="text-sm text-gray-500">{{ role.description || '—' }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ role.scopeType }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="text-sm text-gray-900">{{ role.permissions.length }} permissions</span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(role.createdAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                <DcButton
                  v-if="!role.isSystem"
                  size="sm"
                  variant="danger"
                  @click="openDeleteDialog(role)"
                >
                  Delete
                </DcButton>
                <span v-else class="text-xs text-gray-400">System Role</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <DcModal
      :open="showCreateModal"
      title="Create Role"
      @close="showCreateModal = false"
    >
      <div class="space-y-4">
        <DcAlert v-if="createError" variant="error" :title="createError" />
        <DcInput
          v-model="createForm.name"
          label="Name"
          placeholder="Role name"
        />
        <DcInput
          v-model="createForm.description"
          label="Description"
          placeholder="Role description (optional)"
        />
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Organization</label>
          <select
            v-model="createForm.scopeId"
            class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-0"
          >
            <option value="">Select organization</option>
            <option v-for="org in organizations" :key="org.id" :value="org.id">
              {{ org.name }}
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Permissions</label>
          <div class="flex gap-2 mb-2">
            <DcInput
              v-model="permissionInput"
              placeholder="e.g., document:write"
              class="flex-1"
              @keydown.enter.prevent="addPermission"
            />
            <DcButton type="button" variant="secondary" @click="addPermission">
              Add
            </DcButton>
          </div>
          <div v-if="createForm.permissions.length > 0" class="flex flex-wrap gap-2">
            <span
              v-for="permission in createForm.permissions"
              :key="permission"
              class="inline-flex items-center gap-1 px-2 py-1 text-xs bg-indigo-100 text-indigo-800 rounded"
            >
              {{ permission }}
              <button type="button" @click="removePermission(permission)" class="hover:text-indigo-600">
                ×
              </button>
            </span>
          </div>
        </div>
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
      title="Delete Role"
      :message="`Are you sure you want to delete ${deleteTarget?.name}? This action cannot be undone.`"
      confirm-label="Delete"
      variant="danger"
      :loading="deleteLoading"
      @confirm="handleDelete"
      @cancel="showDeleteDialog = false"
    />
  </div>
</template>
