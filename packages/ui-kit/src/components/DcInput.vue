<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue?: string
  type?: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url'
  label?: string
  placeholder?: string
  helperText?: string
  error?: string
  success?: boolean
  disabled?: boolean
  required?: boolean
  id?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  type: 'text',
  success: false,
  disabled: false,
  required: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const inputId = computed(() => props.id ?? `input-${Math.random().toString(36).substring(2, 11)}`)

const inputClasses = computed(() => {
  const base = 'block w-full rounded-md border px-3 py-2 text-sm shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-0'

  if (props.disabled) {
    return `${base} bg-gray-50 text-gray-500 cursor-not-allowed border-gray-300`
  }

  if (props.error) {
    return `${base} border-red-300 focus:border-red-500 focus:ring-red-500`
  }

  if (props.success) {
    return `${base} border-green-300 focus:border-green-500 focus:ring-green-500`
  }

  return `${base} border-gray-300 focus:border-indigo-500 focus:ring-indigo-500`
})

const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}
</script>

<template>
  <div>
    <label
      v-if="label"
      :for="inputId"
      class="block text-sm font-medium text-gray-700 mb-1"
    >
      {{ label }}
      <span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    <input
      :id="inputId"
      :type="type"
      :value="modelValue"
      :disabled="disabled"
      :placeholder="placeholder"
      :required="required"
      :class="inputClasses"
      @input="handleInput"
    />
    <p v-if="error" class="mt-1 text-sm text-red-600">{{ error }}</p>
    <p v-else-if="helperText" class="mt-1 text-sm text-gray-500">{{ helperText }}</p>
  </div>
</template>
