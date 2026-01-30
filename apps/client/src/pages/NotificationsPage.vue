<script setup lang="ts">
import { DcCard } from '@dc-platform/ui-kit'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
</script>

<template>
  <div class="p-6">
    <PageHeader title="Notifications" description="Stay updated with your activity" />

    <DcCard>
      <EmptyState
        title="No notifications"
        description="You're all caught up! Notifications will appear here when there's new activity."
      />
    </DcCard>
  </div>
</template>
