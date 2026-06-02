<template>
  <div>
    <div class="page-header">
      <div>
        <h1>Impostazioni visualizzazione</h1>
        <p class="text-muted" style="font-size:0.85rem;margin-top:2px">
          Come mostrare i valori contabili (spese, incassi, saldi) nelle pagine del condominio
        </p>
      </div>
    </div>

    <div v-if="loading" class="loading-state">Caricamento…</div>

    <div v-else class="display-layout">

      <!-- Colonna principale: scelta convenzione -->
      <div class="display-main">
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-sort-alt"></i>
            <span>Convenzione segno valori contabili</span>
          </div>

          <label class="option-row" :class="{ 'option-selected': form.accountingSignConvention === SIGN_CONVENTION.SOLO_COLORE }">
            <input type="radio" :value="SIGN_CONVENTION.SOLO_COLORE" v-model.number="form.accountingSignConvention" />
            <span class="option-body">
              <span class="option-title">Solo colore</span>
              <span class="option-desc">Le uscite sono mostrate in rosso senza segno meno; il colore indica la natura della voce.</span>
            </span>
          </label>

          <label class="option-row" :class="{ 'option-selected': form.accountingSignConvention === SIGN_CONVENTION.SEGNO_ESPLICITO }">
            <input type="radio" :value="SIGN_CONVENTION.SEGNO_ESPLICITO" v-model.number="form.accountingSignConvention" />
            <span class="option-body">
              <span class="option-title">Segno esplicito</span>
              <span class="option-desc">Le uscite hanno il segno meno (−), le entrate il segno più (+). Coerente con l'aritmetica del saldo.</span>
            </span>
          </label>

          <button class="btn btn-primary btn-block" style="margin-top:18px" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            <span v-else><i class="pi pi-save"></i></span>
            Salva impostazioni
          </button>
        </div>
      </div>

      <!-- Colonna laterale: anteprima -->
      <div class="display-sidebar">
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-eye"></i>
            <span>Anteprima</span>
          </div>
          <p class="settings-hint">Esempio di come appariranno i box dell'esercizio fiscale.</p>
          <div class="preview-grid">
            <div class="preview-box">
              <div class="preview-label">Spese totali</div>
              <div class="preview-value text-red">{{ previewFmt(26972.23, 'expense') }}</div>
            </div>
            <div class="preview-box">
              <div class="preview-label">Incassi ricevuti</div>
              <div class="preview-value text-green">{{ previewFmt(1500, 'income') }}</div>
            </div>
            <div class="preview-box">
              <div class="preview-label">Saldo</div>
              <div class="preview-value" :class="-28674.42 >= 0 ? 'text-green' : 'text-red'">
                {{ previewFmt(-28674.42, 'balance') }}
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { tenantDisplaySettingsApi } from '@/services/api'
import { useAccountingFormat, SIGN_CONVENTION } from '@/composables/useAccountingFormat'

const store   = useAppStore()
const loading = ref(false)
const saving  = ref(false)

const { load: reloadAccountingFormat, fmtAccounting } = useAccountingFormat()

const form = ref({ accountingSignConvention: SIGN_CONVENTION.SOLO_COLORE })

// Anteprima locale: usa il valore selezionato nel form, non quello globale salvato.
const nf = new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
function previewFmt(value, kind) {
  const n = Number(value)
  if (form.value.accountingSignConvention === SIGN_CONVENTION.SEGNO_ESPLICITO) {
    if (kind === 'expense') return n === 0 ? '€ 0,00' : `€ -${nf.format(Math.abs(n))}`
    if (kind === 'income')  return n === 0 ? '€ 0,00' : `€ +${nf.format(Math.abs(n))}`
    return '€ ' + nf.format(n)
  }
  if (kind === 'expense' || kind === 'income') return '€ ' + nf.format(Math.abs(n))
  return '€ ' + nf.format(n)
}

async function load() {
  loading.value = true
  try {
    const { data } = await tenantDisplaySettingsApi.get()
    form.value.accountingSignConvention = data?.accountingSignConvention ?? SIGN_CONVENTION.SOLO_COLORE
  } catch {
    // default già impostato
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    await tenantDisplaySettingsApi.update({ accountingSignConvention: form.value.accountingSignConvention })
    store.toast('Impostazioni salvate', 'success')
    // Ricarica il singleton globale così le altre pagine usano subito la nuova convenzione
    await reloadAccountingFormat(true)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 12px; }

.display-layout {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 16px;
  align-items: start;
}
@media (max-width: 860px) {
  .display-layout { grid-template-columns: 1fr; }
}

.display-main, .display-sidebar { display: flex; flex-direction: column; gap: 16px; }

.settings-card {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md, 8px);
  padding: 18px 20px;
}
.settings-card-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 0.8rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.5px; color: var(--text-muted);
  margin-bottom: 16px; padding-bottom: 10px;
  border-bottom: 1px solid var(--border);
}
.settings-card-header .pi { font-size: 0.85rem; }

.option-row {
  display: flex; align-items: flex-start; gap: 12px;
  padding: 14px; margin-bottom: 10px;
  border: 1px solid var(--border); border-radius: 8px;
  cursor: pointer; transition: border-color 0.15s, background 0.15s;
}
.option-row:hover { border-color: var(--border-active, var(--accent)); }
.option-selected  { border-color: var(--accent); background: var(--accent-glow, rgba(99,102,241,0.06)); }
.option-row input[type="radio"] { margin-top: 3px; flex-shrink: 0; accent-color: var(--accent); }
.option-body  { display: flex; flex-direction: column; gap: 3px; }
.option-title { font-weight: 600; font-size: 0.9rem; }
.option-desc  { font-size: 0.82rem; color: var(--text-muted); line-height: 1.45; }

.btn-block { width: 100%; justify-content: center; gap: 6px; }

.settings-hint { font-size: 0.82rem; color: var(--text-muted); margin-bottom: 12px; line-height: 1.5; }

.preview-grid { display: flex; flex-direction: column; gap: 10px; }
.preview-box {
  border: 1px solid var(--border); border-radius: 8px; padding: 10px 12px;
  background: var(--bg-base, transparent);
}
.preview-label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.4px; color: var(--text-muted); }
.preview-value { font-size: 1.05rem; font-weight: 700; font-family: monospace; margin-top: 2px; }

.text-red   { color: var(--accent-red, #ef4444); }
.text-green { color: var(--accent-green, #22c55e); }
</style>
