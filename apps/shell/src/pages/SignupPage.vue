<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useTenantStore } from '@/stores/tenant'
import { DcCard, DcInput, DcButton, DcAlert } from '@dc-platform/ui-kit'
import { generateSlug } from '@/utils/slug'
import axios from 'axios'
import type { SignupFormData } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const tenantStore = useTenantStore()

const form = ref<SignupFormData>({
  fullName: '',
  email: '',
  password: '',
  confirmPassword: '',
  organizationName: '',
})

const errors = ref<Partial<Record<keyof SignupFormData, string>>>({})
const serverError = ref('')
const loading = ref(false)

const orgSlug = computed(() => generateSlug(form.value.organizationName))

watch(() => form.value.confirmPassword, () => {
  if (errors.value.confirmPassword && form.value.password === form.value.confirmPassword) {
    errors.value.confirmPassword = ''
  }
})

function validate(): boolean {
  const newErrors: Partial<Record<keyof SignupFormData, string>> = {}

  if (!form.value.fullName.trim()) {
    newErrors.fullName = 'Full name is required'
  }

  if (!form.value.email.trim()) {
    newErrors.email = 'Email is required'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email)) {
    newErrors.email = 'Please enter a valid email address'
  }

  if (!form.value.password) {
    newErrors.password = 'Password is required'
  } else if (form.value.password.length < 8) {
    newErrors.password = 'Password must be at least 8 characters'
  }

  if (form.value.password !== form.value.confirmPassword) {
    newErrors.confirmPassword = 'Passwords do not match'
  }

  if (!form.value.organizationName.trim()) {
    newErrors.organizationName = 'Organization name is required'
  }

  errors.value = newErrors
  return Object.keys(newErrors).length === 0
}

async function handleSubmit() {
  if (!validate()) return

  serverError.value = ''
  loading.value = true

  try {
    const nameParts = form.value.fullName.trim().split(/\s+/)
    const firstName = nameParts[0]
    const lastName = nameParts.slice(1).join(' ') || firstName

    const response = await authStore.signupUser({
      email: form.value.email,
      password: form.value.password,
      firstName,
      lastName,
      organizationName: form.value.organizationName,
    })

    const orgObject = {
      id: response.organizationId,
      name: form.value.organizationName,
      slug: orgSlug.value,
      createdAt: new Date().toISOString(),
    }
    tenantStore.setOrganization(response.organizationId, orgObject)
    tenantStore.setWorkspace(response.workspaceId)
    await router.push('/dashboard')
  } catch (err) {
    if (axios.isAxiosError(err)) {
      const errorCode = err.response?.data?.error
      if (err.response?.status === 409) {
        if (errorCode === 'email_exists') {
          serverError.value = 'An account with this email already exists'
        } else if (errorCode === 'resource_conflict') {
          serverError.value = 'Organization slug already exists. Please choose a different name.'
        } else {
          serverError.value = 'A conflict occurred. Please try a different email or organization name.'
        }
      } else {
        serverError.value = 'An unexpected error occurred. Please try again.'
      }
    } else {
      serverError.value = 'An unexpected error occurred. Please try again.'
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
      <h1 class="mt-4 text-2xl font-bold text-gray-900">Create your account</h1>
      <p class="mt-2 text-gray-600">Get started with DC Platform</p>
    </div>

    <DcCard>
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <DcAlert v-if="serverError" variant="error" dismissible @dismiss="serverError = ''">
          {{ serverError }}
        </DcAlert>

        <DcInput
          v-model="form.fullName"
          label="Full Name"
          placeholder="John Doe"
          required
          :disabled="loading"
          :error="errors.fullName"
        />

        <DcInput
          v-model="form.email"
          type="email"
          label="Email"
          placeholder="you@example.com"
          required
          :disabled="loading"
          :error="errors.email"
        />

        <DcInput
          v-model="form.password"
          type="password"
          label="Password"
          placeholder="At least 8 characters"
          required
          :disabled="loading"
          :error="errors.password"
        />

        <DcInput
          v-model="form.confirmPassword"
          type="password"
          label="Confirm Password"
          placeholder="Repeat your password"
          required
          :disabled="loading"
          :error="errors.confirmPassword"
        />

        <div>
          <DcInput
            v-model="form.organizationName"
            label="Organization Name"
            placeholder="My Company"
            required
            :disabled="loading"
            :error="errors.organizationName"
          />
          <p v-if="orgSlug" class="mt-1 text-sm text-gray-500">
            Slug: {{ orgSlug }}
          </p>
        </div>

        <DcButton
          type="submit"
          variant="primary"
          size="lg"
          :loading="loading"
          :disabled="loading"
          class="w-full"
        >
          Create Account
        </DcButton>
      </form>

      <template #footer>
        <p class="text-center text-sm text-gray-600">
          Already have an account?
          <router-link to="/login" class="font-medium text-indigo-600 hover:text-indigo-500">
            Sign in
          </router-link>
        </p>
      </template>
    </DcCard>
  </div>
</template>
