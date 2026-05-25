<template>
  <div class="reg-wrapper">
    <div class="med-wave">
      <svg viewBox="0 0 1200 140" preserveAspectRatio="none" style="width:100%;height:100%">
        <path d="M0,80 C200,30 400,110 600,70 C800,30 1000,90 1200,60 L1200,140 L0,140 Z" fill="#cce4f7" opacity="0.5" />
        <path d="M0,110 C200,80 400,130 600,100 C800,70 1000,110 1200,90 L1200,140 L0,140 Z" fill="#82b8df" opacity="0.4" />
      </svg>
    </div>

    <div class="reg-card slide-up">
      <!-- Brand -->
      <div class="med-brand">
        <div class="med-brand-mark">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" style="position:relative;z-index:1">
            <path d="M2 12 Q6 7 10 12 T18 12 T24 12" stroke="white" stroke-width="2.2" fill="none" stroke-linecap="round"/>
            <path d="M2 16.5 Q6 11.5 10 16.5 T18 16.5 T24 16.5" stroke="white" stroke-width="2.2" fill="none" stroke-linecap="round" opacity="0.55"/>
          </svg>
        </div>
        <span>Domu<em>Wave</em></span>
      </div>

      <!-- Step indicator (steps 1-3) -->
      <div class="step-indicator" v-if="step < 4">
        <div v-for="s in 3" :key="s" class="step-dot" :class="{ active: step === s, done: step > s }">
          <span v-if="step > s">✓</span>
          <span v-else>{{ s }}</span>
        </div>
        <div class="step-line"></div>
      </div>

      <!-- ── STEP 1: Credenziali ── -->
      <template v-if="step === 1">
        <div class="med-tag"><span style="font-size:12px">✦</span> Gratis · senza carta di credito</div>
        <h1 class="med-headline">Amministrare,<br>sotto un <span class="sun-word">altro sole</span>.</h1>
        <p class="med-subtitle">Bastano 30 secondi per iniziare la prova gratuita.</p>

        <!-- Fase 1a: inserimento email + nuova password -->
        <form v-if="!awaitingCondominoVerify" @submit.prevent="handleStep1" novalidate>
          <div class="field-group">
            <label class="med-label">Email</label>
            <input class="med-input" :class="{ 'med-input--error': errors.email }"
              v-model="form.email" type="email" placeholder="marco.rossi@studio.it" autocomplete="email" />
            <span class="error-msg" v-if="errors.email">{{ errors.email }}</span>
          </div>
          <div class="field-group">
            <label class="med-label">Password</label>
            <input class="med-input" :class="{ 'med-input--error': errors.password }"
              v-model="form.password" type="password" placeholder="min. 8 caratteri" autocomplete="new-password" />
            <span class="error-msg" v-if="errors.password">{{ errors.password }}</span>
          </div>
          <div class="field-group">
            <label class="med-label">Conferma password</label>
            <input class="med-input" :class="{ 'med-input--error': errors.confirmPassword }"
              v-model="form.confirmPassword" type="password" placeholder="Ripeti la password" autocomplete="new-password" />
            <span class="error-msg" v-if="errors.confirmPassword">{{ errors.confirmPassword }}</span>
          </div>
          <label class="check-label">
            <input type="checkbox" v-model="form.acceptTerms" />
            <span>Accetto i <a href="#" class="link">Termini di servizio</a> e la <a href="#" class="link">Privacy Policy</a></span>
          </label>
          <span class="error-msg" v-if="errors.acceptTerms">{{ errors.acceptTerms }}</span>
          <div v-if="apiError" class="api-error">⚠ {{ apiError }}</div>
          <button type="submit" class="med-btn" :disabled="loading">
            <span v-if="loading" class="btn-spinner"></span>
            <span v-else>Continua</span>
            <span v-if="!loading" style="font-size:16px">→</span>
          </button>
        </form>

        <!-- Fase 1b: verifica identità Condomino -->
        <template v-else>
          <div class="condomino-banner">
            <div class="condomino-banner__icon">🏠</div>
            <div>
              <strong>Account già registrato come Condomino</strong>
              <p>L'email <em>{{ form.email }}</em> è già presente come Condomino. Inserisci la tua password attuale per procedere con la registrazione come Amministratore.</p>
            </div>
          </div>
          <form @submit.prevent="handleCondominoVerify" novalidate style="display:flex;flex-direction:column;gap:14px;margin-top:16px">
            <div class="field-group">
              <label class="med-label">Password attuale</label>
              <input class="med-input" :class="{ 'med-input--error': errors.currentPassword }"
                v-model="form.currentPassword" type="password" placeholder="La tua password di accesso attuale"
                autocomplete="current-password" autofocus />
              <span class="error-msg" v-if="errors.currentPassword">{{ errors.currentPassword }}</span>
            </div>
            <div v-if="apiError" class="api-error">⚠ {{ apiError }}</div>
            <button type="submit" class="med-btn" :disabled="loading">
              <span v-if="loading" class="btn-spinner"></span>
              <span v-else>Verifica identità</span>
              <span v-if="!loading" style="font-size:16px">→</span>
            </button>
            <button type="button" class="med-btn med-btn--ghost" @click="resetStep1">
              ← Cambia email
            </button>
          </form>
        </template>

        <p class="signin-link">
          Hai già un account? <RouterLink to="/login" class="signin-link__a">Accedi</RouterLink>
        </p>
      </template>

      <!-- ── STEP 2: Studio ── -->
      <template v-if="step === 2">
        <h2 class="step-title">Il tuo studio</h2>
        <p class="med-subtitle">Come si chiama il tuo studio o la tua società di amministrazione?</p>

        <form @submit.prevent="handleStep2" novalidate>
          <div class="field-group">
            <label class="med-label">Nome studio / azienda</label>
            <input class="med-input" :class="{ 'med-input--error': errors.tenantName }"
              v-model="form.tenantName" placeholder="Es. Studio Rossi Amministrazioni" />
            <span class="error-msg" v-if="errors.tenantName">{{ errors.tenantName }}</span>
          </div>
          <div v-if="apiError" class="api-error">⚠ {{ apiError }}</div>
          <button type="submit" class="med-btn" :disabled="loading">
            <span v-if="loading" class="btn-spinner"></span>
            <span v-else>Continua</span>
            <span v-if="!loading" style="font-size:16px">→</span>
          </button>
        </form>
      </template>

      <!-- ── STEP 3: Primo condominio ── -->
      <template v-if="step === 3">
        <h2 class="step-title">Il primo condominio</h2>
        <p class="med-subtitle">Aggiungi il tuo primo condominio. Potrai aggiungerne altri in seguito.</p>

        <form @submit.prevent="handleStep3" novalidate>
          <div class="field-group">
            <label class="med-label">Denominazione condominio</label>
            <input class="med-input" :class="{ 'med-input--error': errors.condominiumName }"
              v-model="form.condominiumName" placeholder="Es. Condominio Primavera" />
            <span class="error-msg" v-if="errors.condominiumName">{{ errors.condominiumName }}</span>
          </div>
          <div class="field-group">
            <label class="med-label">Codice (opzionale)</label>
            <input class="med-input" v-model="form.condominiumCode" placeholder="Es. COND-001" />
          </div>
          <div class="two-col">
            <div class="field-group">
              <label class="med-label">Città</label>
              <input class="med-input" v-model="form.condominiumCity" placeholder="Milano" />
            </div>
            <div class="field-group">
              <label class="med-label">CAP</label>
              <input class="med-input" v-model="form.condominiumZip" placeholder="20100" />
            </div>
          </div>
          <div v-if="apiError" class="api-error">⚠ {{ apiError }}</div>
          <div class="btn-group">
            <button type="button" class="med-btn med-btn--ghost" @click="skipStep3" :disabled="loading">Salta per ora</button>
            <button type="submit" class="med-btn" :disabled="loading">
              <span v-if="loading" class="btn-spinner"></span>
              <span v-else>Continua</span>
              <span v-if="!loading" style="font-size:16px">→</span>
            </button>
          </div>
        </form>
      </template>

      <!-- ── STEP 4: Welcome ── -->
      <template v-if="step === 4">
        <div class="welcome-icon">🌊</div>
        <h2 class="step-title">Benvenuto in DomuWave!</h2>
        <p class="med-subtitle">Il tuo account è pronto. La prova gratuita è attiva per 30 giorni.</p>

        <ul class="checklist">
          <li class="checklist__item checklist__item--done">Account creato</li>
          <li class="checklist__item checklist__item--done">Studio configurato</li>
          <li class="checklist__item" :class="condominiumCreated ? 'checklist__item--done' : 'checklist__item--skip'">
            {{ condominiumCreated ? 'Primo condominio aggiunto' : 'Condominio (da aggiungere)' }}
          </li>
          <li class="checklist__item checklist__item--done">Prova gratuita attivata</li>
        </ul>

        <div v-if="apiError" class="api-error">⚠ {{ apiError }}</div>
        <button class="med-btn" @click="goToDashboard" :disabled="loading">
          <span v-if="loading" class="btn-spinner"></span>
          <span v-else>Vai alla dashboard</span>
          <span v-if="!loading" style="font-size:16px">→</span>
        </button>
      </template>
    </div>

    <p class="reg-footer">© {{ new Date().getFullYear() }} VizaSoft S.r.l. — Tutti i diritti riservati</p>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi, condominiumApi, licenseApi } from '@/services/api'
import { useAuthStore } from '@/stores/authStore'
import { useMenuStore } from '@/stores/menuStore'

const router    = useRouter()
const authStore = useAuthStore()
const menuStore = useMenuStore()

const step                  = ref(1)
const loading               = ref(false)
const apiError              = ref('')
const errors                = reactive({})
const condominiumCreated    = ref(false)
const registrationId        = ref(null)   // GUID restituito da selfRegister
const awaitingCondominoVerify = ref(false) // true = email è Condomino → chiedi verifica

const form = reactive({
  email: '', password: '', confirmPassword: '', acceptTerms: false,
  currentPassword: '',   // usato solo per verifica Condomino
  tenantName: '',
  condominiumName: '', condominiumCode: '', condominiumCity: '', condominiumZip: '',
})

function clearErrors() { Object.keys(errors).forEach(k => delete errors[k]) }

function resetStep1() {
  awaitingCondominoVerify.value = false
  form.currentPassword = ''
  clearErrors()
  apiError.value = ''
}

// ── Step 1: validazione locale → check email → eventuale verifica Condomino
async function handleStep1() {
  clearErrors()
  apiError.value = ''
  if (!form.email.trim())       errors.email = "L'email è obbligatoria"
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) errors.email = 'Email non valida'
  if (!form.password)           errors.password = 'La password è obbligatoria'
  else if (form.password.length < 8) errors.password = 'Minimo 8 caratteri'
  if (form.password && form.confirmPassword !== form.password) errors.confirmPassword = 'Le password non coincidono'
  if (!form.acceptTerms)        errors.acceptTerms = 'Devi accettare i termini di servizio'
  if (Object.keys(errors).length > 0) return

  loading.value = true
  try {
    const res = await authApi.checkEmail({ email: form.email.trim() })
    if (res.data.isExistingCondomino) {
      awaitingCondominoVerify.value = true
    } else {
      step.value = 2
    }
  } catch (err) {
    const d = err.response?.data
    apiError.value = typeof d === 'string' ? d : d?.message ?? d?.title ?? 'Errore di rete. Riprova.'
  } finally {
    loading.value = false
  }
}

// ── Step 1b: verifica identità Condomino tramite login ──────────────────
async function handleCondominoVerify() {
  clearErrors()
  apiError.value = ''
  if (!form.currentPassword) { errors.currentPassword = 'Inserisci la password attuale'; return }

  loading.value = true
  try {
    await authApi.login({ email: form.email.trim(), password: form.currentPassword })
    // Login riuscito → identità verificata, prosegui
    awaitingCondominoVerify.value = false
    step.value = 2
  } catch (err) {
    const status = err.response?.status
    if (status === 404 || status === 401) {
      errors.currentPassword = 'Password non corretta. Riprova.'
    } else {
      const d = err.response?.data
      apiError.value = typeof d === 'string' ? d : d?.message ?? d?.title ?? 'Errore di verifica. Riprova.'
    }
  } finally {
    loading.value = false
  }
}

// ── Step 2: salva in staging → riceve registrationId ────────────────────
async function handleStep2() {
  clearErrors()
  if (!form.tenantName.trim()) { errors.tenantName = 'Il nome dello studio è obbligatorio'; return }
  loading.value = true
  apiError.value = ''
  try {
    const res = await authApi.selfRegister({
      email:      form.email,
      password:   form.password,
      tenantName: form.tenantName.trim(),
    })
    registrationId.value = res.data.registrationId
    step.value = 3
  } catch (err) {
    const d = err.response?.data
    apiError.value = typeof d === 'string' ? d : d?.message ?? d?.title ?? 'Errore durante la registrazione. Riprova.'
  } finally {
    loading.value = false
  }
}

// ── Step 3: crea condominio (opzionale), poi conferma registrazione ──────
async function handleStep3() {
  clearErrors()
  if (!form.condominiumName.trim()) { errors.condominiumName = 'La denominazione è obbligatoria'; return }
  loading.value = true
  apiError.value = ''
  try {
    // Prima conferma registrazione (crea utente + tenant)
    await confirmRegistration()
    // Poi crea condominio con il tenant già attivo
    await condominiumApi.create({
      name:    form.condominiumName.trim(),
      code:    form.condominiumCode.trim() || null,
      address: {
        city:       form.condominiumCity.trim() || null,
        postalCode: form.condominiumZip.trim()  || null,
      }
    })
    condominiumCreated.value = true
    await activateTrial()
  } catch (err) {
    if (!condominiumCreated.value) {
      const d = err.response?.data
      apiError.value = typeof d === 'string' ? d : d?.message ?? d?.title ?? 'Si è verificato un errore. Riprova.'
      loading.value = false
    }
  }
}

async function skipStep3() {
  loading.value = true
  apiError.value = ''
  try {
    await confirmRegistration()
    await activateTrial()
  } catch (err) {
    const d = err.response?.data
    apiError.value = typeof d === 'string' ? d : d?.message ?? d?.title ?? 'Si è verificato un errore. Riprova.'
    loading.value = false
  }
}

// ── Conferma registrazione: crea utente + tenant con l'Id del pending ───
async function confirmRegistration() {
  const res  = await authApi.confirmRegistration({ registrationId: registrationId.value })
  const data = res.data
  // Imposta token e tenant in localStorage così le successive chiamate api li trovano
  localStorage.setItem('domuwave_token', data.token ?? '')
  localStorage.setItem('domuwave_user', JSON.stringify({ id: data.userId, username: form.email }))
  localStorage.setItem('tenantId',   data.tenantId)
  localStorage.setItem('tenantName', data.tenantName)
}

// ── Attiva piano trial su LicenseManager ────────────────────────────────
async function activateTrial() {
  try {
    await licenseApi.register({
      firstName: '', lastName: '',
      email:      form.email,
      password:   form.password,
      tenantName: form.tenantName.trim(),
    })
  } catch {
    // trial non critico — continua comunque verso step 4
  }
  loading.value = false
  step.value = 4
}

// ── Step 4: login completo e redirect ────────────────────────────────────
async function goToDashboard() {
  loading.value = true
  apiError.value = ''
  try {
    const result = await authStore.login(form.email, form.password)
    if (result?.success) {
      await menuStore.fetchMenu()
      router.push('/dashboard')
    } else {
      router.push({ path: '/login', query: { email: form.email, registered: '1' } })
    }
  } catch {
    router.push({ path: '/login', query: { email: form.email, registered: '1' } })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&family=Inter:wght@300;400;500;600;700&display=swap');

.reg-wrapper {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 24px;
  padding: 40px 16px 60px;
  background: linear-gradient(180deg, #fdfaf2 0%, #f6efe0 100%);
  position: relative;
  overflow: hidden;
  font-family: 'Inter', sans-serif;
}

.med-wave {
  position: absolute;
  bottom: 0; left: 0; right: 0;
  height: 140px;
  pointer-events: none;
  opacity: 0.55;
}

.reg-card {
  width: 100%;
  max-width: 460px;
  background: rgba(255,255,255,0.92);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255,255,255,0.9);
  border-radius: 22px;
  box-shadow:
    0 30px 70px -20px rgba(14,39,64,0.18),
    0 8px 20px -8px rgba(82,156,210,0.15),
    inset 0 1px 0 rgba(255,255,255,0.95);
  padding: 44px 44px 38px;
  position: relative;
  z-index: 1;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to   { opacity: 1; transform: translateY(0); }
}
.slide-up { animation: slideUp 0.6s cubic-bezier(0.4,0,0.2,1) forwards; }

.med-brand {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  font-family: 'Instrument Serif', serif;
  font-weight: 400;
  font-size: 26px;
  color: #0e2740;
  letter-spacing: -0.025em;
  margin-bottom: 24px;
}
.med-brand em { font-style: italic; color: #1e6db8; }
.med-brand-mark {
  width: 42px; height: 42px;
  border-radius: 14px;
  background: linear-gradient(135deg, #1e6db8 0%, #2a8bc7 50%, #f4c842 130%);
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 8px 18px -4px rgba(30,109,184,0.4);
  position: relative; overflow: hidden; flex-shrink: 0;
}
.med-brand-mark::after {
  content: '';
  position: absolute; inset: 0;
  background: linear-gradient(135deg, rgba(255,255,255,0.3), transparent 60%);
}

.step-indicator {
  display: flex;
  align-items: center;
  margin-bottom: 28px;
  position: relative;
}
.step-line {
  position: absolute;
  top: 50%; left: 16px; right: 16px;
  height: 2px;
  background: rgba(82,156,210,0.2);
  z-index: 0;
}
.step-dot {
  width: 32px; height: 32px;
  border-radius: 50%;
  border: 2px solid rgba(82,156,210,0.3);
  display: flex; align-items: center; justify-content: center;
  font-size: 12px; font-weight: 600;
  color: #a0b3c7;
  background: #fff;
  position: relative; z-index: 1; flex-shrink: 0;
}
.step-dot + .step-dot { margin-left: auto; }
.step-dot.active { border-color: #1e6db8; color: #1e6db8; background: rgba(30,109,184,0.08); }
.step-dot.done   { border-color: #22c55e; color: #fff; background: #22c55e; }

.med-tag {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 6px 13px;
  border-radius: 100px;
  background: linear-gradient(135deg, rgba(244,200,66,0.2), rgba(244,200,66,0.08));
  color: #b08a1a; font-size: 11.5px; font-weight: 600;
  border: 1px solid rgba(244,200,66,0.4);
  margin-bottom: 20px;
}

.med-headline {
  font-family: 'Instrument Serif', serif;
  font-weight: 400; font-size: 36px;
  line-height: 1.08; letter-spacing: -0.025em;
  color: #0e2740; margin-bottom: 14px;
}
.sun-word {
  background: linear-gradient(135deg, #f4c842, #f29e3a);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
  font-style: italic;
}
.step-title {
  font-family: 'Instrument Serif', serif;
  font-weight: 400; font-size: 28px; line-height: 1.1;
  color: #0e2740; margin-bottom: 10px;
}
.med-subtitle { font-size: 14.5px; color: #56739a; line-height: 1.6; margin-bottom: 28px; }

form { display: flex; flex-direction: column; gap: 14px; }
.two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.field-group { display: flex; flex-direction: column; gap: 6px; }

.med-label { font-size: 11.5px; font-weight: 500; color: #56739a; text-transform: uppercase; letter-spacing: 0.08em; }
.med-input {
  width: 100%;
  background: rgba(255,255,255,0.65);
  border: 1.5px solid rgba(82,156,210,0.18);
  border-radius: 14px; padding: 13px 16px;
  color: #0e2740; font-family: 'Inter', sans-serif; font-size: 14.5px;
  outline: none; transition: all 0.25s; box-sizing: border-box;
}
.med-input:focus { border-color: #1e6db8; background: #fff; box-shadow: 0 0 0 4px rgba(30,109,184,0.1); }
.med-input::placeholder { color: #a0b3c7; }
.med-input--error { border-color: #e05252; }
.error-msg { font-size: 12px; color: #e05252; }

.check-label {
  display: flex; align-items: flex-start; gap: 10px;
  font-size: 13px; color: #56739a; cursor: pointer; line-height: 1.5;
}
.check-label input[type=checkbox] { margin-top: 2px; flex-shrink: 0; }
.link { color: #1e6db8; }

.api-error {
  background: rgba(224,82,82,0.08); border: 1px solid rgba(224,82,82,0.25);
  border-radius: 12px; padding: 12px 16px; font-size: 13.5px; color: #b33a3a;
}

.med-btn {
  width: 100%;
  background: linear-gradient(135deg, #1e6db8, #2a8bc7);
  color: #fff; border: none; padding: 15px;
  font-family: 'Inter', sans-serif; font-size: 14.5px; font-weight: 600;
  cursor: pointer; border-radius: 14px; transition: all 0.25s;
  box-shadow: 0 8px 22px -4px rgba(30,109,184,0.4), inset 0 1px 0 rgba(255,255,255,0.2);
  display: flex; align-items: center; justify-content: center; gap: 8px; margin-top: 4px;
}
.med-btn:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 12px 30px -4px rgba(30,109,184,0.5); }
.med-btn:disabled { opacity: 0.7; cursor: not-allowed; }
.med-btn--ghost {
  background: transparent; color: #56739a;
  border: 1.5px solid rgba(82,156,210,0.3); box-shadow: none;
}
.med-btn--ghost:hover:not(:disabled) { background: rgba(82,156,210,0.06); transform: none; box-shadow: none; }
.btn-group { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }

.btn-spinner {
  width: 18px; height: 18px;
  border: 2px solid rgba(255,255,255,0.4); border-top-color: #fff;
  border-radius: 50%; animation: spin .7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.signin-link { margin-top: 24px; font-size: 13px; color: #56739a; text-align: center; }
.signin-link__a {
  color: #1e6db8; font-family: 'Instrument Serif', serif;
  font-style: italic; font-size: 15px; text-decoration: none;
}
.signin-link__a:hover { text-decoration: underline; }

.welcome-icon { font-size: 48px; margin-bottom: 12px; }
.checklist { list-style: none; padding: 0; margin: 0 0 28px; display: flex; flex-direction: column; gap: 10px; }
.checklist__item { display: flex; align-items: center; gap: 10px; font-size: 14px; color: #56739a; }
.checklist__item::before { content: '○'; font-size: 16px; color: #a0b3c7; }
.checklist__item--done { color: #0e2740; font-weight: 500; }
.checklist__item--done::before { content: '✓'; color: #22c55e; font-weight: 700; }
.checklist__item--skip::before { content: '—'; color: #a0b3c7; }

.reg-footer { font-size: 12px; color: #a0b3c7; text-align: center; position: relative; z-index: 1; }

.condomino-banner {
  display: flex;
  gap: 14px;
  align-items: flex-start;
  background: rgba(30,109,184,0.07);
  border: 1.5px solid rgba(30,109,184,0.22);
  border-radius: 14px;
  padding: 16px 18px;
  margin-bottom: 4px;
}
.condomino-banner__icon { font-size: 26px; flex-shrink: 0; line-height: 1; }
.condomino-banner strong { font-size: 14px; color: #0e2740; display: block; margin-bottom: 6px; }
.condomino-banner p { font-size: 13px; color: #56739a; margin: 0; line-height: 1.55; }
.condomino-banner em { font-style: normal; font-weight: 600; color: #1e6db8; }
</style>
