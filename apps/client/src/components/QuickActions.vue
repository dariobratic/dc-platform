<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const actions = [
  { label: 'My Profile', description: 'View your account', route: '/app/profile', icon: 'user' },
  { label: 'Invitations', description: 'Check pending invites', route: '/app/invitations', icon: 'mail' },
  { label: 'Notifications', description: 'View notifications', route: '/app/notifications', icon: 'bell' },
]
</script>

<template>
  <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
    <button
      v-for="action in actions"
      :key="action.route"
      class="flex flex-col items-center p-4 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 hover:border-indigo-300 transition-colors text-center"
      @click="router.push(action.route)"
    >
      <div class="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center mb-3">
        <svg v-if="action.icon === 'user'" class="h-5 w-5 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
        </svg>
        <svg v-else-if="action.icon === 'mail'" class="h-5 w-5 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
        </svg>
        <svg v-else class="h-5 w-5 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
      </div>
      <span class="text-sm font-medium text-gray-900">{{ action.label }}</span>
      <span class="text-xs text-gray-500 mt-1">{{ action.description }}</span>
    </button>
  </div>
</template>
