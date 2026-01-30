<script setup lang="ts">
import type { AuditEntryResponse } from '@dc-platform/shared-types'

interface Props {
  entries: AuditEntryResponse[]
}

defineProps<Props>()

function formatTime(dateString: string): string {
  return new Date(dateString).toLocaleString()
}

function getActionLabel(action: string): string {
  return action.replace(/\./g, ' ').replace(/\b\w/g, c => c.toUpperCase())
}
</script>

<template>
  <div class="flow-root">
    <ul class="-mb-4">
      <li v-for="entry in entries" :key="entry.id" class="pb-4">
        <div class="flex space-x-3">
          <div class="flex-shrink-0">
            <div class="h-8 w-8 rounded-full bg-indigo-100 flex items-center justify-center">
              <svg class="h-4 w-4 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900">{{ getActionLabel(entry.action) }}</p>
            <p class="text-xs text-gray-500">
              {{ entry.entityType }} · {{ formatTime(entry.timestamp) }}
            </p>
          </div>
        </div>
      </li>
    </ul>
    <p v-if="entries.length === 0" class="text-sm text-gray-500 text-center py-4">
      No recent activity
    </p>
  </div>
</template>
