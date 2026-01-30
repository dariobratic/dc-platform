<script setup lang="ts">
// Placeholder for client remote module
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <div class="p-8 bg-blue-50 border border-blue-200 rounded-lg">
      <h1 class="text-2xl font-bold text-blue-900 mb-4">Client Module Not Loaded</h1>
      <p class="text-blue-800 mb-4">
        The client-facing microfrontend module is not currently available.
        This is a placeholder page that will be replaced when the client remote is implemented.
      </p>
      <div class="text-sm text-blue-700 space-y-1">
        <p><strong>Expected Remote Configuration:</strong></p>
        <ul class="list-disc list-inside ml-4">
          <li>Remote Name: client</li>
          <li>URL: http://localhost:5174/assets/remoteEntry.js</li>
          <li>Exposes: ./routes</li>
        </ul>
      </div>
    </div>
  </div>
</template>
