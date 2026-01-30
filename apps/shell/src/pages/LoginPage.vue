<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { DcCard, DcInput, DcButton, DcAlert } from '@dc-platform/ui-kit'
import axios from 'axios'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleSubmit() {
  error.value = ''
  loading.value = true

  try {
    await authStore.loginWithCredentials(email.value, password.value)
    const intended = sessionStorage.getItem('intendedRoute')
    sessionStorage.removeItem('intendedRoute')
    await router.push(intended ?? '/select-organization')
  } catch (err) {
    if (axios.isAxiosError(err)) {
      const status = err.response?.status
      if (status === 401) {
        error.value = 'Invalid email or password'
      } else if (status === 429) {
        error.value = 'Too many attempts. Please try again later.'
      } else {
        error.value = 'An unexpected error occurred. Please try again.'
      }
    } else {
      error.value = 'An unexpected error occurred. Please try again.'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="w-full max-w-md">
    <div class="flex flex-col items-center mb-8">
      <div class="w-16 h-16 bg-indigo-600 rounded-lg flex items-center justify-center text-white text-2xl font-bold">
        DC
      </div>
      <h1 class="mt-4 text-2xl font-bold text-gray-900">Sign in to DC Platform</h1>
      <p class="mt-2 text-gray-600">Enter your credentials to continue</p>
    </div>

    <DcCard>
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <DcAlert v-if="error" variant="error" dismissible @dismiss="error = ''">
          {{ error }}
        </DcAlert>

        <DcInput
          v-model="email"
          type="email"
          label="Email"
          placeholder="you@example.com"
          required
          :disabled="loading"
        />

        <DcInput
          v-model="password"
          type="password"
          label="Password"
          placeholder="Enter your password"
          required
          :disabled="loading"
        />

        <DcButton
          type="submit"
          variant="primary"
          size="lg"
          :loading="loading"
          :disabled="!email || !password"
          class="w-full"
        >
          Sign in
        </DcButton>
      </form>

      <template #footer>
        <p class="text-center text-sm text-gray-600">
          Don't have an account?
          <router-link to="/signup" class="font-medium text-indigo-600 hover:text-indigo-500">
            Sign up
          </router-link>
        </p>
      </template>
    </DcCard>
  </div>
</template>
