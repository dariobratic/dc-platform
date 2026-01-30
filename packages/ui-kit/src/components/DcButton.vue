<script setup lang="ts">
import { computed } from 'vue'
import DcSpinner from './DcSpinner.vue'

interface Props {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  disabled?: boolean
  loading?: boolean
  type?: 'button' | 'submit' | 'reset'
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  disabled: false,
  loading: false,
  type: 'button',
})

const emit = defineEmits<{
  click: [event: MouseEvent]
}>()

const variantClasses = computed(() => {
  const inactive = props.disabled || props.loading

  switch (props.variant) {
    case 'primary':
      return `bg-indigo-600 text-white focus:ring-indigo-500 ${inactive ? 'opacity-50 cursor-not-allowed' : 'hover:bg-indigo-700'}`
    case 'secondary':
      return `bg-white border border-gray-300 text-gray-700 focus:ring-indigo-500 ${inactive ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-50'}`
    case 'danger':
      return `bg-red-600 text-white focus:ring-red-500 ${inactive ? 'opacity-50 cursor-not-allowed' : 'hover:bg-red-700'}`
    case 'ghost':
      return `bg-transparent text-gray-700 focus:ring-indigo-500 ${inactive ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-100'}`
    default:
      return ''
  }
})

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'px-3 py-1.5 text-sm'
    case 'md':
      return 'px-4 py-2 text-sm'
    case 'lg':
      return 'px-6 py-3 text-base'
    default:
      return ''
  }
})

const handleClick = (event: MouseEvent) => {
  if (!props.disabled && !props.loading) {
    emit('click', event)
  }
}
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'inline-flex items-center justify-center font-medium rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2',
      variantClasses,
      sizeClasses,
    ]"
    @click="handleClick"
  >
    <DcSpinner v-if="loading" size="sm" color="currentColor" class="mr-2" />
    <slot />
  </button>
</template>
