<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()
const menuOpen = ref(false)

const userName = computed(() => authStore.profile?.name ?? authStore.profile?.email ?? 'User')
const userEmail = computed(() => authStore.profile?.email ?? '')

function toggleMenu(): void {
  menuOpen.value = !menuOpen.value
}

function closeMenu(): void {
  menuOpen.value = false
}

async function handleLogout(): Promise<void> {
  closeMenu()
  await authStore.logout()
  await router.push('/login')
}
</script>

<template>
  <div class="relative">
    <button
      @click="toggleMenu"
      class="flex items-center space-x-3 px-3 py-2 rounded-lg hover:bg-gray-100 transition-colors"
    >
      <div class="w-8 h-8 bg-primary-600 rounded-full flex items-center justify-center text-white text-sm font-medium">
        {{ userName.charAt(0).toUpperCase() }}
      </div>
      <div class="hidden md:block text-left">
        <div class="text-sm font-medium text-gray-900">{{ userName }}</div>
        <div class="text-xs text-gray-500">{{ userEmail }}</div>
      </div>
      <span class="text-gray-400">▼</span>
    </button>

    <div
      v-if="menuOpen"
      @click.self="closeMenu"
      class="fixed inset-0 z-50"
    >
      <div class="absolute right-6 top-16 w-64 bg-white rounded-lg shadow-lg border border-gray-200 py-2">
        <div class="px-4 py-3 border-b border-gray-200">
          <div class="text-sm font-medium text-gray-900">{{ userName }}</div>
          <div class="text-xs text-gray-500">{{ userEmail }}</div>
        </div>

        <div class="py-1">
          <button
            @click="handleLogout"
            class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
          >
            Sign out
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
