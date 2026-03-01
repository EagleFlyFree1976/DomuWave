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
      <div class="login-brand">
        <div class="brand-icon">
          <i class="pi pi-building"></i>
        </div>
        <div class="brand-text">
          <h1>DomuWave</h1>
          <span>Gestione Condomini</span>
        </div>
      </div>

      <!-- Card -->
      <div class="login-card">
        <div class="card-header">
          <h2>Seleziona il condominio</h2>
          <p>Sei proprietario in più condomini. Scegli quello a cui vuoi accedere.</p>
        </div>

        <div class="tenant-list">
          <button
            v-for="opt in authStore.condominoTenants"
            :key="opt.tenantId"
            class="tenant-item"
            :disabled="selecting"
            @click="select(opt)"
          >
            <span class="tenant-icon"><i class="pi pi-home"></i></span>
            <span class="tenant-name">{{ opt.condominiumName || 'Condominio' }}</span>
            <i class="pi pi-chevron-right tenant-arrow"></i>
          </button>
        </div>
      </div>

      <p class="login-footer">
        © {{ new Date().getFullYear() }} DomuWave — Tutti i diritti riservati
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useMenuStore } from '@/stores/menuStore'

const router    = useRouter()
const authStore = useAuthStore()
const menuStore = useMenuStore()
const selecting = ref(false)

onMounted(() => {
  // Se non c'è nessuna opzione (es. refresh diretto della pagina) → login
  if (!authStore.condominoTenants.length) {
    router.replace('/login')
  }
})

async function select(option) {
  selecting.value = true
  authStore.selectCondominoTenant(option)
  await menuStore.fetchMenu()
  router.push('/dashboard')
}
</script>

<style scoped>
/* ── Layout (identico a LoginView) ─────────────────────────────────────────── */
.login-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-surface-ground);
  position: relative;
  overflow: hidden;
  font-family: 'DM Sans', sans-serif;
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

.login-card {
  width: 100%;
  background: var(--p-surface-card);
  border: 1px solid var(--p-surface-border);
  border-radius: 16px; padding: 2.5rem 2rem;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,.2), 0 20px 60px -10px rgba(0,0,0,.3);
}

.card-header { margin-bottom: 1.75rem; text-align: center; }
.card-header h2 { font-size: 1.5rem; font-weight: 700; margin: 0 0 0.4rem; color: var(--p-text-color); }
.card-header p  { font-size: 0.875rem; color: var(--p-text-muted-color); margin: 0; }

/* ── Tenant list ────────────────────────────────────────────────────────────── */
.tenant-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.tenant-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  width: 100%;
  padding: 1rem 1.25rem;
  border: 1px solid var(--p-surface-border);
  border-radius: 12px;
  background: var(--p-surface-overlay);
  cursor: pointer;
  text-align: left;
  transition: border-color 0.2s, background 0.2s, box-shadow 0.2s;
  color: var(--p-text-color);
  font-family: inherit;
  font-size: 0.95rem;
  font-weight: 600;
}

.tenant-item:hover:not(:disabled) {
  border-color: var(--p-primary-color);
  background: var(--p-primary-50, rgba(99,102,241,0.06));
  box-shadow: 0 0 0 3px rgba(var(--primary-color-rgb, 99, 102, 241), 0.12);
}

.tenant-item:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.tenant-icon {
  width: 38px; height: 38px;
  border-radius: 10px;
  background: linear-gradient(135deg, var(--p-primary-400), var(--p-primary-600));
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.tenant-icon i { font-size: 1rem; color: #fff; }

.tenant-name { flex: 1; }

.tenant-arrow { color: var(--p-text-muted-color); font-size: 0.8rem; }

.login-footer { font-size: 0.75rem; color: var(--p-text-muted-color); opacity: 0.6; margin: 0; text-align: center; }
</style>
