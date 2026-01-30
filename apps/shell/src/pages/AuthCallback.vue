<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    await authStore.handleCallback()

    const intendedRoute = sessionStorage.getItem('intendedRoute')
    sessionStorage.removeItem('intendedRoute')

    if (intendedRoute && intendedRoute !== '/login' && intendedRoute !== '/callback') {
      await router.push(intendedRoute)
    } else {
      await router.push('/select-organization')
    }
  } catch (err) {
    console.error('Auth callback error:', err)
    error.value = err instanceof Error ? err.message : 'Authentication failed'
  }
})
</script>

<template>
  <div class="w-full max-w-md p-8 bg-white rounded-lg shadow-md">
    <div v-if="error" class="flex flex-col items-center space-y-6">
      <div class="w-16 h-16 bg-red-100 rounded-lg flex items-center justify-center text-red-600 text-2xl">
        ✕
      </div>
      <h1 class="text-2xl font-bold text-gray-900">Authentication Failed</h1>
      <p class="text-red-600 text-center">{{ error }}</p>
      <router-link
        to="/login"
        class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
      >
        Try Again
      </router-link>
    </div>

    <div v-else class="flex flex-col items-center space-y-6">
      <div class="w-16 h-16 bg-primary-600 rounded-lg flex items-center justify-center text-white text-2xl font-bold">
        DC
      </div>
      <h1 class="text-2xl font-bold text-gray-900">Completing Sign In</h1>
      <p class="text-gray-600 text-center">Please wait while we complete your authentication...</p>
      <div class="w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
    </div>
  </div>
</template>
