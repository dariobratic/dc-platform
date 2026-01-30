<script setup lang="ts">
// Placeholder for admin remote module
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <div class="p-8 bg-yellow-50 border border-yellow-200 rounded-lg">
      <h1 class="text-2xl font-bold text-yellow-900 mb-4">Admin Module Not Loaded</h1>
      <p class="text-yellow-800 mb-4">
        The admin microfrontend module is not currently available.
        This is a placeholder page that will be replaced when the admin remote is implemented.
      </p>
      <div class="text-sm text-yellow-700 space-y-1">
        <p><strong>Expected Remote Configuration:</strong></p>
        <ul class="list-disc list-inside ml-4">
          <li>Remote Name: admin</li>
          <li>URL: http://localhost:5173/assets/remoteEntry.js</li>
          <li>Exposes: ./routes</li>
        </ul>
      </div>
    </div>
  </div>
</template>
