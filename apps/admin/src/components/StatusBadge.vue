<script setup lang="ts">
import { computed } from 'vue'
import { DcBadge } from '@dc-platform/ui-kit'

interface Props {
  status: string
}

const props = defineProps<Props>()

const variant = computed(() => {
  switch (props.status) {
    case 'Active': return 'success'
    case 'Suspended': return 'warning'
    case 'Deleted': return 'error'
    case 'Pending': return 'info'
    case 'Expired': return 'default'
    case 'Revoked': return 'error'
    default: return 'default'
  }
})
</script>

<template>
  <DcBadge :variant="variant" dot size="sm">{{ status }}</DcBadge>
</template>
