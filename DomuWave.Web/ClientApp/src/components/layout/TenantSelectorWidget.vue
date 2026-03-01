<template>
  <!-- Visibile solo per il superamministratore -->
  <div v-if="session.isSuperAdmin" class="tenant-selector">

    <!-- Label superiore -->
    <div class="tenant-selector__label">
      <i class="pi pi-globe" />
      <span>Contesto tenant</span>
    </div>

    <!-- Combo di selezione -->
    <Select
      v-model="selectedTenant"
      :options="session.availableTenants"
      option-label="name"
      :loading="session.loadingTenants"
      :placeholder="session.loadingTenants ? 'Caricamento...' : 'Seleziona tenant...'"
      class="tenant-selector__select"
      filter
      filter-placeholder="Cerca tenant..."
      empty-filter-message="Nessun tenant trovato"
      empty-message="Nessun tenant disponibile"
      @change="onTenantChange"
    >
      <!-- Opzione singola nella dropdown -->
      <template #option="{ option }">
        <div class="tenant-option">
          <span class="tenant-option__code">{{ option.code }}</span>
          <span class="tenant-option__name">{{ option.name }}</span>
        </div>
      </template>

      <!-- Valore selezionato nella combo chiusa -->
      <template #value="{ value }">
        <div v-if="value" class="tenant-selected">
          <i class="pi pi-building tenant-selected__icon" />
          <span class="tenant-selected__name">{{ value.name }}</span>
        </div>
        <span v-else class="tenant-placeholder">
          {{ session.loadingTenants ? 'Caricamento...' : 'Seleziona tenant...' }}
        </span>
      </template>
    </Select>

    <!-- Badge tenant attivo (mostrato anche quando la combo è chiusa) -->
    <Transition name="fade">
      <div v-if="session.activeTenant && !session.loadingTenants" class="tenant-active-badge">
        <span class="tenant-active-badge__dot" />
        <span class="tenant-active-badge__text">Accesso come: <strong>{{ session.activeTenant.name }}</strong></span>
      </div>
    </Transition>

    <!-- Avviso se nessun tenant selezionato -->
    <Transition name="fade">
      <div v-if="!session.activeTenant && !session.loadingTenants" class="tenant-warning">
        <i class="pi pi-exclamation-triangle" />
        <span>Seleziona un tenant per operare</span>
      </div>
    </Transition>

    <!-- Separatore visivo prima del profilo utente -->
    <div class="tenant-selector__divider" />
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useSessionStore } from '@/stores/sessionStore'
import Select from 'primevue/select'

// ─── COMPOSABLES ─────────────────────────────────────────────────────────────
const session = useSessionStore()

// ─── LOCAL STATE ─────────────────────────────────────────────────────────────
// Copia locale per il v-model della Select; sincronizzata con lo store
const selectedTenant = ref(session.activeTenant)

// Mantiene il v-model locale allineato se lo store cambia esternamente
// (es. reset al logout, invalidazione dal server)
watch(
  () => session.activeTenant,
  (val) => { selectedTenant.value = val }
)

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
onMounted(async () => {
  // Carica i tenant disponibili (no-op se non superadmin)
  console.log("tenant selector call loadtenants");
  await session.loadTenants()
})

// ─── METHODS ─────────────────────────────────────────────────────────────────

/**
 * Gestisce la selezione di un tenant dalla combo.
 * 1. Persiste in sessionStore (→ localStorage → X-Tenant-Id header)
 * 2. Naviga alla dashboard principale del tenant selezionato
 */
function onTenantChange({ value }) {
  if (!value) return
  session.selectTenant(value)
}
</script>

<style scoped>
/* ── CONTAINER ───────────────────────────────────────────────────────────── */
.tenant-selector {
  padding: 12px 12px 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

/* ── LABEL ───────────────────────────────────────────────────────────────── */
.tenant-selector__label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 1.2px;
  color: var(--text-faint);
  padding: 0 4px;
}
.tenant-selector__label .pi {
  font-size: 11px;
  color: var(--accent);
}

/* ── SELECT ──────────────────────────────────────────────────────────────── */
.tenant-selector__select {
  width: 100%;
  font-size: 13px;
}

/* ── OPTION NELLA DROPDOWN ───────────────────────────────────────────────── */
.tenant-option {
  display: flex;
  align-items: baseline;
  gap: 8px;
}
.tenant-option__code {
  font-family: 'JetBrains Mono', monospace;
  font-size: 10px;
  color: var(--accent);
  background: var(--accent)15;
  border: 1px solid var(--accent)25;
  border-radius: 3px;
  padding: 1px 5px;
  flex-shrink: 0;
}
.tenant-option__name {
  font-size: 13px;
  color: var(--text);
}

/* ── VALORE SELEZIONATO ──────────────────────────────────────────────────── */
.tenant-selected {
  display: flex;
  align-items: center;
  gap: 7px;
}
.tenant-selected__icon {
  color: var(--accent);
  font-size: 13px;
}
.tenant-selected__name {
  font-size: 13px;
  color: var(--text);
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.tenant-placeholder {
  color: var(--text-faint);
  font-size: 13px;
}

/* ── BADGE ATTIVO ────────────────────────────────────────────────────────── */
.tenant-active-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 6px;
  border-radius: 5px;
  background: var(--accent)10;
  border: 1px solid var(--accent)20;
}
.tenant-active-badge__dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--accent);
  flex-shrink: 0;
  animation: pulse 2s infinite;
}
.tenant-active-badge__text {
  font-size: 11px;
  color: var(--text-dim);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.tenant-active-badge__text strong {
  color: var(--accent);
}

/* ── WARNING ─────────────────────────────────────────────────────────────── */
.tenant-warning {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 5px 8px;
  border-radius: 5px;
  background: rgba(255, 170, 0, 0.08);
  border: 1px solid rgba(255, 170, 0, 0.2);
  font-size: 11px;
  color: #ffaa00;
}
.tenant-warning .pi {
  font-size: 12px;
  flex-shrink: 0;
}

/* ── DIVIDER ─────────────────────────────────────────────────────────────── */
.tenant-selector__divider {
  margin-top: 6px;
  height: 1px;
  background: var(--border);
}

/* ── ANIMAZIONI ──────────────────────────────────────────────────────────── */
@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50%       { opacity: 0.5; transform: scale(0.85); }
}

.fade-enter-active,
.fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.fade-enter-from,
.fade-leave-to     { opacity: 0; transform: translateY(-4px); }
</style>
