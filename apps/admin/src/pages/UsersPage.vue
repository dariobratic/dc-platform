<script setup lang="ts">
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
</script>

<template>
  <div class="p-6">
    <PageHeader title="Users" description="Manage platform users and memberships" />

    <EmptyState
      title="User Management Coming Soon"
      description="User listing and management features will be available once the user API is implemented."
    />
  </div>
</template>
