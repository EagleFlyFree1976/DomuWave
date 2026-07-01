<template>
  <!-- Visibile per Condomino, TenantAdmin e SuperAdmin (quando ha un tenant attivo) -->
  <div v-if="session.isCondomino || session.isTenantAdmin || session.hasMixedRoles || (session.isSuperAdmin && session.hasTenantSelected)" class="tenant-selector">

    <!-- Label superiore -->
    <div class="tenant-selector__label">
      <i class="pi pi-home" />
      <span>{{ session.hasMixedRoles ? 'Contesto attivo' : 'Condominio attivo' }}</span>
    </div>

    <!-- Combo di selezione (con gruppi quando l'utente ha ruoli misti) -->
    <Select
      v-model="selectedOption"
      :options="options"
      :option-group-label="grouped ? 'label' : undefined"
      :option-group-children="grouped ? 'items' : undefined"
      option-label="label"
      :placeholder="'Seleziona...'"
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

    <!-- Imposta predefinito: utile quando l'utente ha più contesti -->
    <button
      v-if="canSetDefault"
      class="tenant-set-default"
      :disabled="settingDefault"
      @click="setActiveAsDefault"
    >
      <i class="pi pi-star" />
      <span>Imposta come predefinito</span>
    </button>

    <!-- Separatore visivo prima del profilo utente -->
    <div class="tenant-selector__divider" />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useSessionStore } from '@/stores/sessionStore'
import { useMenuStore } from '@/stores/menuStore'
import { useAppStore } from '@/stores/app'
import { userTenantApi } from '@/services/userService'
import Select from 'primevue/select'

const session   = useSessionStore()
const menuStore = useMenuStore()
const appStore  = useAppStore()

// ── Opzioni normalizzate ────────────────────────────────────────────────────
// Lo stesso utente può avere ruoli misti. Costruiamo:
//   - opzioni "condomino" da session.condominoCondominiums (ognuna col proprio tenant)
//   - opzioni "admin"     dai tenant-admin / appStore.condomini del tenant attivo
const condominoOptions = computed(() =>
  session.condominoCondominiums.map(c => ({
    kind:     'condomino',
    id:       c.condominiumId,
    label:    c.condominiumName,
    tenantId: c.tenantId,
  })))

// Per l'admin le opzioni sono i condomìni del tenant attivo (caricati da appStore).
const adminOptions = computed(() =>
  appStore.condomini.map(c => ({
    kind:  'admin',
    id:    c.id,
    label: c.name,
  })))

// True quando serve il raggruppamento a sezioni (utente con entrambi i ruoli).
const grouped = computed(() => session.hasMixedRoles)

const options = computed(() => {
  if (grouped.value) {
    return [
      { label: 'Amministrazione', items: adminOptions.value },
      { label: 'I miei condomìni', items: condominoOptions.value },
    ].filter(g => g.items.length)
  }
  // Ruolo singolo: lista piatta
  return session.isCondomino ? condominoOptions.value : adminOptions.value
})

// Lista piatta di tutte le opzioni selezionabili (per syncSelection con i gruppi).
const flatOptions = computed(() =>
  grouped.value ? [...adminOptions.value, ...condominoOptions.value] : options.value)

// ── Valore corrente ─────────────────────────────────────────────────────────
const selectedOption = ref(null)

function syncSelection() {
  const match = flatOptions.value.find(o => o.id === appStore.selectedCondominioId)
  selectedOption.value = match
    ?? (session.isCondomino && !grouped.value ? flatOptions.value[0] : null)
    ?? null
}

// ── Nome del condominio attivo (per il badge) ────────────────────────────────
const activeName = computed(() => selectedOption.value?.label ?? null)

// ── Imposta predefinito ──────────────────────────────────────────────────────
const settingDefault = ref(false)

// Tenant del contesto attualmente selezionato.
const activeTenantId = computed(() =>
  selectedOption.value?.tenantId ?? session.activeTenant?.id ?? null)

// Mostrato quando l'utente ha più di un contesto selezionabile.
const canSetDefault = computed(() =>
  !!activeTenantId.value &&
  (session.hasMixedRoles || flatOptions.value.length > 1))

async function setActiveAsDefault() {
  if (!activeTenantId.value) return
  settingDefault.value = true
  try {
    await userTenantApi.setMyDefaultTenant(activeTenantId.value)
    appStore.toast?.('Contesto predefinito aggiornato', 'success')
  } catch {
    appStore.toast?.('Impossibile impostare il predefinito', 'error')
  } finally {
    settingDefault.value = false
  }
}

// ── Cambio selezione ─────────────────────────────────────────────────────────
async function onCondominioChange({ value }) {
  if (!value) return

  if (value.kind === 'condomino') {
    // Contesto condòmino: cambia tenant (X-Tenant-Id) + condominio attivo.
    session.selectTenant({ id: value.tenantId, name: value.label })
    appStore.selectCondominio(value.id)
    // Ruolo per-tenant: ricalcola il profilo nel nuovo tenant.
    await refreshProfileForTenant(value.tenantId)
    await menuStore.fetchMenu()
  } else {
    // Contesto admin: opera nel tenant attivo, cambia solo il condominio.
    appStore.selectCondominio(value.id)
  }
}

// Ricalcola il profilo nel tenant indicato e aggiorna lo store (no-op su errore).
async function refreshProfileForTenant(tenantId) {
  try {
    const { data } = await userTenantApi.getProfileForTenant(tenantId)
    if (data?.profile) session.setProfile(data.profile)
  } catch { /* mantieni il profilo corrente */ }
}

// ── Sincronizzazione reattiva ────────────────────────────────────────────────
watch(flatOptions, () => {
  syncSelection()
  // Se il Condomino (puro) non ha ancora un condominio selezionato, prendi il primo
  if (session.isCondomino && !grouped.value && !appStore.selectedCondominioId && flatOptions.value.length) {
    const first = flatOptions.value[0]
    session.selectTenant({ id: first.tenantId, name: first.label })
    appStore.selectCondominio(first.id)
    selectedOption.value = first
  }
}, { immediate: false })
watch(() => appStore.selectedCondominioId, syncSelection)

// ── Init: carica i condomini se non ancora presenti (admin e superadmin) ─────
onMounted(async () => {
  if ((session.isTenantAdmin || session.isSuperAdmin || session.hasMixedRoles) && !appStore.condomini.length) {
    await appStore.loadCondomini()
  }
  // Condomino puro: auto-seleziona il primo condominio se non già impostato
  if (session.isCondomino && !grouped.value && flatOptions.value.length && !appStore.selectedCondominioId) {
    const first = flatOptions.value[0]
    session.selectTenant({ id: first.tenantId, name: first.label })
    appStore.selectCondominio(first.id)
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

/* ── IMPOSTA PREDEFINITO ─────────────────────────────────────────────────── */
.tenant-set-default {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 2px;
  padding: 4px 6px;
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 11px;
  color: var(--text-dim);
  border-radius: 5px;
  transition: color 0.12s, background 0.12s;
}
.tenant-set-default:hover:not(:disabled) {
  color: var(--accent);
  background: var(--accent)10;
}
.tenant-set-default:disabled { opacity: 0.5; cursor: default; }
.tenant-set-default .pi { font-size: 11px; }

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
