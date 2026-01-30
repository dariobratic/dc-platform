<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useTenantStore } from '@/stores/tenant'
import { http } from '@/plugins/http'
import type { Organization, OrganizationsResponse } from '@/types'

const router = useRouter()
const tenantStore = useTenantStore()

const organizations = ref<Organization[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  await fetchOrganizations()
})

async function fetchOrganizations(): Promise<void> {
  loading.value = true
  error.value = null

  try {
    const response = await http.get<OrganizationsResponse>('/api/v1/users/me/organizations')
    organizations.value = response.data.organizations

    if (organizations.value.length === 1) {
      await selectOrganization(organizations.value[0])
    }
  } catch (err) {
    console.error('Failed to fetch organizations:', err)
    error.value = 'Failed to load organizations. Please try again.'
  } finally {
    loading.value = false
  }
}

async function selectOrganization(organization: Organization): Promise<void> {
  tenantStore.setOrganization(organization.id, organization)

  const intendedRoute = sessionStorage.getItem('intendedRoute')
  sessionStorage.removeItem('intendedRoute')

  if (intendedRoute && intendedRoute !== '/login' && intendedRoute !== '/callback' && intendedRoute !== '/select-organization') {
    await router.push(intendedRoute)
  } else {
    await router.push('/dashboard')
  }
}
</script>

<template>
  <div class="w-full max-w-2xl p-8 bg-white rounded-lg shadow-md">
    <div class="flex flex-col items-center space-y-6">
      <div class="w-16 h-16 bg-primary-600 rounded-lg flex items-center justify-center text-white text-2xl font-bold">
        DC
      </div>
      <h1 class="text-2xl font-bold text-gray-900">Select Organization</h1>

      <div v-if="loading" class="w-full flex justify-center py-8">
        <div class="w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
      </div>

      <div v-else-if="error" class="w-full">
        <div class="p-4 bg-red-50 border border-red-200 rounded-lg">
          <p class="text-red-600 text-center">{{ error }}</p>
        </div>
        <button
          @click="fetchOrganizations"
          class="mt-4 w-full px-6 py-3 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
        >
          Retry
        </button>
      </div>

      <div v-else-if="organizations.length === 0" class="w-full">
        <div class="p-6 bg-yellow-50 border border-yellow-200 rounded-lg">
          <p class="text-yellow-800 text-center">
            You are not a member of any organizations yet.
          </p>
          <p class="text-yellow-700 text-center text-sm mt-2">
            Please contact your administrator to get access.
          </p>
        </div>
      </div>

      <div v-else class="w-full space-y-3">
        <button
          v-for="org in organizations"
          :key="org.id"
          @click="selectOrganization(org)"
          class="w-full p-4 text-left border border-gray-200 rounded-lg hover:border-primary-600 hover:bg-primary-50 transition-all group"
        >
          <div class="flex items-center justify-between">
            <div>
              <h3 class="text-lg font-medium text-gray-900 group-hover:text-primary-700">
                {{ org.name }}
              </h3>
              <p class="text-sm text-gray-500">{{ org.slug }}</p>
            </div>
            <span class="text-gray-400 group-hover:text-primary-600">→</span>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>
