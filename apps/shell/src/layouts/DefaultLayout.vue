<script setup lang="ts">
import { ref } from 'vue'
import AppSidebar from '@/components/AppSidebar.vue'
import AppHeader from '@/components/AppHeader.vue'

const sidebarOpen = ref(true)

function toggleSidebar(): void {
  sidebarOpen.value = !sidebarOpen.value
}
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <AppSidebar :open="sidebarOpen" @toggle="toggleSidebar" />

    <div :class="[
      'transition-all duration-300',
      sidebarOpen ? 'lg:pl-64' : 'lg:pl-20'
    ]">
      <AppHeader @toggle-sidebar="toggleSidebar" />

      <main class="p-6">
        <router-view />
      </main>
    </div>
  </div>
</template>
