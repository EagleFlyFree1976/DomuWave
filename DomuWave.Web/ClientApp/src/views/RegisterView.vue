<template>
  <div class="reg-shell">
    <div class="grain"></div>

    <!-- ── Colonna sinistra: brand + marketing ── -->
    <aside class="reg-aside">
      <div class="aurora"><i></i><i></i></div>

      <RouterLink class="logo" to="/">
        <span class="logo-mark">
          <svg viewBox="0 0 40 40" fill="none">
            <rect x="4" y="14" width="14" height="22" rx="2" fill="#2e9c6c" />
            <rect x="22" y="6" width="14" height="30" rx="2" fill="#c9a55c" />
            <rect x="8" y="19" width="6" height="3" rx="1" fill="#f6f2e9" opacity="0.85" />
            <rect x="8" y="26" width="6" height="3" rx="1" fill="#f6f2e9" opacity="0.85" />
            <rect x="26" y="11" width="6" height="3" rx="1" fill="#0c1f17" opacity="0.5" />
            <rect x="26" y="18" width="6" height="3" rx="1" fill="#0c1f17" opacity="0.5" />
            <rect x="26" y="25" width="6" height="3" rx="1" fill="#0c1f17" opacity="0.5" />
          </svg>
        </span>
        DomuWave
      </RouterLink>

      <div class="aside-body">
        <div class="eyebrow">Inizia gratis</div>
        <h2 class="aside-title">Amministrare,<br>finalmente <em>sotto controllo.</em></h2>
        <p class="aside-lead">
          Bastano due minuti per attivare il tuo primo condominio.
          Contabilità, millesimi, assemblee e comunicazioni in un unico posto.
        </p>

        <ul class="aside-list">
          <li><span class="ck"><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span>1 condominio completo, gratis per sempre</li>
          <li><span class="ck"><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span>Contabilità e tabelle millesimali incluse</li>
          <li><span class="ck"><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5" /></svg></span>Nessuna carta di credito richiesta</li>
        </ul>
      </div>

      <p class="aside-foot">© {{ new Date().getFullYear() }} VizaSoft S.r.l. — Tutti i diritti riservati</p>
    </aside>

    <!-- ── Colonna destra: form ── -->
    <main class="reg-main">
      <div class="reg-card slide-up">
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
          <h1 class="med-headline">Crea il tuo account</h1>
          <p class="med-subtitle">Bastano 30 secondi per iniziare la prova gratuita.</p>

          <!-- Fase 1a: inserimento email + nuova password -->
          <form v-if="!awaitingCondominoVerify && !existingAdmin" @submit.prevent="handleStep1" novalidate>
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

          <!-- Fase 1c: account amministratore già esistente → invito al login -->
          <template v-else-if="existingAdmin">
            <div class="condomino-banner">
              <div class="condomino-banner__icon">👤</div>
              <div>
                <strong>Account già esistente</strong>
                <p>L'email <em>{{ form.email }}</em> è già associata a un account amministratore. Accedi alla piattaforma con le tue credenziali.</p>
              </div>
            </div>
            <RouterLink
              :to="{ path: '/login', query: { email: form.email.trim() } }"
              class="med-btn"
              style="display:flex;align-items:center;justify-content:center;gap:8px;text-decoration:none;margin-top:16px">
              Vai al login <span style="font-size:16px">→</span>
            </RouterLink>
            <button type="button" class="med-btn med-btn--ghost" style="margin-top:10px" @click="resetStep1">
              ← Usa un'altra email
            </button>
          </template>

          <!-- Fase 1b: verifica identità Condomino -->
          <template v-else-if="awaitingCondominoVerify">
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

        <!-- ── STEP 4: Controlla la posta ── -->
        <template v-if="step === 4">
          <div class="welcome-icon">📧</div>
          <h2 class="step-title">Controlla la tua casella</h2>
          <p class="med-subtitle">
            Abbiamo inviato un'email di conferma a <strong>{{ sentEmail || form.email }}</strong>.
            Clicca il link nell'email per completare la registrazione e attivare il tuo account.
          </p>
          <p class="med-subtitle" style="font-size:13px;color:var(--ink-mute);margin-top:16px">
            Non hai ricevuto l'email? Controlla la cartella spam, oppure
            <a href="#" class="link" @click.prevent="resendVerification">invia di nuovo</a>.
          </p>
          <div v-if="apiError" class="api-error">⚠ {{ apiError }}</div>
          <div v-if="resendOk" class="api-ok">✓ Email inviata di nuovo.</div>
        </template>
      </div>
    </main>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { authApi } from '@/services/api'

const step                  = ref(1)
const loading               = ref(false)
const apiError              = ref('')
const errors                = reactive({})
const registrationId        = ref(null)   // GUID restituito da selfRegister
const sentEmail             = ref('')     // email a cui è stata inviata la verifica
const resendOk              = ref(false)
const awaitingCondominoVerify = ref(false) // true = email è Condomino → chiedi verifica
const existingAdmin           = ref(false) // true = email già amministratore → invita al login

const form = reactive({
  email: '', password: '', confirmPassword: '', acceptTerms: false,
  currentPassword: '',   // usato solo per verifica Condomino
  tenantName: '',
  condominiumName: '', condominiumCode: '', condominiumCity: '', condominiumZip: '',
})

function clearErrors() { Object.keys(errors).forEach(k => delete errors[k]) }

// Estrae un messaggio leggibile dalla response di errore.
// Il backend CPQ.Core risponde { "Errors": ["..."] }; gestiamo anche message/title/string.
function extractApiError(err, fallback = 'Si è verificato un errore. Riprova.') {
  const d = err?.response?.data
  if (!d) return err?.message || fallback
  if (typeof d === 'string') return d
  const errs = d.Errors ?? d.errors
  if (Array.isArray(errs) && errs.length) return errs.join(' ')
  return d.message ?? d.title ?? d.detail ?? fallback
}

function resetStep1() {
  awaitingCondominoVerify.value = false
  existingAdmin.value = false
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
    if (res.data.isExistingAdmin) {
      // Account amministratore già esistente → deve accedere, non registrarsi
      existingAdmin.value = true
    } else if (res.data.isExistingCondomino) {
      awaitingCondominoVerify.value = true
    } else {
      step.value = 2
    }
  } catch (err) {
    apiError.value = extractApiError(err, 'Errore di rete. Riprova.')
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
      apiError.value = extractApiError(err, 'Errore di verifica. Riprova.')
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
    apiError.value = extractApiError(err, 'Errore durante la registrazione. Riprova.')
  } finally {
    loading.value = false
  }
}

// ── Step 3: salva i dati condominio + invia mail di verifica ─────────────
async function handleStep3() {
  clearErrors()
  if (!form.condominiumName.trim()) { errors.condominiumName = 'La denominazione è obbligatoria'; return }
  await sendVerification()
}

async function skipStep3() {
  await sendVerification()
}

async function sendVerification() {
  loading.value = true
  apiError.value = ''
  resendOk.value = false
  try {
    const res = await authApi.requestVerification({
      registrationId:  registrationId.value,
      condominiumName: form.condominiumName.trim() || null,
      condominiumCode: form.condominiumCode.trim() || null,
      condominiumCity: form.condominiumCity.trim() || null,
      condominiumZip:  form.condominiumZip.trim()  || null,
    })
    sentEmail.value = res.data.email ?? form.email
    step.value = 4
  } catch (err) {
    apiError.value = extractApiError(err, 'Impossibile inviare l\'email di verifica. Riprova.')
  } finally {
    loading.value = false
  }
}

async function resendVerification() {
  await sendVerification()
  if (!apiError.value) resendOk.value = true
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600&display=swap');

.reg-shell {
  --night: #0c1f17;
  --pine: #1b4a35;
  --emerald: #2e9c6c;
  --mint: #8fd6b4;
  --cream: #f6f2e9;
  --cream-2: #ece6d8;
  --ink: #14201a;
  --ink-soft: #4d5c54;
  --ink-mute: #8a978f;
  --brass: #c9a55c;
  --brass-soft: #e3cf9e;
  --line-light: rgba(20, 32, 26, 0.1);

  min-height: 100vh;
  display: grid;
  grid-template-columns: 0.95fr 1.05fr;
  font-family: 'Outfit', sans-serif;
  background: var(--cream);
  color: var(--ink);
  position: relative;
  overflow: hidden;
}

/* grain overlay */
.grain {
  position: fixed; inset: -50%; width: 200%; height: 200%; pointer-events: none; z-index: 100; opacity: 0.04;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='240' height='240'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
}

/* ── Aside (brand) ── */
.reg-aside {
  background: var(--night);
  color: var(--cream);
  padding: 44px 52px;
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
}
.aurora { position: absolute; inset: 0; pointer-events: none; z-index: 0; }
.aurora i { position: absolute; border-radius: 50%; filter: blur(90px); mix-blend-mode: screen; }
.aurora i:nth-child(1) { width: 420px; height: 420px; background: #2e9c6c; top: -140px; right: -100px; opacity: .4; animation: drift 16s ease-in-out infinite alternate; }
.aurora i:nth-child(2) { width: 340px; height: 340px; background: #c9a55c; bottom: -140px; left: -90px; opacity: .22; animation: drift 20s ease-in-out infinite alternate-reverse; }
@keyframes drift { from { transform: translate(0, 0) scale(1); } to { transform: translate(-40px, 40px) scale(1.12); } }

.logo {
  display: inline-flex; align-items: center; gap: 11px;
  font-family: 'Clash Display', 'Outfit', sans-serif; font-weight: 500; font-size: 23px;
  color: var(--cream); letter-spacing: -0.01em; text-decoration: none;
  position: relative; z-index: 1;
}
.logo-mark { width: 34px; height: 34px; flex-shrink: 0; }
.logo-mark svg { width: 100%; height: 100%; }

.aside-body { margin: auto 0; position: relative; z-index: 1; }
.eyebrow {
  display: inline-flex; align-items: center; gap: 10px; font-size: 12.5px; font-weight: 600;
  letter-spacing: 0.16em; text-transform: uppercase; color: var(--brass-soft); margin-bottom: 22px;
}
.eyebrow::before { content: ""; width: 36px; height: 1px; background: var(--brass); }
.aside-title {
  font-family: 'Clash Display', 'Outfit', sans-serif; font-weight: 500;
  font-size: clamp(30px, 3.4vw, 44px); line-height: 1.1; letter-spacing: -0.015em; margin-bottom: 18px;
}
.aside-title em { font-style: normal; color: var(--mint); }
.aside-lead { font-size: 16.5px; font-weight: 300; color: rgba(246, 242, 233, 0.74); max-width: 420px; margin-bottom: 32px; }

.aside-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 14px; }
.aside-list li { display: flex; gap: 13px; align-items: center; font-size: 15px; font-weight: 300; color: rgba(246, 242, 233, 0.9); }
.aside-list .ck {
  width: 24px; height: 24px; border-radius: 7px; flex-shrink: 0;
  background: rgba(46, 156, 108, 0.18); border: 1px solid rgba(143, 214, 180, 0.3);
  display: flex; align-items: center; justify-content: center; color: var(--mint);
}
.aside-foot { font-size: 12px; color: rgba(246, 242, 233, 0.4); position: relative; z-index: 1; margin-top: 24px; }

/* ── Main (form) ── */
.reg-main {
  display: flex; align-items: center; justify-content: center;
  padding: 48px 32px; position: relative; z-index: 1;
}
.reg-card { width: 100%; max-width: 440px; }

@keyframes slideUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
.slide-up { animation: slideUp 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards; }

/* step indicator */
.step-indicator { display: flex; align-items: center; margin-bottom: 30px; position: relative; }
.step-line { position: absolute; top: 50%; left: 16px; right: 16px; height: 2px; background: rgba(27, 74, 53, 0.15); z-index: 0; }
.step-dot {
  width: 32px; height: 32px; border-radius: 50%; border: 2px solid rgba(27, 74, 53, 0.22);
  display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 600;
  color: var(--ink-mute); background: var(--cream); position: relative; z-index: 1; flex-shrink: 0;
}
.step-dot + .step-dot { margin-left: auto; }
.step-dot.active { border-color: var(--emerald); color: var(--pine); background: rgba(46, 156, 108, 0.1); }
.step-dot.done   { border-color: var(--emerald); color: var(--cream); background: var(--emerald); }

.med-tag {
  display: inline-flex; align-items: center; gap: 6px; padding: 6px 13px; border-radius: 100px;
  background: linear-gradient(135deg, rgba(201, 165, 92, 0.2), rgba(201, 165, 92, 0.08));
  color: #9a7a32; font-size: 11.5px; font-weight: 600; border: 1px solid rgba(201, 165, 92, 0.4); margin-bottom: 20px;
}

.med-headline {
  font-family: 'Clash Display', 'Outfit', sans-serif; font-weight: 500; font-size: 34px;
  line-height: 1.1; letter-spacing: -0.02em; color: var(--ink); margin-bottom: 12px;
}
.step-title {
  font-family: 'Clash Display', 'Outfit', sans-serif; font-weight: 500; font-size: 28px; line-height: 1.12;
  color: var(--ink); margin-bottom: 10px;
}
.med-subtitle { font-size: 14.5px; color: var(--ink-soft); line-height: 1.6; margin-bottom: 28px; font-weight: 300; }

form { display: flex; flex-direction: column; gap: 14px; }
.two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.field-group { display: flex; flex-direction: column; gap: 6px; }

.med-label { font-size: 11.5px; font-weight: 600; color: var(--ink-soft); text-transform: uppercase; letter-spacing: 0.08em; }
.med-input {
  width: 100%; background: #fff; border: 1.5px solid rgba(27, 74, 53, 0.14);
  border-radius: 13px; padding: 13px 16px; color: var(--ink);
  font-family: 'Outfit', sans-serif; font-size: 14.5px; outline: none; transition: all 0.25s; box-sizing: border-box;
}
.med-input:focus { border-color: var(--emerald); box-shadow: 0 0 0 4px rgba(46, 156, 108, 0.12); }
.med-input::placeholder { color: var(--ink-mute); }
.med-input--error { border-color: #d65a4a; }
.error-msg { font-size: 12px; color: #c44634; }

.check-label { display: flex; align-items: flex-start; gap: 10px; font-size: 13px; color: var(--ink-soft); cursor: pointer; line-height: 1.5; }
.check-label input[type=checkbox] { margin-top: 2px; flex-shrink: 0; accent-color: var(--emerald); }
.link { color: var(--pine); font-weight: 500; }

.api-error { background: rgba(214, 90, 74, 0.08); border: 1px solid rgba(214, 90, 74, 0.25); border-radius: 12px; padding: 12px 16px; font-size: 13.5px; color: #a73c2c; }
.api-ok { background: rgba(46, 156, 108, 0.1); border: 1px solid rgba(46, 156, 108, 0.3); border-radius: 12px; padding: 12px 16px; font-size: 13.5px; color: var(--pine); margin-top: 10px; }

.med-btn {
  width: 100%; background: var(--emerald); color: var(--cream); border: none; padding: 15px;
  font-family: 'Outfit', sans-serif; font-size: 14.5px; font-weight: 600; cursor: pointer;
  border-radius: 13px; transition: all 0.25s;
  box-shadow: 0 10px 28px -10px rgba(46, 156, 108, 0.6);
  display: flex; align-items: center; justify-content: center; gap: 8px; margin-top: 4px;
}
.med-btn:hover:not(:disabled) { transform: translateY(-1px); background: #2a8e62; box-shadow: 0 14px 34px -10px rgba(46, 156, 108, 0.7); }
.med-btn:disabled { opacity: 0.7; cursor: not-allowed; }
.med-btn--ghost { background: transparent; color: var(--ink-soft); border: 1.5px solid rgba(27, 74, 53, 0.22); box-shadow: none; }
.med-btn--ghost:hover:not(:disabled) { background: rgba(27, 74, 53, 0.05); transform: none; box-shadow: none; }
.btn-group { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }

.btn-spinner { width: 18px; height: 18px; border: 2px solid rgba(246, 242, 233, 0.4); border-top-color: var(--cream); border-radius: 50%; animation: spin .7s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.signin-link { margin-top: 24px; font-size: 13px; color: var(--ink-soft); text-align: center; }
.signin-link__a { color: var(--pine); font-weight: 600; text-decoration: none; }
.signin-link__a:hover { text-decoration: underline; }

.welcome-icon { font-size: 48px; margin-bottom: 12px; }

.condomino-banner {
  display: flex; gap: 14px; align-items: flex-start;
  background: rgba(46, 156, 108, 0.07); border: 1.5px solid rgba(46, 156, 108, 0.22);
  border-radius: 14px; padding: 16px 18px; margin-bottom: 4px;
}
.condomino-banner__icon { font-size: 26px; flex-shrink: 0; line-height: 1; }
.condomino-banner strong { font-size: 14px; color: var(--ink); display: block; margin-bottom: 6px; }
.condomino-banner p { font-size: 13px; color: var(--ink-soft); margin: 0; line-height: 1.55; }
.condomino-banner em { font-style: normal; font-weight: 600; color: var(--pine); }

/* ── Responsive: collassa a una colonna ── */
@media (max-width: 880px) {
  .reg-shell { grid-template-columns: 1fr; }
  .reg-aside { padding: 32px 28px; }
  .reg-aside .aside-body { margin: 28px 0; }
  .aside-list { display: none; }
  .reg-main { padding: 36px 24px 56px; }
}
</style>
