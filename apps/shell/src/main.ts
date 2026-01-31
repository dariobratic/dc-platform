import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router, { loadRemoteRoutes } from './router'
import { setupAuthErrorHandling } from './plugins/auth'
import './style.css'

console.log('[Shell Main] Starting shell application')
setupAuthErrorHandling()

const app = createApp(App)

app.use(createPinia())
app.use(router)

// Load remote microfrontend routes, then mount
// Fallback placeholders are shown if remotes are unavailable
loadRemoteRoutes().finally(() => {
  app.mount('#app')
})
