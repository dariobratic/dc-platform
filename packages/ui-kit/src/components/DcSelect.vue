<script setup lang="ts">
import { computed } from 'vue'

interface SelectOption {
  value: string | number
  label: string
  disabled?: boolean
}

interface Props {
  modelValue?: string | number | null
  options: SelectOption[]
  label?: string
  placeholder?: string
  error?: string
  helperText?: string
  disabled?: boolean
  required?: boolean
  id?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  placeholder: 'Select an option',
  disabled: false,
  required: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number | null]
}>()

const selectId = computed(() => props.id ?? `select-${Math.random().toString(36).substring(2, 11)}`)

const selectClasses = computed(() => {
  const base = 'block w-full rounded-md border px-3 py-2 text-sm shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-0'

  if (props.disabled) {
    return `${base} bg-gray-50 text-gray-500 cursor-not-allowed border-gray-300`
  }

  if (props.error) {
    return `${base} border-red-300 focus:border-red-500 focus:ring-red-500`
  }

  return `${base} border-gray-300 focus:border-indigo-500 focus:ring-indigo-500`
})

const handleChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const value = target.value

  if (value === '') {
    emit('update:modelValue', null)
  } else {
    const option = props.options.find(opt => String(opt.value) === value)
    emit('update:modelValue', option?.value ?? null)
  }
}
</script>

<template>
  <div>
    <label
      v-if="label"
      :for="selectId"
      class="block text-sm font-medium text-gray-700 mb-1"
    >
      {{ label }}
      <span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    <select
      :id="selectId"
      :value="modelValue ?? ''"
      :disabled="disabled"
      :required="required"
      :class="selectClasses"
      @change="handleChange"
    >
      <option value="" disabled>{{ placeholder }}</option>
      <option
        v-for="option in options"
        :key="String(option.value)"
        :value="option.value"
        :disabled="option.disabled"
      >
        {{ option.label }}
      </option>
    </select>
    <p v-if="error" class="mt-1 text-sm text-red-600">{{ error }}</p>
    <p v-else-if="helperText" class="mt-1 text-sm text-gray-500">{{ helperText }}</p>
  </div>
</template>
