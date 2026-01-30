<script setup lang="ts">
import { DcButton, DcBadge, DcCard } from '@dc-platform/ui-kit'
import type { Invitation } from '@dc-platform/shared-types'

interface Props {
  invitation: Invitation
}

defineProps<Props>()

const emit = defineEmits<{
  accept: [invitation: Invitation]
  decline: [invitation: Invitation]
}>()

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}
</script>

<template>
  <DcCard>
    <div class="flex items-start justify-between">
      <div class="space-y-1">
        <h3 class="text-base font-semibold text-gray-900">Workspace Invitation</h3>
        <p class="text-sm text-gray-500">
          Role: <DcBadge variant="info" size="sm">{{ invitation.role }}</DcBadge>
        </p>
        <p class="text-xs text-gray-400">
          Expires {{ formatDate(invitation.expiresAt) }}
        </p>
      </div>
      <div class="flex space-x-2">
        <DcButton variant="secondary" size="sm" @click="emit('decline', invitation)">
          Decline
        </DcButton>
        <DcButton size="sm" @click="emit('accept', invitation)">
          Accept
        </DcButton>
      </div>
    </div>
  </DcCard>
</template>
