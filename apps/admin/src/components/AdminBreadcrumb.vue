<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const breadcrumbs = computed(() => {
  const items = [{ label: 'Admin', to: '/admin' }]
  for (const matched of route.matched) {
    if (matched.meta?.breadcrumb) {
      items.push({
        label: matched.meta.breadcrumb as string,
        to: matched.path.replace(/:\w+/g, (param) => {
          const key = param.slice(1)
          return (route.params[key] as string) || param
        }),
      })
    }
  }
  return items
})
</script>

<template>
  <nav class="flex items-center space-x-2 text-sm text-gray-500 mb-4">
    <template v-for="(crumb, index) in breadcrumbs" :key="crumb.to">
      <span v-if="index > 0" class="text-gray-300">/</span>
      <router-link
        v-if="index < breadcrumbs.length - 1"
        :to="crumb.to"
        class="hover:text-gray-700 transition-colors"
      >
        {{ crumb.label }}
      </router-link>
      <span v-else class="text-gray-900 font-medium">{{ crumb.label }}</span>
    </template>
  </nav>
</template>
