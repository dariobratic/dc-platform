<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { DcCard, DcButton } from '@dc-platform/ui-kit'
import PageHeader from '@/components/PageHeader.vue'

interface OidcUser {
  name: string
  email: string
  sub: string
}

const user = ref<OidcUser | null>(null)

function getOidcUser(): OidcUser | null {
  const storageKey = Object.keys(sessionStorage).find(k => k.startsWith('oidc.user:'))
  if (!storageKey) return null
  try {
    const userData = JSON.parse(sessionStorage.getItem(storageKey) || '{}')
    const profile = userData.profile || {}
    return {
      name: profile.preferred_username || profile.name || profile.sub || 'User',
      email: profile.email || '',
      sub: profile.sub || '',
    }
  } catch {
    return null
  }
}

function openKeycloakAccount() {
  const keycloakUrl = (import.meta.env.VITE_KEYCLOAK_URL as string | undefined) ?? 'http://localhost:8080'
  const realm = (import.meta.env.VITE_KEYCLOAK_REALM as string | undefined) ?? 'dc-platform'
  const accountUrl = `${keycloakUrl}/realms/${realm}/account`
  window.open(accountUrl, '_blank')
}

onMounted(() => {
  user.value = getOidcUser()
})
</script>

<template>
  <div class="p-6">
    <PageHeader title="Profile" description="Manage your account information" />

    <div class="space-y-6 max-w-3xl">
      <DcCard v-if="user">
        <h3 class="text-base font-semibold text-gray-900 mb-4">User Information</h3>
        <dl class="space-y-3">
          <div>
            <dt class="text-sm font-medium text-gray-500">Display Name</dt>
            <dd class="mt-1 text-sm text-gray-900">{{ user.name }}</dd>
          </div>
          <div>
            <dt class="text-sm font-medium text-gray-500">Email</dt>
            <dd class="mt-1 text-sm text-gray-900">{{ user.email || 'Not provided' }}</dd>
          </div>
          <div>
            <dt class="text-sm font-medium text-gray-500">User ID</dt>
            <dd class="mt-1 text-sm text-gray-900 font-mono">{{ user.sub }}</dd>
          </div>
        </dl>
        <div class="mt-6 pt-6 border-t border-gray-200">
          <DcButton variant="secondary" @click="openKeycloakAccount">
            Change Password
          </DcButton>
        </div>
      </DcCard>

      <DcCard>
        <h3 class="text-base font-semibold text-gray-900 mb-4">Preferences</h3>
        <p class="text-sm text-gray-500">
          User preferences and settings coming soon. Future features include theme selection, language preferences, and notification settings.
        </p>
      </DcCard>
    </div>
  </div>
</template>
