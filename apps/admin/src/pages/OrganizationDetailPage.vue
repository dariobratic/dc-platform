<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { DcSpinner, DcAlert, DcButton, DcInput, DcModal, DcCard, DcBadge, DcSelect } from '@dc-platform/ui-kit'
import {
  getOrganization,
  getWorkspacesByOrganization,
  createWorkspace,
  updateOrganization,
  deleteWorkspace,
  getWorkspaceMembers,
  addMember,
  changeMemberRole,
  removeMember,
} from '@dc-platform/api-client'
import type {
  OrganizationResponse,
  WorkspaceResponse,
  CreateWorkspaceRequest,
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

const orgId = computed(() => route.params.id as string)
const organization = ref<OrganizationResponse | null>(null)
const workspaces = ref<WorkspaceResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const activeTab = ref<'overview' | 'workspaces' | 'members'>('overview')

// Create workspace modal
const showCreateWorkspaceModal = ref(false)
const createWorkspaceForm = ref<CreateWorkspaceRequest>({ name: '', slug: '' })
const createWorkspaceLoading = ref(false)
const createWorkspaceError = ref<string | null>(null)

// Edit org name
const editingName = ref(false)
const editName = ref('')
const editNameLoading = ref(false)
const editNameError = ref<string | null>(null)

// Delete workspace
const deleteWorkspaceTarget = ref<WorkspaceResponse | null>(null)
const deleteWorkspaceLoading = ref(false)

// Members
const workspaceMembers = ref<Record<string, MembershipResponse[]>>({})
const expandedWorkspaces = ref<Set<string>>(new Set())
const membersLoading = ref<Record<string, boolean>>({})

// Add member modal
const showAddMemberModal = ref(false)
const addMemberWorkspaceId = ref<string | null>(null)
const addMemberForm = ref({ userId: '', role: 'Member' as WorkspaceRole })
const addMemberLoading = ref(false)
const addMemberError = ref<string | null>(null)

// Remove member
const removeMemberTarget = ref<{ workspaceId: string; userId: string } | null>(null)
const removeMemberLoading = ref(false)

const roleOptions = [
  { value: 'Owner', label: 'Owner' },
  { value: 'Admin', label: 'Admin' },
  { value: 'Member', label: 'Member' },
  { value: 'Viewer', label: 'Viewer' },
]

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

function startEditName() {
  editName.value = organization.value?.name ?? ''
  editNameError.value = null
  editingName.value = true
}

function cancelEditName() {
  editingName.value = false
  editNameError.value = null
}

async function saveEditName() {
  if (!editName.value.trim()) return
  editNameLoading.value = true
  editNameError.value = null
  try {
    const updated = await updateOrganization(client, orgId.value, { name: editName.value.trim() })
    organization.value = updated
    editingName.value = false
  } catch (e) {
    editNameError.value = e instanceof Error ? e.message : 'Failed to update organization'
  } finally {
    editNameLoading.value = false
  }
}

async function handleDeleteWorkspace() {
  if (!deleteWorkspaceTarget.value) return
  deleteWorkspaceLoading.value = true
  try {
    await deleteWorkspace(client, deleteWorkspaceTarget.value.id)
    deleteWorkspaceTarget.value = null
    await loadOrganization()
  } catch {
    // Error handling via reload
  } finally {
    deleteWorkspaceLoading.value = false
  }
}

async function toggleWorkspaceMembers(workspaceId: string) {
  if (expandedWorkspaces.value.has(workspaceId)) {
    expandedWorkspaces.value.delete(workspaceId)
    return
  }
  expandedWorkspaces.value.add(workspaceId)
  if (!workspaceMembers.value[workspaceId]) {
    await loadWorkspaceMembers(workspaceId)
  }
}

async function loadWorkspaceMembers(workspaceId: string) {
  membersLoading.value[workspaceId] = true
  try {
    workspaceMembers.value[workspaceId] = await getWorkspaceMembers(client, workspaceId)
  } catch {
    workspaceMembers.value[workspaceId] = []
  } finally {
    membersLoading.value[workspaceId] = false
  }
}

function openAddMemberModal(workspaceId: string) {
  addMemberWorkspaceId.value = workspaceId
  addMemberForm.value = { userId: '', role: 'Member' }
  addMemberError.value = null
  showAddMemberModal.value = true
}

async function handleAddMember() {
  if (!addMemberWorkspaceId.value || !addMemberForm.value.userId.trim()) return
  addMemberLoading.value = true
  addMemberError.value = null
  try {
    await addMember(client, addMemberWorkspaceId.value, {
      userId: addMemberForm.value.userId.trim(),
      role: addMemberForm.value.role,
    })
    showAddMemberModal.value = false
    await loadWorkspaceMembers(addMemberWorkspaceId.value)
  } catch (e) {
    addMemberError.value = e instanceof Error ? e.message : 'Failed to add member'
  } finally {
    addMemberLoading.value = false
  }
}

async function handleChangeRole(workspaceId: string, userId: string, newRole: WorkspaceRole) {
  try {
    await changeMemberRole(client, workspaceId, userId, { role: newRole })
    await loadWorkspaceMembers(workspaceId)
  } catch {
    // Reload to show current state
    await loadWorkspaceMembers(workspaceId)
  }
}

async function handleRemoveMember() {
  if (!removeMemberTarget.value) return
  removeMemberLoading.value = true
  try {
    await removeMember(client, removeMemberTarget.value.workspaceId, removeMemberTarget.value.userId)
    const wsId = removeMemberTarget.value.workspaceId
    removeMemberTarget.value = null
    await loadWorkspaceMembers(wsId)
  } catch {
    // Handled via reload
  } finally {
    removeMemberLoading.value = false
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

      <!-- Overview Tab -->
      <div v-if="activeTab === 'overview'" class="space-y-6">
        <DcCard>
          <template #header>
            <div class="flex items-center justify-between">
              <h2 class="text-lg font-semibold text-gray-900">Organization Details</h2>
              <DcButton v-if="!editingName" variant="secondary" size="sm" @click="startEditName">
                Edit
              </DcButton>
            </div>
          </template>

          <DcAlert v-if="editNameError" variant="error" :title="editNameError" class="mb-4" />

          <div v-if="editingName" class="mb-4 flex items-end space-x-3">
            <div class="flex-1">
              <DcInput v-model="editName" label="Organization Name" :disabled="editNameLoading" />
            </div>
            <DcButton size="sm" :loading="editNameLoading" @click="saveEditName">Save</DcButton>
            <DcButton variant="secondary" size="sm" :disabled="editNameLoading" @click="cancelEditName">Cancel</DcButton>
          </div>

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

      <!-- Workspaces Tab -->
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
                  <router-link
                    :to="`/admin/workspaces/${workspace.id}`"
                    class="font-semibold text-indigo-600 hover:text-indigo-800 transition-colors"
                  >
                    {{ workspace.name }}
                  </router-link>
                  <p class="text-sm text-gray-500">{{ workspace.slug }}</p>
                </div>
                <div class="flex items-center space-x-2">
                  <StatusBadge :status="workspace.status" />
                  <DcButton
                    variant="danger"
                    size="sm"
                    @click="deleteWorkspaceTarget = workspace"
                  >
                    Delete
                  </DcButton>
                </div>
              </div>
              <p class="text-xs text-gray-500">Created {{ formatDate(workspace.createdAt) }}</p>
            </div>
          </DcCard>
        </div>
      </div>

      <!-- Members Tab -->
      <div v-if="activeTab === 'members'">
        <div v-if="workspaces.length === 0" class="text-center py-12">
          <p class="text-gray-500">No workspaces in this organization</p>
        </div>

        <div v-else class="space-y-4">
          <DcCard v-for="workspace in workspaces" :key="workspace.id">
            <div
              class="flex items-center justify-between cursor-pointer"
              @click="toggleWorkspaceMembers(workspace.id)"
            >
              <div class="flex items-center space-x-3">
                <span class="text-sm text-gray-400 transition-transform" :class="expandedWorkspaces.has(workspace.id) ? 'rotate-90' : ''">
                  &#9654;
                </span>
                <h3 class="font-semibold text-gray-900">{{ workspace.name }}</h3>
                <DcBadge variant="default" size="sm">
                  {{ workspaceMembers[workspace.id]?.length ?? '...' }} members
                </DcBadge>
              </div>
              <DcButton
                variant="secondary"
                size="sm"
                @click.stop="openAddMemberModal(workspace.id)"
              >
                Add Member
              </DcButton>
            </div>

            <div v-if="expandedWorkspaces.has(workspace.id)" class="mt-4">
              <DcSpinner v-if="membersLoading[workspace.id]" class="mx-auto" />
              <div v-else-if="!workspaceMembers[workspace.id]?.length" class="text-center py-4">
                <p class="text-sm text-gray-500">No members in this workspace</p>
              </div>
              <div v-else class="overflow-hidden border border-gray-200 rounded-lg">
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
                    <tr v-for="member in workspaceMembers[workspace.id]" :key="member.id" class="hover:bg-gray-50">
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
                            @change="handleChangeRole(workspace.id, member.userId, ($event.target as HTMLSelectElement).value as WorkspaceRole)"
                            class="text-xs border border-gray-300 rounded px-2 py-1 focus:ring-1 focus:ring-indigo-500 focus:outline-none"
                          >
                            <option v-for="opt in roleOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
                          </select>
                          <DcButton
                            variant="danger"
                            size="sm"
                            @click="removeMemberTarget = { workspaceId: workspace.id, userId: member.userId }"
                          >
                            Remove
                          </DcButton>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </DcCard>
        </div>
      </div>
    </template>

    <!-- Create Workspace Modal -->
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

    <!-- Delete Workspace Confirm -->
    <ConfirmDialog
      :open="!!deleteWorkspaceTarget"
      title="Delete Workspace"
      :message="`Are you sure you want to delete workspace '${deleteWorkspaceTarget?.name}'? This action cannot be undone.`"
      confirm-label="Delete"
      variant="danger"
      :loading="deleteWorkspaceLoading"
      @confirm="handleDeleteWorkspace"
      @cancel="deleteWorkspaceTarget = null"
    />

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
  </div>
</template>
