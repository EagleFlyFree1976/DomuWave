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
        <div class="card-divider"></div>
        <div class="card-header">
          <h2>Accedi</h2>
          <p>Inserisci le tue credenziali per accedere alla piattaforma</p>
        </div>

        <form @submit.prevent="handleLogin" class="login-form" novalidate>
          <!-- Username -->
          <div class="field">
            <label for="username">Username</label>
            <div class="input-wrapper" :class="{ 'has-error': v$.username.$error }">
              <i class="pi pi-user input-icon"></i>
              <InputText id="username"
                         v-model="form.username"
                         placeholder="Il tuo username"
                         autocomplete="username"
                         :class="{ 'p-invalid': v$.username.$error }"
                         @blur="v$.username.$touch()" />
            </div>
            <small class="error-msg" v-if="v$.username.$error">
              {{ v$.username.$errors[0].$message }}
            </small>
          </div>

          <!-- Password -->
          <div class="field">
            <label for="password">Password</label>
            <div class="input-wrapper" :class="{ 'has-error': v$.password.$error }">
              <i class="pi pi-lock input-icon"></i>
              <Password id="password"
                        v-model="form.password"
                        placeholder="La tua password"
                        :feedback="false"
                        toggle-mask
                        autocomplete="current-password"
                        :class="{ 'p-invalid': v$.password.$error }"
                        @blur="v$.password.$touch()" />
            </div>
            <small class="error-msg" v-if="v$.password.$error">
              {{ v$.password.$errors[0].$message }}
            </small>
          </div>

          <div class="form-actions">
            <RouterLink :to="{ path: '/reset-password', query: form.username ? { email: form.username } : {} }" class="forgot-link">
              Password dimenticata?
            </RouterLink>
          </div>

          <!-- API Error -->
          <Message v-if="authStore.error" severity="error" :closable="false" class="login-error">
            <i class="pi pi-exclamation-triangle" style="margin-right: 6px"></i>
            {{ authStore.error }}
          </Message>

          <!-- Submit -->
          <Button type="submit"
                  label="Accedi"
                  icon="pi pi-sign-in"
                  icon-pos="right"
                  class="login-btn"
                  :loading="authStore.loading"
                  :disabled="authStore.loading" />

          <p style="text-align:center;font-size:0.85rem;color:#64748b;margin-top:8px">
            Non hai un account?
            <RouterLink to="/register" style="color:var(--p-primary-color);font-weight:600;text-decoration:none">
              Inizia la prova gratuita
            </RouterLink>
          </p>
        </form>
      </div>

      <p class="login-footer">
        © {{ new Date().getFullYear() }} VizaSoft S.r.l. — Tutti i diritti riservati
      </p>
    </div>
  </div>
</template>

<script setup>
  import { reactive } from 'vue'
  import { useRouter, useRoute } from 'vue-router'
  import { useVuelidate } from '@vuelidate/core'
  import { required, helpers, minLength } from '@vuelidate/validators'
  import { useAuthStore } from '@/stores/authStore'
  import { useMenuStore } from '@/stores/menuStore'
  import InputText from 'primevue/inputtext'
  import Password from 'primevue/password'
  import Button from 'primevue/button'
  import Message from 'primevue/message'

  const router = useRouter()
  const route = useRoute()
  const authStore = useAuthStore()
  const menuStore = useMenuStore()

  const form = reactive({
    username: route.query.email ?? '',
    password: '',
  })

  const rules = {
    username: {
      required: helpers.withMessage('Lo username è obbligatorio', required),
    },
    password: {
      required: helpers.withMessage('La password è obbligatoria', required),
      minLength: helpers.withMessage('Minimo 4 caratteri', minLength(4)),
    },
  }

  const v$ = useVuelidate(rules, form)

  async function handleLogin() {
    const valid = await v$.value.$validate()
    if (!valid) return

    const result = await authStore.login(form.username, form.password)
    if (result.success) {
      // Pre-fetch menu for current user
      await menuStore.fetchMenu()
      // Redirect to originally requested URL or dashboard
      const redirect = route.query.redirect ?? '/dashboard'
      router.push(redirect)
    }
  }
</script>

<style scoped>
  /* ── Layout ──────────────────────────────────────────────────────────────── */
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

  /* ── Decorative background ───────────────────────────────────────────────── */
  .login-bg {
    position: absolute;
    inset: 0;
    pointer-events: none;
    z-index: 0;
  }

  .bg-wave {
    position: absolute;
    border-radius: 50%;
    filter: blur(80px);
    opacity: 0.06;
  }

  .bg-wave-1 {
    width: 600px;
    height: 600px;
    background: var(--p-primary-color);
    top: -200px;
    left: -150px;
  }

  .bg-wave-2 {
    width: 500px;
    height: 500px;
    background: var(--p-primary-400);
    bottom: -150px;
    right: -100px;
  }

  .bg-orb {
    position: absolute;
    border-radius: 50%;
    filter: blur(40px);
    opacity: 0.04;
  }

  .bg-orb-1 {
    width: 250px;
    height: 250px;
    background: #60a5fa;
    top: 30%;
    left: 5%;
    animation: float 8s ease-in-out infinite;
  }

  .bg-orb-2 {
    width: 180px;
    height: 180px;
    background: #a78bfa;
    bottom: 20%;
    right: 10%;
    animation: float 10s ease-in-out infinite reverse;
  }

  @keyframes float {
    0%, 100% {
      transform: translateY(0px);
    }

    50% {
      transform: translateY(-20px);
    }
  }

  /* ── Container ───────────────────────────────────────────────────────────── */
  .login-container {
    position: relative;
    z-index: 1;
    width: 100%;
    max-width: 440px;
    padding: 1.5rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2rem;
  }

  .form-actions {
    display: flex;
    justify-content: flex-end;
    margin: 0.2rem 0 0.8rem;
  }

  .forgot-link {
    font-size: 0.9rem;
    color: var(--p-primary-color);
    text-decoration: none;
  }

  .forgot-link:hover {
    text-decoration: underline;
  }

  /* ── Brand ───────────────────────────────────────────────────────────────── */
  .login-brand {
    display: flex;
    align-items: center;
    gap: 1rem;
    text-decoration: none;
    color: inherit;
  }

  .brand-icon {
    width: 52px;
    height: 52px;
    border-radius: 14px;
    background: linear-gradient(135deg, var(--p-primary-400), var(--p-primary-600));
    
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 8px 24px rgba(var(--primary-color-rgb, 99, 102, 241), 0.35);
  }

    .brand-icon i {
      font-size: 1.5rem;
      color: #fff;
    }

  .brand-text h1 {
    font-size: 1.6rem;
    font-weight: 700;
    margin: 0;
    line-height: 1;
    color: var(--p-text-color);
    letter-spacing: -0.5px;
  }

  .brand-text span {
    font-size: 0.78rem;
    color: var(--p-text-muted-color);
    letter-spacing: 0.5px;
    text-transform: uppercase;
  }

  /* ── Card ────────────────────────────────────────────────────────────────── */
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

  /* ── Card logo ───────────────────────────────────────────────────────────── */
  .card-logo {
    display: flex;
    justify-content: center;
    padding: 0.25rem 0 1.25rem;
  }

  .card-logo img {
    max-width: 220px;
    height: auto;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  }

  .card-divider {
    height: 1px;
    background: linear-gradient(to right, transparent, #cbd5e1, transparent);
    margin-bottom: 1.5rem;
  }

  /* ── Card header ─────────────────────────────────────────────────────────── */
  .card-header {
    margin-bottom: 2rem;
    text-align: center;
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
  }

    .card-header h2 {
      font-size: 1.5rem;
      font-weight: 700;
      margin: 0;
      color: #1e293b;
    }

    .card-header p {
      font-size: 0.875rem;
      color: #64748b;
      margin: 0;
    }

  /* ── Form ────────────────────────────────────────────────────────────────── */
  .login-form {
    display: flex;
    flex-direction: column;
    gap: 1.25rem;
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
  }

    .field label {
      font-size: 0.85rem;
      font-weight: 600;
      color: #1e293b;
    }

  .input-wrapper {
    position: relative;
  }

  .input-icon {
    position: absolute;
    left: 0.85rem;
    top: 50%;
    transform: translateY(-50%);
    color: var(--p-text-muted-color);
    font-size: 0.9rem;
    z-index: 1;
    pointer-events: none;
  }

  /* Force icon padding on InputText */
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
  .input-wrapper.has-error :deep(.p-password-input) {
    border-color: var(--p-red-400);
  }

  /* Make Password full-width */
  .input-wrapper :deep(.p-password) {
    width: 100%;
  }

  .error-msg {
    font-size: 0.78rem;
    color: var(--p-red-400);
  }

  /* ── Login error message ─────────────────────────────────────────────────── */
  .login-error {
    border-radius: 10px;
  }

  /* ── Submit button ───────────────────────────────────────────────────────── */
  .login-btn {
    width: 100%;
    padding: 0.75rem;
    border-radius: 10px;
    font-weight: 600;
    font-size: 0.95rem;
    margin-top: 0.5rem;
    justify-content: center;
  }

  /* ── Footer ──────────────────────────────────────────────────────────────── */
  .login-footer {
    font-size: 0.75rem;
    color: var(--p-text-muted-color);
    opacity: 0.6;
    margin: 0;
    text-align: center;
  }
</style>
