import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/authStore'
import { useDomuWaveStore } from '@/stores/domuWaveStore'
import currencyDirective from './directives/currency'
import dateDirective from './directives/date'

import PrimeVue from 'primevue/config';
import ConfirmationService from 'primevue/confirmationservice'
import DialogService from 'primevue/dialogservice'
import ToastService from 'primevue/toastservice';

import Material from '@primeuix/themes/material';
import Calendar from 'primevue/calendar'

const app = createApp(App)
app.use(PrimeVue, {
  theme: {
    preset: Material,
    options:
    {
      darkModeSelector: false || 'none'
    }
  }
});
app.config.devtools = true;
app.use(createPinia())
app.use(router)
app.use(ConfirmationService);
app.use(ToastService);
app.use(DialogService);

app.directive('currency', currencyDirective)
app.directive('date', dateDirective)

const auth = useAuthStore()
auth.loadFromStorage()

const DomuWaveStore = useDomuWaveStore()

// Caricamento del menu prima del mount
DomuWaveStore.loadMenu().then(() => {
  console.log("app mount");
  app.mount('#wrapper');
});

