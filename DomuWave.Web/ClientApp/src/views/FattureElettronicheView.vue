<template>
  <div class="einvoice-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-file-import page-title__icon" />
          Fatture Elettroniche
        </h1>
        <span class="page-subtitle">
          Download massivo delle fatture passive dal Cassetto Fiscale (Sistema di Interscambio)
        </span>
      </div>
    </div>

    <!-- ── CONFIGURAZIONE ─────────────────────────────────────────────────── -->
    <div class="config-card">
      <div class="config-card__head">
        <span class="config-card__title"><i class="pi pi-cog" /> Configurazione provider</span>
        <span v-if="config.lastSyncDate" class="config-card__sync">
          Ultimo download: {{ formatDateTime(config.lastSyncDate) }}
        </span>
      </div>

      <div class="config-grid">
        <div class="filter-field">
          <label class="filter-field__label">Provider SdI</label>
          <Select v-model="cfgForm.providerId" :options="providers"
                  optionLabel="label" optionValue="id"
                  placeholder="Seleziona provider…" class="config-input" showClear />
        </div>
        <div class="filter-field">
          <label class="filter-field__label">
            Partita IVA ricezione
            <span class="field-hint-inline">override opzionale</span>
          </label>
          <InputText v-model="cfgForm.vatNumberOverride"
                     :placeholder="config.condominiumVatNumber
                        ? `Anagrafica: ${config.condominiumVatNumber}`
                        : 'P.IVA del condominio non impostata'"
                     class="config-input"
                     autocomplete="off" inputmode="numeric" name="einvoice-vat-override" />
        </div>
        <div class="filter-field">
          <label class="filter-field__label">
            Chiave API
            <span v-if="config.hasApiKey" class="key-set"><i class="pi pi-check-circle" /> impostata</span>
          </label>
          <Password v-model="cfgForm.apiKey" :feedback="false" toggleMask
                    :placeholder="config.hasApiKey ? '•••••••• (lascia vuoto per non cambiare)' : 'Incolla la chiave API'"
                    class="config-input" inputClass="config-input"
                    :inputProps="{ autocomplete: 'new-password', name: 'einvoice-api-key' }" />
        </div>
      </div>

      <div class="config-actions">
        <Button icon="pi pi-save" label="Salva configurazione"
                class="btn-primary" :loading="savingConfig" @click="saveConfig" />
      </div>
    </div>

    <!-- ── FILTER BAR (download) ──────────────────────────────────────────── -->
    <div class="filter-bar">
      <div class="filter-field">
        <label class="filter-field__label">Dal</label>
        <DatePicker v-model="from" dateFormat="dd/mm/yy" showIcon iconDisplay="input"
                    class="filter-date" />
      </div>
      <div class="filter-field">
        <label class="filter-field__label">Al</label>
        <DatePicker v-model="to" dateFormat="dd/mm/yy" showIcon iconDisplay="input"
                    class="filter-date" />
      </div>
      <Button icon="pi pi-download"
              label="Scarica fatture"
              class="btn-primary"
              :loading="syncing"
              :disabled="syncing || !condominiumId || !isConfigured"
              v-tooltip="!isConfigured ? 'Configura prima il provider' : ''"
              @click="sync" />
      <Button icon="pi pi-refresh" class="btn-ghost" v-tooltip="'Ricarica'"
              :loading="loading" @click="loadData" />
    </div>

    <!-- ── TABLE ──────────────────────────────────────────────────────────── -->
    <div class="table-wrapper">
      <DataTable :value="items" :loading="loading" data-key="id" class="domu-table"
                 paginator :rows="15" :rowsPerPageOptions="[15, 30, 50]">
        <Column field="invoiceNumber" header="Numero" style="width: 140px">
          <template #body="{ data }">
            <span class="code-badge">{{ data.invoiceNumber }}</span>
          </template>
        </Column>
        <Column field="invoiceDate" header="Data" style="width: 120px">
          <template #body="{ data }">{{ formatDate(data.invoiceDate) }}</template>
        </Column>
        <Column field="supplierName" header="Fornitore">
          <template #body="{ data }">{{ data.supplierName || '—' }}</template>
        </Column>
        <Column field="supplierVat" header="P.IVA" style="width: 140px">
          <template #body="{ data }">
            <span class="mono">{{ data.supplierVat || '—' }}</span>
          </template>
        </Column>
        <Column field="totalAmount" header="Importo" style="width: 130px; text-align: right">
          <template #body="{ data }">
            <span class="amount">{{ formatMoney(data.totalAmount) }}</span>
          </template>
        </Column>
        <Column field="statusName" header="Stato" style="width: 120px">
          <template #body="{ data }">
            <span class="status-badge" :class="statusClass(data.statusId)">{{ data.statusName }}</span>
          </template>
        </Column>

        <template #empty>
          <div class="empty-state">
            <i class="pi pi-file-import empty-state__icon" />
            <span>Nessuna fattura scaricata. Imposta un intervallo e premi “Scarica fatture”.</span>
          </div>
        </template>
        <template #loadingicon>
          <i class="pi pi-spinner pi-spin loading-spinner" />
        </template>
      </DataTable>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { eInvoiceApi } from '@/services/api'

import DataTable  from 'primevue/datatable'
import Column     from 'primevue/column'
import Button     from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import Select     from 'primevue/select'
import InputText  from 'primevue/inputtext'
import Password   from 'primevue/password'

const store = useAppStore()

// ── Provider disponibili (allineati a EInvoiceProviderLookup) ───────────────
const providers = [
  { id: 1, label: 'Acube' },
  { id: 2, label: 'Aruba' },
  { id: 3, label: 'Fatture in Cloud' },
]

// ── State ──────────────────────────────────────────────────────────────────
const loading      = ref(false)
const syncing      = ref(false)
const savingConfig = ref(false)
const items        = ref([])

const config  = reactive({
  providerId: null,
  vatNumberOverride: '',
  condominiumVatNumber: '',
  effectiveVatNumber: '',
  hasApiKey: false,
  lastSyncDate: null,
})
const cfgForm = reactive({ providerId: null, vatNumberOverride: '', apiKey: '' })

// Default: ultimi 90 giorni
const today = new Date()
const from  = ref(new Date(today.getTime() - 90 * 24 * 60 * 60 * 1000))
const to    = ref(today)

// ── Computed ───────────────────────────────────────────────────────────────
const condominiumId = computed(() => store.selectedCondominioId)
const isConfigured  = computed(() => !!config.providerId && !!config.effectiveVatNumber && config.hasApiKey)

const formatDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const formatDateTime = (d) => d ? new Date(d).toLocaleString('it-IT') : '—'
const formatMoney = (n) =>
  (n ?? 0).toLocaleString('it-IT', { style: 'currency', currency: 'EUR' })

const statusClass = (statusId) => ({
  0: 'status-badge--new',
  1: 'status-badge--linked',
  2: 'status-badge--muted',
}[statusId] ?? 'status-badge--muted')

const toIso = (d) => (d instanceof Date ? d : new Date(d)).toISOString().slice(0, 10)

// ── Config ─────────────────────────────────────────────────────────────────
async function loadConfig() {
  if (!condominiumId.value) return
  try {
    const { data } = await eInvoiceApi.getConfig(condominiumId.value)
    config.providerId           = data?.providerId ?? null
    config.vatNumberOverride    = data?.vatNumberOverride ?? ''
    config.condominiumVatNumber = data?.condominiumVatNumber ?? ''
    config.effectiveVatNumber   = data?.effectiveVatNumber ?? ''
    config.hasApiKey            = data?.hasApiKey ?? false
    config.lastSyncDate         = data?.lastSyncDate ?? null
    // popola il form (chiave sempre vuota: non viene mai restituita)
    cfgForm.providerId        = config.providerId
    cfgForm.vatNumberOverride = config.vatNumberOverride
    cfgForm.apiKey            = ''
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function saveConfig() {
  if (!condominiumId.value) {
    store.toast('Seleziona prima un condominio', 'error')
    return
  }
  savingConfig.value = true
  try {
    await eInvoiceApi.updateConfig(condominiumId.value, {
      providerId:        cfgForm.providerId,
      vatNumberOverride: cfgForm.vatNumberOverride || null,  // vuoto = usa P.IVA anagrafica
      apiKey:            cfgForm.apiKey || null,              // vuoto = non cambiare
    })
    store.toast('Configurazione salvata', 'success')
    await loadConfig()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    savingConfig.value = false
  }
}

// ── Data loading ───────────────────────────────────────────────────────────
async function loadData() {
  if (!condominiumId.value) return
  loading.value = true
  try {
    const { data } = await eInvoiceApi.getByCondominium(condominiumId.value)
    items.value = data ?? []
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    loading.value = false
  }
}

// ── Sync (download massivo) ─────────────────────────────────────────────────
async function sync() {
  if (!condominiumId.value) {
    store.toast('Seleziona prima un condominio', 'error')
    return
  }
  if (to.value < from.value) {
    store.toast('L\'intervallo di date non è valido', 'error')
    return
  }
  syncing.value = true
  try {
    const { data } = await eInvoiceApi.sync(condominiumId.value, toIso(from.value), toIso(to.value))
    const count = data?.length ?? 0
    store.toast(
      count > 0 ? `${count} nuove fatture scaricate` : 'Nessuna nuova fattura nel periodo',
      count > 0 ? 'success' : 'info'
    )
    await Promise.all([loadData(), loadConfig()])
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    syncing.value = false
  }
}

// ── Lifecycle ──────────────────────────────────────────────────────────────
async function bootstrap() {
  await Promise.all([loadConfig(), loadData()])
}
onMounted(bootstrap)
watch(condominiumId, bootstrap)
</script>

<style scoped>
/* ── PAGE ─────────────────────────────────────────────────────────────────── */
.einvoice-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  min-height: 100%;
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header { display: flex; align-items: flex-start; }
.page-header__left { display: flex; flex-direction: column; gap: 4px; }
.page-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}
.page-title__icon { color: var(--accent); font-size: 20px; }
.page-subtitle { font-size: 13px; color: var(--text-dim); }

/* ── CONFIG CARD ─────────────────────────────────────────────────────────── */
.config-card {
  display: flex;
  flex-direction: column;
  gap: 14px;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
}
.config-card__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.config-card__title {
  display: flex;
  align-items: center;
  gap: 7px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.config-card__title .pi { color: var(--accent); }
.config-card__sync { font-size: 11px; color: var(--text-faint); }
.config-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 14px;
}
.config-input { width: 100%; }
.field-hint-inline {
  margin-left: 6px;
  font-weight: 400;
  text-transform: none;
  letter-spacing: 0;
  color: var(--text-faint);
}
.key-set { color: var(--accent); font-weight: 600; margin-left: 6px; }
.key-set .pi { font-size: 11px; }
.config-actions { display: flex; justify-content: flex-end; }

/* ── FILTER BAR ──────────────────────────────────────────────────────────── */
.filter-bar {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 12px;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 14px;
}
.filter-field { display: flex; flex-direction: column; gap: 5px; }
.filter-field__label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.filter-date { width: 170px; }

/* ── BUTTONS ─────────────────────────────────────────────────────────────── */
.btn-primary {
  background: var(--accent) !important;
  border-color: var(--accent) !important;
  color: #000 !important;
  font-weight: 600 !important;
  font-size: 13px !important;
}
.btn-ghost {
  background: transparent !important;
  border-color: var(--border) !important;
  color: var(--text-dim) !important;
}

/* ── TABLE ───────────────────────────────────────────────────────────────── */
.table-wrapper {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.domu-table { font-size: 13px; }
.code-badge {
  display: inline-block;
  padding: 2px 8px;
  background: rgba(52,211,153,0.08);
  border: 1px solid rgba(52,211,153,0.25);
  border-radius: 5px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  color: var(--accent);
}
.mono { font-family: 'JetBrains Mono', monospace; font-size: 12px; }
.amount { font-variant-numeric: tabular-nums; white-space: nowrap; }

/* Status badge */
.status-badge {
  display: inline-block;
  padding: 2px 9px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 600;
  border: 1px solid transparent;
}
.status-badge--new {
  background: rgba(148,163,184,0.10);
  border-color: rgba(148,163,184,0.30);
  color: var(--text-dim);
}
.status-badge--linked {
  background: rgba(52,211,153,0.10);
  border-color: rgba(52,211,153,0.30);
  color: var(--accent);
}
.status-badge--muted {
  background: rgba(148,163,184,0.08);
  border-color: rgba(148,163,184,0.20);
  color: var(--text-faint);
}

.loading-spinner { font-size: 28px; color: var(--accent); }
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 48px 0;
  color: var(--text-faint);
}
.empty-state__icon { font-size: 36px; opacity: 0.4; }

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .einvoice-page { padding: 16px; }
  .filter-bar { flex-direction: column; align-items: stretch; }
  .filter-date { width: 100%; }
}
</style>
