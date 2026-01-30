<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

onMounted(async () => {
  if (authStore.isAuthenticated) {
    return
  }
  await authStore.login()
})
</script>

<template>
  <div class="w-full max-w-md p-8 bg-white rounded-lg shadow-md">
    <div class="flex flex-col items-center space-y-6">
      <div class="w-16 h-16 bg-primary-600 rounded-lg flex items-center justify-center text-white text-2xl font-bold">
        DC
      </div>
      <h1 class="text-2xl font-bold text-gray-900">DC Platform</h1>
      <p class="text-gray-600 text-center">Redirecting to authentication...</p>
      <div class="w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
    </div>
  </div>
</template>
