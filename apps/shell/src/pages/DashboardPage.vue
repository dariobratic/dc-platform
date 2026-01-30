<script setup lang="ts">
import { computed } from 'vue'
import { useTenantStore } from '@/stores/tenant'
import { useAuthStore } from '@/stores/auth'

const tenantStore = useTenantStore()
const authStore = useAuthStore()

const organizationName = computed(() => tenantStore.currentOrganization?.name ?? 'Unknown')
const userName = computed(() => authStore.profile?.name ?? authStore.profile?.email ?? 'User')
</script>

<template>
  <div class="max-w-7xl mx-auto space-y-6">
    <div>
      <h1 class="text-3xl font-bold text-gray-900">Dashboard</h1>
      <p class="mt-2 text-gray-600">Welcome back, {{ userName }}</p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div class="p-6 bg-white rounded-lg shadow border border-gray-200">
        <div class="flex items-center space-x-3">
          <div class="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center text-primary-600 text-xl">
            🏢
          </div>
          <div>
            <h3 class="text-sm font-medium text-gray-500">Organization</h3>
            <p class="text-lg font-semibold text-gray-900">{{ organizationName }}</p>
          </div>
        </div>
      </div>

      <div class="p-6 bg-white rounded-lg shadow border border-gray-200">
        <div class="flex items-center space-x-3">
          <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center text-green-600 text-xl">
            ✓
          </div>
          <div>
            <h3 class="text-sm font-medium text-gray-500">Status</h3>
            <p class="text-lg font-semibold text-gray-900">Active</p>
          </div>
        </div>
      </div>

      <div class="p-6 bg-white rounded-lg shadow border border-gray-200">
        <div class="flex items-center space-x-3">
          <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center text-blue-600 text-xl">
            🔐
          </div>
          <div>
            <h3 class="text-sm font-medium text-gray-500">Authentication</h3>
            <p class="text-lg font-semibold text-gray-900">Keycloak</p>
          </div>
        </div>
      </div>
    </div>

    <div class="p-8 bg-white rounded-lg shadow border border-gray-200">
      <h2 class="text-xl font-semibold text-gray-900 mb-4">Welcome to DC Platform</h2>
      <p class="text-gray-600 mb-4">
        This is the shell application for the DC Platform microfrontend architecture.
        The platform provides enterprise workflow automation capabilities across multiple tenants.
      </p>
      <div class="space-y-2 text-sm text-gray-600">
        <p><strong>Current Features:</strong></p>
        <ul class="list-disc list-inside space-y-1 ml-4">
          <li>Keycloak OIDC authentication with PKCE flow</li>
          <li>Multi-organization tenant context</li>
          <li>Microfrontend architecture with Module Federation</li>
          <li>Gateway API integration</li>
        </ul>
      </div>
    </div>
  </div>
</template>
