<script setup lang="ts">
import { DcCard } from '@dc-platform/ui-kit'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
</script>

<template>
  <div class="p-6">
    <PageHeader title="Invitations" description="Manage your workspace invitations" />

    <DcCard>
      <EmptyState
        title="No pending invitations"
        description="When you're invited to join a workspace, invitations will appear here."
      />
    </DcCard>
  </div>
</template>
