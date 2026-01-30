<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  variant?: 'default' | 'success' | 'error' | 'warning' | 'info'
  size?: 'sm' | 'md'
  dot?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  size: 'md',
  dot: false,
})

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'bg-green-100 text-green-800'
    case 'error':
      return 'bg-red-100 text-red-800'
    case 'warning':
      return 'bg-yellow-100 text-yellow-800'
    case 'info':
      return 'bg-blue-100 text-blue-800'
    case 'default':
    default:
      return 'bg-gray-100 text-gray-800'
  }
})

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'px-2 py-0.5 text-xs'
    case 'md':
    default:
      return 'px-2.5 py-0.5 text-sm'
  }
})

const dotColor = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'bg-green-600'
    case 'error':
      return 'bg-red-600'
    case 'warning':
      return 'bg-yellow-600'
    case 'info':
      return 'bg-blue-600'
    case 'default':
    default:
      return 'bg-gray-600'
  }
})
</script>

<template>
  <span
    :class="[
      'inline-flex items-center font-medium rounded-full',
      variantClasses,
      sizeClasses,
    ]"
  >
    <span v-if="dot" :class="['w-1.5 h-1.5 rounded-full mr-1.5', dotColor]" />
    <slot />
  </span>
</template>
