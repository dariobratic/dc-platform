<script setup lang="ts">
import { computed } from 'vue'
import { useTenantStore } from '@/stores/tenant'
import UserMenu from '@/components/UserMenu.vue'

defineEmits<{
  toggleSidebar: []
}>()

const tenantStore = useTenantStore()

const organizationName = computed(() => {
  return tenantStore.currentOrganization?.name ?? 'No Organization'
})
</script>

<template>
  <header class="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6">
    <div class="flex items-center space-x-4">
      <button
        @click="$emit('toggleSidebar')"
        class="lg:hidden p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
      >
        <span class="text-xl">☰</span>
      </button>

      <div class="flex items-center space-x-2">
        <span class="text-sm font-medium text-gray-700">Organization:</span>
        <div class="px-3 py-1 bg-gray-100 rounded-md text-sm font-medium text-gray-900">
          {{ organizationName }}
        </div>
      </div>
    </div>

    <UserMenu />
  </header>
</template>
