<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { DcSpinner, DcAlert, DcButton, DcInput, DcModal, DcCard, DcBadge, DcSelect } from '@dc-platform/ui-kit'
import {
  getWorkspace,
  getWorkspaceMembers,
  addMember,
  changeMemberRole,
  removeMember,
  updateWorkspace,
  deleteWorkspace,
} from '@dc-platform/api-client'
import type {
  WorkspaceResponse,
  MembershipResponse,
  WorkspaceRole,
} from '@dc-platform/shared-types'
import { useApiClient } from '@/composables/useApiClient'
import PageHeader from '@/components/PageHeader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

const route = useRoute()
const router = useRouter()
const client = useApiClient()

const workspaceId = computed(() => route.params.id as string)
const workspace = ref<WorkspaceResponse | null>(null)
const members = ref<MembershipResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const activeTab = ref<'overview' | 'members' | 'settings'>('overview')

// Add member modal
const showAddMemberModal = ref(false)
const addMemberForm = ref({ userId: '', role: 'Member' as WorkspaceRole })
const addMemberLoading = ref(false)
const addMemberError = ref<string | null>(null)

// Remove member
const removeMemberTarget = ref<MembershipResponse | null>(null)
const removeMemberLoading = ref(false)

// Settings - rename
const renameName = ref('')
const renameLoading = ref(false)
const renameError = ref<string | null>(null)
const renameSuccess = ref(false)

// Settings - delete
const showDeleteConfirm = ref(false)
const deleteLoading = ref(false)

const roleOptions = [
  { value: 'Owner', label: 'Owner' },
  { value: 'Admin', label: 'Admin' },
  { value: 'Member', label: 'Member' },
  { value: 'Viewer', label: 'Viewer' },
]

onMounted(async () => {
  await loadWorkspace()
})

async function loadWorkspace() {
  loading.value = true
  error.value = null
  try {
    const [workspaceData, membersData] = await Promise.all([
      getWorkspace(client, workspaceId.value),
      getWorkspaceMembers(client, workspaceId.value),
    ])
    workspace.value = workspaceData
    members.value = membersData
    renameName.value = workspaceData.name
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load workspace'
  } finally {
    loading.value = false
  }
}

async function handleAddMember() {
  if (!addMemberForm.value.userId.trim()) return
  addMemberLoading.value = true
  addMemberError.value = null
  try {
    await addMember(client, workspaceId.value, {
      userId: addMemberForm.value.userId.trim(),
      role: addMemberForm.value.role,
    })
    showAddMemberModal.value = false
    addMemberForm.value = { userId: '', role: 'Member' }
    members.value = await getWorkspaceMembers(client, workspaceId.value)
  } catch (e) {
    addMemberError.value = e instanceof Error ? e.message : 'Failed to add member'
  } finally {
    addMemberLoading.value = false
  }
}

async function handleChangeRole(userId: string, newRole: WorkspaceRole) {
  try {
    await changeMemberRole(client, workspaceId.value, userId, { role: newRole })
    members.value = await getWorkspaceMembers(client, workspaceId.value)
  } catch {
    members.value = await getWorkspaceMembers(client, workspaceId.value)
  }
}

async function handleRemoveMember() {
  if (!removeMemberTarget.value) return
  removeMemberLoading.value = true
  try {
    await removeMember(client, workspaceId.value, removeMemberTarget.value.userId)
    removeMemberTarget.value = null
    members.value = await getWorkspaceMembers(client, workspaceId.value)
  } catch {
    // Handled via reload
  } finally {
    removeMemberLoading.value = false
  }
}

async function handleRename() {
  if (!renameName.value.trim()) return
  renameLoading.value = true
  renameError.value = null
  renameSuccess.value = false
  try {
    workspace.value = await updateWorkspace(client, workspaceId.value, { name: renameName.value.trim() })
    renameSuccess.value = true
  } catch (e) {
    renameError.value = e instanceof Error ? e.message : 'Failed to rename workspace'
  } finally {
    renameLoading.value = false
  }
}

async function handleDelete() {
  deleteLoading.value = true
  try {
    await deleteWorkspace(client, workspaceId.value)
    const orgId = workspace.value?.organizationId
    if (orgId) {
      router.push(`/admin/organizations/${orgId}`)
    } else {
      router.push('/admin/workspaces')
    }
  } catch {
    deleteLoading.value = false
  }
}

function roleBadgeVariant(role: string): 'warning' | 'info' | 'success' | 'default' {
  switch (role) {
    case 'Owner': return 'warning'
    case 'Admin': return 'info'
    case 'Member': return 'success'
    default: return 'default'
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
      :title="workspace?.name ?? 'Workspace'"
      :description="workspace ? `Slug: ${workspace.slug}` : undefined"
    />

    <DcSpinner v-if="loading" class="mx-auto mt-12" size="lg" />
    <DcAlert v-else-if="error" variant="error" :title="error" />
    <template v-else-if="workspace">
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
              activeTab === 'members'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
            @click="activeTab = 'members'"
          >
            Members ({{ members.length }})
          </button>
          <button
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'settings'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
            @click="activeTab = 'settings'"
          >
            Settings
          </button>
        </nav>
      </div>

      <!-- Overview Tab -->
      <div v-if="activeTab === 'overview'" class="space-y-6">
        <DcCard>
          <template #header>
            <h2 class="text-lg font-semibold text-gray-900">Workspace Details</h2>
          </template>
          <dl class="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
                <StatusBadge :status="workspace.status" />
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-gray-500">Organization</dt>
              <dd class="mt-1">
                <router-link
                  :to="`/admin/organizations/${workspace.organizationId}`"
                  class="text-sm text-indigo-600 hover:text-indigo-800"
                >
                  {{ workspace.organizationId }}
                </router-link>
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
      </div>

      <!-- Members Tab -->
      <div v-if="activeTab === 'members'">
        <div class="mb-4 flex justify-end">
          <DcButton @click="showAddMemberModal = true">
            Add Member
          </DcButton>
        </div>

        <DcCard>
          <div v-if="members.length === 0" class="text-center py-8">
            <p class="text-sm text-gray-500">No members in this workspace</p>
          </div>
          <div v-else class="overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User ID</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Joined</th>
                  <th class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="member in members" :key="member.id" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900 font-mono">{{ member.userId }}</td>
                  <td class="px-4 py-3 text-sm">
                    <DcBadge :variant="roleBadgeVariant(member.role)" size="sm">
                      {{ member.role }}
                    </DcBadge>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-500">{{ formatDate(member.joinedAt) }}</td>
                  <td class="px-4 py-3 text-right">
                    <div class="flex items-center justify-end space-x-2">
                      <select
                        :value="member.role"
                        @change="handleChangeRole(member.userId, ($event.target as HTMLSelectElement).value as WorkspaceRole)"
                        class="text-xs border border-gray-300 rounded px-2 py-1 focus:ring-1 focus:ring-indigo-500 focus:outline-none"
                      >
                        <option v-for="opt in roleOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
                      </select>
                      <DcButton
                        variant="danger"
                        size="sm"
                        @click="removeMemberTarget = member"
                      >
                        Remove
                      </DcButton>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </DcCard>
      </div>

      <!-- Settings Tab -->
      <div v-if="activeTab === 'settings'" class="space-y-6">
        <DcCard>
          <template #header>
            <h2 class="text-lg font-semibold text-gray-900">Rename Workspace</h2>
          </template>
          <DcAlert v-if="renameError" variant="error" :title="renameError" class="mb-4" />
          <DcAlert v-if="renameSuccess" variant="success" title="Workspace renamed successfully" class="mb-4" />
          <div class="flex items-end space-x-3">
            <div class="flex-1">
              <DcInput v-model="renameName" label="Workspace Name" :disabled="renameLoading" />
            </div>
            <DcButton :loading="renameLoading" @click="handleRename">Save</DcButton>
          </div>
        </DcCard>

        <DcCard>
          <template #header>
            <h2 class="text-lg font-semibold text-red-600">Danger Zone</h2>
          </template>
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-900">Delete this workspace</p>
              <p class="text-sm text-gray-500">Once deleted, this workspace and all its data cannot be recovered.</p>
            </div>
            <DcButton variant="danger" @click="showDeleteConfirm = true">
              Delete Workspace
            </DcButton>
          </div>
        </DcCard>
      </div>
    </template>

    <!-- Add Member Modal -->
    <DcModal
      :open="showAddMemberModal"
      title="Add Member"
      @close="showAddMemberModal = false"
    >
      <div class="space-y-4">
        <DcAlert v-if="addMemberError" variant="error" :title="addMemberError" />
        <DcInput
          v-model="addMemberForm.userId"
          label="User ID"
          placeholder="Enter user ID"
        />
        <DcSelect
          v-model="addMemberForm.role"
          :options="roleOptions"
          label="Role"
        />
      </div>
      <template #footer>
        <div class="flex justify-end space-x-3">
          <DcButton variant="secondary" @click="showAddMemberModal = false">
            Cancel
          </DcButton>
          <DcButton :loading="addMemberLoading" @click="handleAddMember">
            Add Member
          </DcButton>
        </div>
      </template>
    </DcModal>

    <!-- Remove Member Confirm -->
    <ConfirmDialog
      :open="!!removeMemberTarget"
      title="Remove Member"
      message="Are you sure you want to remove this member from the workspace?"
      confirm-label="Remove"
      variant="danger"
      :loading="removeMemberLoading"
      @confirm="handleRemoveMember"
      @cancel="removeMemberTarget = null"
    />

    <!-- Delete Workspace Confirm -->
    <ConfirmDialog
      :open="showDeleteConfirm"
      title="Delete Workspace"
      :message="`Are you sure you want to delete '${workspace?.name}'? This action cannot be undone.`"
      confirm-label="Delete"
      variant="danger"
      :loading="deleteLoading"
      @confirm="handleDelete"
      @cancel="showDeleteConfirm = false"
    />
  </div>
</template>
