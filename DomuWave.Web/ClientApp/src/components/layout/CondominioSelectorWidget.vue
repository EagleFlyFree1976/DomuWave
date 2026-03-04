<template>
  <!-- Visibile per Condomino, TenantAdmin e SuperAdmin (quando ha un tenant attivo) -->
  <div v-if="session.isCondomino || session.isTenantAdmin || (session.isSuperAdmin && session.hasTenantSelected)" class="tenant-selector">

    <!-- Label superiore -->
    <div class="tenant-selector__label">
      <i class="pi pi-home" />
      <span>Condominio attivo</span>
    </div>

    <!-- Combo di selezione -->
    <Select
      v-model="selectedOption"
      :options="options"
      option-label="label"
      :placeholder="'Seleziona condominio...'"
      class="tenant-selector__select"
      @change="onCondominioChange"
    >
      <!-- Valore selezionato nella combo chiusa -->
      <template #value="{ value }">
        <div v-if="value" class="tenant-selected">
          <i class="pi pi-building tenant-selected__icon" />
          <span class="tenant-selected__name">{{ value.label }}</span>
        </div>
        <span v-else class="tenant-placeholder">Seleziona condominio...</span>
      </template>
    </Select>

    <!-- Badge condominio attivo -->
    <Transition name="fade">
      <div v-if="activeName" class="tenant-active-badge">
        <span class="tenant-active-badge__dot" />
        <span class="tenant-active-badge__text">Attivo: <strong>{{ activeName }}</strong></span>
      </div>
    </Transition>

    <!-- Separatore visivo prima del profilo utente -->
    <div class="tenant-selector__divider" />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useSessionStore } from '@/stores/sessionStore'
import { useMenuStore } from '@/stores/menuStore'
import { useAppStore } from '@/stores/app'
import Select from 'primevue/select'

const session   = useSessionStore()
const menuStore = useMenuStore()
const appStore  = useAppStore()

// ── Opzioni normalizzate ────────────────────────────────────────────────────
// Condomino: session.condominoCondominiums → { condominiumId, condominiumName, tenantId }
// Admin:     appStore.condomini            → { id, name, ... }
const options = computed(() => {
  if (session.isCondomino) {
    return session.condominoCondominiums.map(c => ({
      id:       c.condominiumId,
      label:    c.condominiumName,
      tenantId: c.tenantId,
    }))
  }
  // TenantAdmin
  return appStore.condomini.map(c => ({
    id:    c.id,
    label: c.name,
  }))
})

// ── Valore corrente ─────────────────────────────────────────────────────────
const selectedOption = ref(null)

function syncSelection() {
  if (session.isCondomino) {
    selectedOption.value = options.value.find(o => o.tenantId === session.activeTenant?.id) ?? null
  } else {
    selectedOption.value = options.value.find(o => o.id === appStore.selectedCondominioId) ?? null
  }
}

// ── Nome del condominio attivo (per il badge) ────────────────────────────────
const activeName = computed(() => {
  if (session.isCondomino) return session.activeTenant?.name ?? null
  return appStore.selectedCondominio?.name ?? null
})

// ── Cambio selezione ─────────────────────────────────────────────────────────
async function onCondominioChange({ value }) {
  if (!value) return
  if (session.isCondomino) {
    // Cambia tenant (X-Tenant-Id) + aggiorna menu
    session.selectTenant({ id: value.tenantId, name: value.label })
    await menuStore.fetchMenu()
    // Resetta e ricarica i condomini del nuovo tenant così tutte le
    // view che watchano selectedCondominioId si aggiornano automaticamente
    appStore.selectCondominio(null)
    await appStore.loadCondomini()
  } else {
    // Admin: imposta il condominio selezionato → tutti i watcher nelle view scattano
    appStore.selectCondominio(value.id)
  }
}

// ── Sincronizzazione reattiva ────────────────────────────────────────────────
watch(options, syncSelection, { immediate: false })
watch(() => session.activeTenant, syncSelection)
watch(() => appStore.selectedCondominioId, syncSelection)

// ── Init: carica i condomini se non ancora presenti (admin e superadmin) ─────
onMounted(async () => {
  if ((session.isTenantAdmin || session.isSuperAdmin) && !appStore.condomini.length) {
    await appStore.loadCondomini()
  }
  syncSelection()
})
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
