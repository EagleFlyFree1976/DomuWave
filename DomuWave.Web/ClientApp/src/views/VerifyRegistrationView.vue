<template>
  <div class="verify-wrapper">
    <div class="verify-card">
      <!-- In corso -->
      <template v-if="state === 'verifying'">
        <div class="verify-icon spin">⏳</div>
        <h2>Verifica in corso…</h2>
        <p class="muted">Stiamo confermando la tua registrazione, attendi un momento.</p>
      </template>

      <!-- Successo -->
      <template v-else-if="state === 'success'">
        <div class="verify-icon">🌊</div>
        <h2>Benvenuto in DomuWave!</h2>
        <p class="muted">Il tuo account è attivo e la prova gratuita è iniziata.</p>
        <button class="btn-primary" :disabled="loading" @click="goToDashboard">
          <span v-if="loading" class="spinner"></span>
          <span v-else>Vai alla dashboard →</span>
        </button>
      </template>

      <!-- Errore -->
      <template v-else>
        <div class="verify-icon">⚠️</div>
        <h2>Verifica non riuscita</h2>
        <p class="muted">{{ errorMsg }}</p>
        <RouterLink to="/register" class="btn-ghost">Torna alla registrazione</RouterLink>
        <RouterLink to="/login" class="btn-ghost">Vai al login</RouterLink>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authApi, licenseApi } from '@/services/api'
import { useAuthStore } from '@/stores/authStore'
import { useMenuStore } from '@/stores/menuStore'

const route     = useRoute()
const router    = useRouter()
const authStore = useAuthStore()
const menuStore = useMenuStore()

const state    = ref('verifying')   // verifying | success | error
const errorMsg = ref('')
const loading  = ref(false)
const userEmail = ref('')

onMounted(async () => {
  const token = route.query.token
  if (!token) {
    state.value = 'error'
    errorMsg.value = 'Link di verifica mancante o non valido.'
    return
  }

  // Guardia anti doppia-esecuzione: in dev (StrictMode/HMR) onMounted può girare due volte.
  // Il token va consumato UNA sola volta: se è già in corso/fatto per questo token, esci.
  const guardKey = `verify_in_progress_${token}`
  if (sessionStorage.getItem(guardKey)) return
  sessionStorage.setItem(guardKey, '1')

  try {
    // Conferma: crea utente + tenant + condominio
    const res  = await authApi.confirmRegistration({ token })
    const data = res.data
    userEmail.value = data.email

    localStorage.setItem('domuwave_token', data.token ?? '')
    localStorage.setItem('domuwave_user', JSON.stringify({ id: data.userId, username: data.email }))
    localStorage.setItem('tenantId',   data.tenantId)
    localStorage.setItem('tenantName', data.tenantName)

    // Attiva il piano trial su LicenseManager (non bloccante)
    try {
      await licenseApi.register({
        firstName: '', lastName: '',
        email:      data.email,
        password:   '',
        tenantName: data.tenantName,
      })
    } catch { /* trial non critico */ }

    state.value = 'success'
  } catch (err) {
    const d = err?.response?.data
    const errs = d?.Errors ?? d?.errors
    const msg = (Array.isArray(errs) && errs.length ? errs.join(' ') : null)
      ?? d?.message ?? (typeof d === 'string' ? d : null)
      ?? 'Il link di verifica non è valido o è scaduto.'

    // Se l'account risulta già confermato/registrato, il token è già stato usato con successo
    // (es. doppio mount o ricarica pagina): trattalo come successo idempotente.
    const alreadyDone = /già (stata )?(confermat|registrat)/i.test(msg) || /already/i.test(msg)
    if (alreadyDone) {
      state.value = 'success'
    } else {
      state.value = 'error'
      errorMsg.value = msg
      sessionStorage.removeItem(guardKey)   // permetti un retry su errore reale
    }
  }
})

async function goToDashboard() {
  loading.value = true
  try {
    // Il token è già in localStorage dalla conferma; vai alla dashboard
    await menuStore.fetchMenu().catch(() => {})
    router.push('/dashboard')
  } catch {
    router.push({ path: '/login', query: { email: userEmail.value, registered: '1' } })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.verify-wrapper {
  min-height: 100vh; display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, #eef4fb, #f7fafd); padding: 24px;
}
.verify-card {
  background: #fff; border-radius: 20px; padding: 48px 40px; text-align: center;
  max-width: 440px; width: 100%; box-shadow: 0 20px 60px -12px rgba(30,109,184,0.25);
  display: flex; flex-direction: column; align-items: center; gap: 14px;
}
.verify-icon { font-size: 56px; }
.verify-icon.spin { animation: pulse 1.2s ease-in-out infinite; }
@keyframes pulse { 0%,100% { opacity: .5; } 50% { opacity: 1; } }
.verify-card h2 { font-size: 22px; font-weight: 700; color: #1e3a5f; margin: 0; }
.muted { color: #6b7280; font-size: 14.5px; line-height: 1.5; margin: 0; }
.btn-primary {
  margin-top: 12px; background: linear-gradient(135deg, #1e6db8, #2a8bc7); color: #fff;
  border: none; padding: 13px 28px; border-radius: 12px; font-size: 14.5px; font-weight: 600;
  cursor: pointer; min-width: 200px;
}
.btn-ghost {
  margin-top: 4px; color: #1e6db8; text-decoration: none; font-size: 14px; font-weight: 500;
}
.btn-ghost:hover { text-decoration: underline; }
.spinner {
  display: inline-block; width: 16px; height: 16px;
  border: 2px solid rgba(255,255,255,.4); border-top-color: #fff; border-radius: 50%;
  animation: spin .6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
