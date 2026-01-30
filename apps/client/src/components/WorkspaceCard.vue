<script setup lang="ts">
import { useRouter } from 'vue-router'
import { DcBadge } from '@dc-platform/ui-kit'
import type { Workspace } from '@dc-platform/shared-types'

interface Props {
  workspace: Workspace
}

defineProps<Props>()
const router = useRouter()
</script>

<template>
  <button
    class="block w-full text-left bg-white border border-gray-200 rounded-lg p-5 hover:border-indigo-300 hover:shadow-sm transition-all"
    @click="router.push(`/app/workspace/${workspace.id}`)"
  >
    <div class="flex items-start justify-between">
      <div>
        <h3 class="text-base font-semibold text-gray-900">{{ workspace.name }}</h3>
        <p class="text-sm text-gray-500 mt-1">{{ workspace.slug }}</p>
      </div>
      <DcBadge
        :variant="workspace.status === 'Active' ? 'success' : workspace.status === 'Suspended' ? 'warning' : 'error'"
        size="sm"
        dot
      >
        {{ workspace.status }}
      </DcBadge>
    </div>
    <p class="text-xs text-gray-400 mt-3">
      Created {{ new Date(workspace.createdAt).toLocaleDateString() }}
    </p>
  </button>
</template>
