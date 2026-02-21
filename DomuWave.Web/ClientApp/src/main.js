import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import ToastService from 'primevue/toastservice'
import Tooltip from 'primevue/tooltip'

import router from '@/router'
import App from './App.vue'

// CSS globale + PrimeIcons
import '@/assets/main.css'
import 'primeicons/primeicons.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      // Usa la classe .p-dark sull'elemento <html> per il dark mode
      darkModeSelector: '.p-dark',
      cssLayer: false,
    },
  },
})
app.use(ToastService)
app.directive('tooltip', Tooltip)

// Applica dark theme di default
document.documentElement.classList.add('p-dark')

app.mount('#app')
