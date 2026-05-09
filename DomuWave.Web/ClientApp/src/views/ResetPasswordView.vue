<template>
  <div class="login-wrapper">
    <!-- Background decorative elements -->
    <div class="login-bg">
      <div class="bg-wave bg-wave-1"></div>
      <div class="bg-wave bg-wave-2"></div>
      <div class="bg-orb bg-orb-1"></div>
      <div class="bg-orb bg-orb-2"></div>
    </div>

    <div class="login-container">
      <!-- Logo / Brand -->
      <!--<div class="login-brand">
        <div class="brand-icon">
          <i class="pi pi-building"></i>
        </div>
        <div class="brand-text">
          <h1>DomuWave</h1>
          <span>Gestione Condomini</span>
        </div>
      </div>-->

      <!-- Card -->
      <div class="login-card">
        <div class="card-logo">
          <img src="../assets/logostudiogalli.png" alt="Studio Amministrativo Galli" />
        </div>
        <!-- Richiesta reset (senza token) -->
        <template v-if="!token">
          <div class="card-header">
            <h2>Recupero password</h2>
            <p>Inserisci la tua email per ricevere il link di reimpostazione.</p>
          </div>

          <form @submit.prevent="handleRequestReset" class="login-form" novalidate>
            <div class="field">
              <label for="email">Email</label>
              <div class="input-wrapper" :class="{ 'has-error': requestV$.email.$error }">
                <i class="pi pi-envelope input-icon"></i>
                <InputText id="email"
                           v-model="requestForm.email"
                           placeholder="nome@dominio.it"
                           autocomplete="email"
                           :class="{ 'p-invalid': requestV$.email.$error }"
                           @blur="requestV$.email.$touch()" />
              </div>
              <small class="error-msg" v-if="requestV$.email.$error">
                {{ requestV$.email.$errors[0].$message }}
              </small>
            </div>

            <Message v-if="requestError" severity="error" :closable="false" class="login-error">
              <i class="pi pi-exclamation-triangle" style="margin-right:6px"></i>
              {{ requestError }}
            </Message>

            <Message v-if="requestSuccess" severity="success" :closable="false" class="login-success">
              <i class="pi pi-check" style="margin-right:6px"></i>
              Se l'email esiste, riceverai un link per reimpostare la password.
            </Message>

            <Button type="submit"
                    label="Invia link di reset"
                    icon="pi pi-send"
                    icon-pos="right"
                    class="login-btn"
                    :loading="requestLoading"
                    :disabled="requestLoading" />
          </form>

          <router-link to="/login" class="helper-link">
            <i class="pi pi-arrow-left" style="margin-right:0.5rem"></i> Torna al login
          </router-link>
        </template>

        <!-- Successo -->
        <template v-else-if="state === 'success'">
          <div class="card-header">
            <div class="state-icon state-icon--success"><i class="pi pi-check"></i></div>
            <h2>Password aggiornata</h2>
            <p>La tua password è stata reimpostata con successo. Puoi ora accedere con le nuove credenziali.</p>
          </div>
          <router-link to="/login" class="login-btn p-button p-component" style="text-decoration:none;display:flex;justify-content:center">
            <i class="pi pi-sign-in" style="margin-right:0.5rem"></i> Vai al login
          </router-link>
        </template>

        <!-- Form -->
        <template v-else>
          <div class="card-header">
            <h2>Nuova password</h2>
            <p>Scegli una nuova password sicura per il tuo account.</p>
          </div>

          <form @submit.prevent="handleSubmit" class="login-form" novalidate>

            <!-- Nuova password -->
            <div class="field">
              <label for="newPassword">Nuova password</label>
              <div class="input-wrapper" :class="{ 'has-error': v$.newPassword.$error }">
                <i class="pi pi-lock input-icon"></i>
                <Password id="newPassword"
                          v-model="form.newPassword"
                          placeholder="Almeno 8 caratteri"
                          toggle-mask
                          autocomplete="new-password"
                          :class="{ 'p-invalid': v$.newPassword.$error }"
                          @blur="v$.newPassword.$touch()" />
              </div>
              <small class="error-msg" v-if="v$.newPassword.$error">
                {{ v$.newPassword.$errors[0].$message }}
              </small>
            </div>

            <!-- Conferma password -->
            <div class="field">
              <label for="confirmPassword">Conferma password</label>
              <div class="input-wrapper" :class="{ 'has-error': v$.confirmPassword.$error }">
                <i class="pi pi-lock input-icon"></i>
                <Password id="confirmPassword"
                          v-model="form.confirmPassword"
                          placeholder="Ripeti la password"
                          :feedback="false"
                          toggle-mask
                          autocomplete="new-password"
                          :class="{ 'p-invalid': v$.confirmPassword.$error }"
                          @blur="v$.confirmPassword.$touch()" />
              </div>
              <small class="error-msg" v-if="v$.confirmPassword.$error">
                {{ v$.confirmPassword.$errors[0].$message }}
              </small>
            </div>

            <!-- API Error -->
            <Message v-if="apiError" severity="error" :closable="false" class="login-error">
              <i class="pi pi-exclamation-triangle" style="margin-right:6px"></i>
              {{ apiError }}
            </Message>

            <!-- Submit -->
            <Button type="submit"
                    label="Reimposta password"
                    icon="pi pi-check"
                    icon-pos="right"
                    class="login-btn"
                    :loading="loading"
                    :disabled="loading" />
          </form>
        </template>

      </div>

      <p class="login-footer">
        © {{ new Date().getFullYear() }} VizaSoft S.r.l. — Tutti i diritti riservati
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useVuelidate } from '@vuelidate/core'
import { required, helpers, minLength, sameAs, email } from '@vuelidate/validators'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/authStore'
import authApiClient from '@/services/authApiClient'

const route = useRoute()
  const authStore = useAuthStore()
const token   = computed(() => route.query.token ?? '')
const state   = ref('pending')   // 'pending' | 'success'
const loading = ref(false)
const apiError = ref('')

const requestLoading = ref(false)
const requestError = ref('')
const requestSuccess = ref(false)

const form = reactive({
  newPassword:     '',
  confirmPassword: '',
})

const requestForm = reactive({
  email: route.query.email ?? '',
})

const rules = {
  newPassword: {
    required:  helpers.withMessage('La password è obbligatoria', required),
    minLength: helpers.withMessage('Minimo 8 caratteri', minLength(8)),
  },
  confirmPassword: {
    required:  helpers.withMessage('Conferma la password', required),
    sameAs:    helpers.withMessage('Le password non corrispondono', sameAs(computed(() => form.newPassword))),
  },
}

const requestRules = {
  email: {
    required: helpers.withMessage("L'email è obbligatoria", required),
    email: helpers.withMessage('Inserisci un indirizzo email valido', email),
  },
}

const v$ = useVuelidate(rules, form)
const requestV$ = useVuelidate(requestRules, requestForm)

function getErrorMessage(err) {
  const data = err.response?.data
  const errs = data?.Errors ?? data?.errors
  if (Array.isArray(errs) && errs.length) return errs.join('\n')
  if (data?.message) return data.message
  if (data?.title) return data.title
  if (!err.response) return 'Impossibile raggiungere il server'
  return 'Si è verificato un errore. Riprova o richiedi un nuovo link.'
}

async function handleRequestReset() {
  requestError.value = ''
  requestSuccess.value = false
  const valid = await requestV$.value.$validate()
  if (!valid) return

  requestLoading.value = true
  try {
    const result = await authStore.requestReset(requestForm.email)
  
    requestSuccess.value = true
  } catch (err) {
    requestError.value = getErrorMessage(err)
  } finally {
    requestLoading.value = false
  }
}

  async function handleSubmit() {
    console.log("handleSubmit");
  apiError.value = ''
    const valid = await v$.value.$validate()
    console.log("handleSubmit valid :" + valid);
  if (!valid) return

  loading.value = true
    try {
      console.log("handleSubmit call  auth/confirm-reset-password:", {
        token: token.value,
        newPassword: form.newPassword,
      });
    await authApiClient.post('/auth/confirm-reset-password', {
      token:       token.value,
      newPassword: form.newPassword,
    })
      console.log("handleSubmit CALLED  auth/confirm-reset-password:", {
        token: token.value,
        newPassword: form.newPassword,
      });
    state.value = 'success'
    } catch (err) {
      console.log("handleSubmit err", err)
    apiError.value = getErrorMessage(err)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* ── Layout ───────────────────────────────────────────────────────────────── */
.login-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-surface-ground);
  position: relative;
  overflow: hidden;
  font-family: 'DM Sans', sans-serif;
  background-image: url(../assets/sfondodomuweb_viza.png);
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

.login-bg { position: absolute; inset: 0; pointer-events: none; z-index: 0; }

.bg-wave { position: absolute; border-radius: 50%; filter: blur(80px); opacity: 0.06; }
.bg-wave-1 { width: 600px; height: 600px; background: var(--p-primary-color); top: -200px; left: -150px; }
.bg-wave-2 { width: 500px; height: 500px; background: var(--p-primary-400); bottom: -150px; right: -100px; }

.bg-orb { position: absolute; border-radius: 50%; filter: blur(40px); opacity: 0.04; }
.bg-orb-1 { width: 250px; height: 250px; background: #60a5fa; top: 30%; left: 5%; animation: float 8s ease-in-out infinite; }
.bg-orb-2 { width: 180px; height: 180px; background: #a78bfa; bottom: 20%; right: 10%; animation: float 10s ease-in-out infinite reverse; }

@keyframes float {
  0%, 100% { transform: translateY(0px); }
  50%       { transform: translateY(-20px); }
}

.login-container {
  position: relative; z-index: 1;
  width: 100%; max-width: 440px;
  padding: 1.5rem;
  display: flex; flex-direction: column; align-items: center; gap: 2rem;
}

.login-brand { display: flex; align-items: center; gap: 1rem; color: inherit; }

.brand-icon {
  width: 52px; height: 52px; border-radius: 14px;
  background: linear-gradient(135deg, var(--p-primary-400), var(--p-primary-600));
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 8px 24px rgba(var(--primary-color-rgb, 99, 102, 241), 0.35);
}
.brand-icon i { font-size: 1.5rem; color: #fff; }

.brand-text h1 { font-size: 1.6rem; font-weight: 700; margin: 0; line-height: 1; color: var(--p-text-color); letter-spacing: -0.5px; }
.brand-text span { font-size: 0.78rem; color: var(--p-text-muted-color); letter-spacing: 0.5px; text-transform: uppercase; }

/* ── Card ─────────────────────────────────────────────────────────────────── */
.login-card {
  width: 100%;
  background: rgba(255, 255, 255, 0.92);
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: 16px;
  padding: 2.5rem 2rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.25), 0 20px 60px -10px rgba(0, 0, 0, 0.35);
  backdrop-filter: blur(12px);
  transition: box-shadow 0.3s ease;
}
.login-card:hover {
  box-shadow: 0 8px 40px rgba(0, 0, 0, 0.3), 0 25px 70px -10px rgba(0, 0, 0, 0.4);
}
  .card-logo {
    display: flex;
    justify-content: center;
    padding: 0.25rem 0 1.25rem;
  }
/* ── Card header ──────────────────────────────────────────────────────────── */
.card-header {
  margin-bottom: 2rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.card-header h2 { font-size: 1.5rem; font-weight: 700; margin: 0; color: #1e293b; }
.card-header p  { font-size: 0.875rem; color: #64748b; margin: 0; }

.state-icon {
  width: 56px; height: 56px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 1rem;
  font-size: 1.5rem;
}
.state-icon--success { background: rgba(56, 161, 105, 0.15); color: var(--p-green-500, #38a169); }
.state-icon--error   { background: rgba(229, 62, 62, 0.12);  color: var(--p-red-400, #e53e3e); }

/* ── Form ─────────────────────────────────────────────────────────────────── */
.login-form { display: flex; flex-direction: column; gap: 1.25rem; }

.field { display: flex; flex-direction: column; gap: 0.4rem; }
.field label { font-size: 0.85rem; font-weight: 600; color: #1e293b; }

.input-wrapper { position: relative; }
.input-icon {
  position: absolute; left: 0.85rem; top: 50%; transform: translateY(-50%);
  color: var(--p-text-muted-color); font-size: 0.9rem; z-index: 1; pointer-events: none;
}

.input-wrapper :deep(.p-inputtext),
.input-wrapper :deep(.p-password-input) {
  padding-left: 2.5rem !important;
  width: 100%;
  border-radius: 10px;
  background: #f8fafc;
  border-color: #cbd5e1;
  color: #1e293b;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.input-wrapper :deep(.p-inputtext:-webkit-autofill),
.input-wrapper :deep(.p-password-input:-webkit-autofill) {
  -webkit-text-fill-color: #1e293b;
  -webkit-box-shadow: 0 0 0 1000px #f8fafc inset;
}

.input-wrapper :deep(.p-inputtext:focus),
.input-wrapper :deep(.p-password-input:focus) {
  border-color: var(--p-primary-color);
  box-shadow: 0 0 0 3px rgba(var(--primary-color-rgb, 99, 102, 241), 0.15);
}
.input-wrapper.has-error :deep(.p-inputtext),
.input-wrapper.has-error :deep(.p-password-input) { border-color: var(--p-red-400); }
.input-wrapper :deep(.p-password) { width: 100%; }

.error-msg { font-size: 0.78rem; color: var(--p-red-400); }
.login-error { border-radius: 10px; }
.login-success { border-radius: 10px; }

.login-btn {
  width: 100%; padding: 0.75rem; border-radius: 10px;
  font-weight: 600; font-size: 0.95rem; margin-top: 0.5rem;
  justify-content: center;
}

.helper-link {
  display: flex; align-items: center; justify-content: center;
  margin-top: 1rem; font-size: 0.9rem;
  color: var(--p-primary-color); text-decoration: none;
}
.helper-link:hover { text-decoration: underline; }

.login-footer { font-size: 0.75rem; color: var(--p-text-muted-color); opacity: 0.6; margin: 0; text-align: center; }
</style>
