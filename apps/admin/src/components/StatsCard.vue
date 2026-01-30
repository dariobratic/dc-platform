<script setup lang="ts">
interface Props {
  title: string
  value: string | number
  icon?: string
  trend?: 'up' | 'down' | 'neutral'
}

defineProps<Props>()
</script>

<template>
  <div class="bg-white border border-gray-200 rounded-lg p-6">
    <p class="text-sm font-medium text-gray-500">{{ title }}</p>
    <p class="mt-2 text-3xl font-bold text-gray-900">{{ value }}</p>
  </div>
</template>
