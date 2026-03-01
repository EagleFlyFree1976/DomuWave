<template>
  <!-- Visibile solo per gli utenti Condomino -->
  <div v-if="session.isCondomino" class="tenant-selector">

    <!-- Label superiore -->
    <div class="tenant-selector__label">
      <i class="pi pi-home" />
      <span>Il tuo condominio</span>
    </div>

    <!-- Combo di selezione -->
    <Select
      v-model="selectedCondominio"
      :options="session.condominoCondominiums"
      option-label="condominiumName"
      :placeholder="'Seleziona condominio...'"
      class="tenant-selector__select"
      @change="onCondominioChange"
    >
      <!-- Valore selezionato nella combo chiusa -->
      <template #value="{ value }">
        <div v-if="value" class="tenant-selected">
          <i class="pi pi-building tenant-selected__icon" />
          <span class="tenant-selected__name">{{ value.condominiumName }}</span>
        </div>
        <span v-else class="tenant-placeholder">Seleziona condominio...</span>
      </template>
    </Select>

    <!-- Badge condominio attivo -->
    <Transition name="fade">
      <div v-if="session.activeTenant" class="tenant-active-badge">
        <span class="tenant-active-badge__dot" />
        <span class="tenant-active-badge__text">Accesso come: <strong>{{ session.activeTenant.name }}</strong></span>
      </div>
    </Transition>

    <!-- Separatore visivo prima del profilo utente -->
    <div class="tenant-selector__divider" />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useSessionStore } from '@/stores/sessionStore'
import { useMenuStore } from '@/stores/menuStore'
import Select from 'primevue/select'

const session  = useSessionStore()
const menuStore = useMenuStore()

// Trova l'opzione corrispondente al tenant attivo corrente
// CondominiumSummaryDto: { condominiumId, condominiumName, tenantId (Guid as string) }
const selectedCondominio = ref(
  session.condominoCondominiums.find(t => t.tenantId === session.activeTenant?.id) ?? null
)

// Mantiene il v-model allineato se lo store cambia (es. dopo refresh)
watch(
  () => session.activeTenant,
  (val) => {
    selectedCondominio.value = session.condominoCondominiums.find(t => t.tenantId === val?.id) ?? null
  }
)

async function onCondominioChange({ value }) {
  if (!value) return
  session.selectTenant({ id: value.tenantId, name: value.condominiumName })
  await menuStore.fetchMenu()
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
