<template>
  <div>
    <div class="page-header">
      <div>
        <h1>Impostazioni</h1>
        <p class="text-muted" style="font-size:0.85rem;margin-top:2px">
          Configurazione dello studio: aspetto e visualizzazione dei dati
        </p>
      </div>
    </div>

    <div v-if="loading" class="loading-state">Caricamento…</div>

    <div v-else class="settings-shell">
      <!-- ── Navigazione sezioni ── -->
      <nav class="settings-nav">
        <button
          v-for="s in sections" :key="s.key"
          class="settings-nav-item"
          :class="{ active: activeSection === s.key }"
          @click="activeSection = s.key">
          <i class="pi" :class="s.icon"></i>
          <span>{{ s.label }}</span>
        </button>
      </nav>

      <!-- ── Pannello sezione ── -->
      <div class="settings-panel">

        <!-- ===== Aspetto / Branding ===== -->
        <div v-show="activeSection === 'branding'" class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-image"></i>
            <span>Logo dello studio</span>
          </div>
          <p class="settings-hint">
            Il logo appare nella barra laterale dell'applicazione. Formati ammessi: PNG, JPG, WebP, SVG · max 512 KB.
          </p>

          <div class="logo-row">
            <div class="logo-preview">
              <img :src="logoUrl || DEFAULT_LOGO" alt="Logo studio" />
            </div>
            <div class="logo-actions">
              <input
                ref="fileInput" type="file" class="hidden-file"
                accept="image/png,image/jpeg,image/webp,image/svg+xml"
                @change="onFileChange" />
              <button class="btn btn-primary" :disabled="uploading" @click="fileInput?.click()">
                <span v-if="uploading" class="spinner" style="width:14px;height:14px"></span>
                <span v-else><i class="pi pi-upload"></i></span>
                {{ logoUrl ? 'Sostituisci logo' : 'Carica logo' }}
              </button>
              <button class="btn btn-ghost" :disabled="uploading || !hasLogo" @click="removeLogo">
                <i class="pi pi-trash"></i> Rimuovi logo
              </button>
              <p v-if="!hasLogo" class="settings-hint" style="margin:0">
                Nessun logo personalizzato: viene mostrato il logo predefinito.
              </p>
            </div>
          </div>
        </div>

        <!-- ===== Contabilità ===== -->
        <div v-show="activeSection === 'accounting'" class="settings-card">
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

          <!-- Anteprima -->
          <div class="preview-grid" style="margin-top:16px">
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

          <button class="btn btn-primary btn-block" style="margin-top:18px" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            <span v-else><i class="pi pi-save"></i></span>
            Salva impostazioni
          </button>
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
import { useTenantBranding } from '@/composables/useTenantBranding'

const store   = useAppStore()
const loading = ref(false)
const saving  = ref(false)
const uploading = ref(false)

const { load: reloadAccountingFormat } = useAccountingFormat()
const { logoUrl, reload: reloadBranding, DEFAULT_LOGO } = useTenantBranding()

const sections = [
  { key: 'branding',   label: 'Aspetto',     icon: 'pi-image' },
  { key: 'accounting', label: 'Contabilità', icon: 'pi-sort-alt' },
]
const activeSection = ref('branding')

const form    = ref({ accountingSignConvention: SIGN_CONVENTION.SOLO_COLORE })
const hasLogo = ref(false)
const fileInput = ref(null)

const MAX_LOGO_BYTES = 512 * 1024
const ALLOWED_TYPES = ['image/png', 'image/jpeg', 'image/webp', 'image/svg+xml']

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
    hasLogo.value = !!data?.hasLogo
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
    await reloadAccountingFormat(true)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

// ── Branding: upload / rimozione logo ──────────────────────────────────────
function fileToBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload  = () => resolve(String(reader.result).split(',')[1] ?? '')
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

async function onFileChange(e) {
  const file = e.target.files?.[0]
  e.target.value = '' // consente di ricaricare lo stesso file
  if (!file) return

  if (!ALLOWED_TYPES.includes(file.type)) {
    store.toast('Formato non supportato. Usa PNG, JPG, WebP o SVG.', 'error')
    return
  }
  if (file.size > MAX_LOGO_BYTES) {
    store.toast('Il logo non può superare 512 KB.', 'error')
    return
  }

  uploading.value = true
  try {
    const base64 = await fileToBase64(file)
    await tenantDisplaySettingsApi.uploadLogo({
      fileName: file.name,
      contentType: file.type,
      base64Data: base64,
    })
    hasLogo.value = true
    await reloadBranding()
    store.toast('Logo aggiornato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
    else store.toast('Caricamento del logo non riuscito', 'error')
  } finally {
    uploading.value = false
  }
}

async function removeLogo() {
  if (!confirm('Rimuovere il logo dello studio?')) return
  uploading.value = true
  try {
    await tenantDisplaySettingsApi.deleteLogo()
    hasLogo.value = false
    await reloadBranding()
    store.toast('Logo rimosso', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    uploading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 12px; }

.settings-shell {
  display: grid;
  grid-template-columns: 220px 1fr;
  gap: 16px;
  align-items: start;
}
@media (max-width: 860px) {
  .settings-shell { grid-template-columns: 1fr; }
}

/* Navigazione sezioni */
.settings-nav {
  display: flex;
  flex-direction: column;
  gap: 4px;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md, 8px);
  padding: 8px;
}
.settings-nav-item {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 12px; border-radius: 7px;
  border: none; background: transparent; cursor: pointer;
  color: var(--text-secondary); font-size: 0.9rem; font-weight: 500;
  text-align: left; width: 100%;
  transition: background 0.12s, color 0.12s;
}
.settings-nav-item:hover { background: var(--accent-glow, rgba(99,102,241,0.06)); color: var(--text-primary); }
.settings-nav-item.active { background: var(--accent-glow, rgba(99,102,241,0.1)); color: var(--accent); font-weight: 600; }
.settings-nav-item .pi { font-size: 0.9rem; width: 18px; text-align: center; }

.settings-panel { min-width: 0; }

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

/* Logo */
.logo-row { display: flex; gap: 20px; align-items: flex-start; flex-wrap: wrap; }
.logo-preview {
  width: 200px; min-height: 90px;
  border: 1px dashed var(--border); border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  padding: 12px; background: var(--bg-base, transparent); flex-shrink: 0;
}
.logo-preview img { max-width: 100%; max-height: 120px; object-fit: contain; }
.logo-actions { display: flex; flex-direction: column; gap: 10px; }
.hidden-file { display: none; }

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
