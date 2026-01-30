<script setup lang="ts">
import { useRoute } from 'vue-router'

interface Props {
  open: boolean
}

defineProps<Props>()
const emit = defineEmits<{
  toggle: []
}>()

const route = useRoute()

const isActive = (path: string): boolean => {
  return route.path.startsWith(path)
}

const navItems = [
  { name: 'Dashboard', path: '/dashboard', icon: '📊' },
  { name: 'Admin', path: '/admin', icon: '⚙️' },
  { name: 'Client Portal', path: '/app', icon: '👥' },
]
</script>

<template>
  <aside
    :class="[
      'fixed top-0 left-0 z-40 h-screen transition-all duration-300 bg-white border-r border-gray-200',
      open ? 'w-64' : 'w-20'
    ]"
  >
    <div class="flex flex-col h-full">
      <div class="flex items-center justify-between h-16 px-4 border-b border-gray-200">
        <div v-if="open" class="flex items-center space-x-2">
          <div class="w-8 h-8 bg-primary-600 rounded-lg flex items-center justify-center text-white font-bold">
            DC
          </div>
          <span class="text-lg font-semibold text-gray-900">Platform</span>
        </div>
        <div v-else class="w-full flex justify-center">
          <div class="w-8 h-8 bg-primary-600 rounded-lg flex items-center justify-center text-white font-bold">
            DC
          </div>
        </div>
      </div>

      <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <router-link
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          :class="[
            'flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors',
            isActive(item.path)
              ? 'bg-primary-50 text-primary-700'
              : 'text-gray-700 hover:bg-gray-100'
          ]"
        >
          <span class="text-xl">{{ item.icon }}</span>
          <span v-if="open" class="ml-3">{{ item.name }}</span>
        </router-link>
      </nav>

      <div class="p-4 border-t border-gray-200">
        <button
          @click="emit('toggle')"
          class="w-full flex items-center justify-center px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <span class="text-xl">{{ open ? '◀' : '▶' }}</span>
          <span v-if="open" class="ml-3">Collapse</span>
        </button>
      </div>
    </div>
  </aside>
</template>
