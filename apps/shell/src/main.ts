import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { setupAuthErrorHandling } from './plugins/auth'
import './style.css'

setupAuthErrorHandling()

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')
