<script setup lang="ts">
import { DcModal, DcButton } from '@dc-platform/ui-kit'

interface Props {
  open: boolean
  title: string
  message: string
  confirmLabel?: string
  variant?: 'danger' | 'primary'
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  confirmLabel: 'Confirm',
  variant: 'danger',
  loading: false,
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<template>
  <DcModal :open="open" :title="title" size="sm" @close="emit('cancel')">
    <p class="text-sm text-gray-600">{{ message }}</p>
    <template #footer>
      <div class="flex justify-end space-x-3">
        <DcButton variant="secondary" @click="emit('cancel')">Cancel</DcButton>
        <DcButton :variant="variant" :loading="loading" @click="emit('confirm')">
          {{ confirmLabel }}
        </DcButton>
      </div>
    </template>
  </DcModal>
</template>
