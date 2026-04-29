<template>
  <div>
    <div class="page-header">
      <h1>Template Notifiche</h1>
      <div style="display:flex;gap:8px">
        <button v-if="canCreate" class="btn btn-ghost" @click="seedDefaults" :disabled="seeding" title="Crea i template standard mancanti">
          <span v-if="seeding" class="spinner" style="width:13px;height:13px"></span>
          <span v-else>⚙ Ripristina default</span>
        </button>
        <button v-if="canCreate" class="btn btn-primary" @click="openModal()">+ Nuovo template</button>
      </div>
    </div>

    <div class="card" style="margin-bottom:12px;padding:12px 16px;background:var(--accent-glow);border:1px solid var(--accent);border-radius:8px">
      <p style="margin:0;font-size:0.85rem;color:var(--text-secondary)">
        I template vengono usati per generare automaticamente il testo delle notifiche ai condomini.
        Usa le variabili per personalizzare il messaggio:
        <span class="var-chips">
          <span v-for="v in variables" :key="v" class="var-chip" @click="copyVar(v)">{{ wrapVar(v) }}</span>
        </span>
      </p>
    </div>

    <!-- Filtro per tipo -->
    <div class="toolbar">
      <select class="form-select" v-model="filterType" style="width:220px">
        <option value="">Tutti i tipi</option>
        <option v-for="t in commTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
      </select>
      <span class="text-muted" style="font-size:0.85rem">{{ filteredTemplates.length }} template</span>
    </div>

    <div v-if="loading" class="loading-state">Caricamento…</div>
    <div v-else-if="!filteredTemplates.length" class="empty-state">
      Nessun template configurato. Crea il primo per iniziare.
    </div>

    <div v-else class="template-grid">
      <div v-for="t in filteredTemplates" :key="t.id" class="template-card">
        <div class="template-card-header">
          <div class="template-card-title">
            <span class="badge" :class="typeBadge(t.communicationType)">{{ typeLabel(t.communicationType) }}</span>
            <span v-if="t.isDefault" class="badge badge-green" style="margin-left:4px">Default</span>
            <span class="template-name">{{ t.name }}</span>
          </div>
          <div class="row-actions">
            <button v-if="canEdit" class="btn-icon" @click="openModal(t)" title="Modifica">✎</button>
            <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)" @click="deleteTemplate(t.id)" title="Elimina">✕</button>
          </div>
        </div>
        <div class="template-subject">
          <span class="text-muted" style="font-size:0.78rem">Oggetto:</span>
          {{ t.subjectTemplate }}
        </div>
        <div class="template-body-preview">{{ previewBody(t.bodyTemplate) }}</div>
      </div>
    </div>

    <!-- Modal create/edit -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal" style="max-width:760px;width:95vw">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica template' : 'Nuovo template' }}</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" :class="{ 'has-error': errors.name }">
              <label class="form-label">Nome *</label>
              <input class="form-input" v-model="form.name" @input="clearError('name')" placeholder="es. Avviso rate mensili" />
              <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
            </div>
            <div class="form-group" :class="{ 'has-error': errors.communicationType }">
              <label class="form-label">Tipo comunicazione *</label>
              <select class="form-select" v-model="form.communicationType" @change="clearError('communicationType')" :disabled="!!editing">
                <option value="">— Seleziona —</option>
                <option v-for="t in commTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
              </select>
              <span v-if="errors.communicationType" class="field-error">{{ errors.communicationType }}</span>
            </div>
            <div class="form-group form-group--full" :class="{ 'has-error': errors.subjectTemplate }">
              <label class="form-label">Oggetto *</label>
              <input class="form-input" v-model="form.subjectTemplate" @input="clearError('subjectTemplate')"
                     placeholder="es. Avviso di pagamento rata {{installmentNumber}} — {{CondominiumName}}" />
              <span v-if="errors.subjectTemplate" class="field-error">{{ errors.subjectTemplate }}</span>
            </div>
            <div class="form-group form-group--full" :class="{ 'has-error': errors.bodyTemplate }">
              <label class="form-label">Corpo del messaggio *</label>
              <div class="var-chips" style="margin-bottom:6px">
                <span v-for="v in variables" :key="v" class="var-chip" @click="insertVar(v)" title="Inserisci variabile">{{ wrapVar(v) }}</span>
              </div>
              <textarea class="form-textarea" v-model="form.bodyTemplate" @input="clearError('bodyTemplate')"
                        rows="10" ref="bodyRef"
                        placeholder="Gentile {{RecipientName}},&#10;&#10;Le comunichiamo che…" />
              <span v-if="errors.bodyTemplate" class="field-error">{{ errors.bodyTemplate }}</span>
            </div>
            <div class="form-group form-group--full">
              <label class="form-label" style="display:flex;align-items:center;gap:8px;cursor:pointer">
                <input type="checkbox" v-model="form.isDefault" />
                <span>Imposta come template predefinito per questo tipo</span>
              </label>
            </div>
          </div>

          <!-- Anteprima -->
          <div v-if="form.bodyTemplate" class="preview-box">
            <div class="preview-title">Anteprima (valori di esempio)</div>
            <div class="preview-subject">{{ resolvePreview(form.subjectTemplate) }}</div>
            <div class="preview-body">{{ resolvePreview(form.bodyTemplate) }}</div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
          <button class="btn btn-primary" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            {{ editing ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import { notificationTemplateApi } from '@/services/api'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const loading   = ref(false)
const saving    = ref(false)
const seeding   = ref(false)
const templates = ref([])
const showModal = ref(false)
const editing   = ref(null)
const errors    = ref({})
const filterType = ref('')
const bodyRef   = ref(null)

const variables = [
  'RecipientName', 'UnitNumber', 'CondominiumName',
  'CommunicationTitle', 'CommunicationBody',
  'AdministratorName', 'AdministratorEmail', 'AdministratorPhone',
  'FiscalYearCode', 'DueDate', 'Amount', 'PaymentCode', 'Iban',
  'FeeTable', 'TotalAmount',
]

const commTypes = [
  { value: 'Notice',      label: 'Avviso' },
  { value: 'Meeting',     label: 'Assemblea' },
  { value: 'Maintenance', label: 'Manutenzione' },
  { value: 'Emergency',   label: 'Urgente' },
  { value: 'Info',        label: 'Informazione' },
  { value: 'FeeNotice',   label: 'Avviso di pagamento' },
]

const defaultForm = () => ({
  name: '', communicationType: '', subjectTemplate: '', bodyTemplate: '', isDefault: false,
})
const form = ref(defaultForm())

const filteredTemplates = computed(() =>
  templates.value.filter(t => !filterType.value || t.communicationType === filterType.value)
)

// ── Data ──────────────────────────────────────────────────────────────────────
async function loadTemplates() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await notificationTemplateApi.getByCondominium(store.selectedCondominioId)
    templates.value = data ?? []
  } catch {
    // handled globally
  } finally {
    loading.value = false
  }
}

// ── Modal ─────────────────────────────────────────────────────────────────────
function openModal(t = null) {
  editing.value = t?.id ?? null
  if (t) {
    form.value = {
      name:              t.name              ?? t.description ?? '',
      communicationType: t.communicationType ?? '',
      subjectTemplate:   t.subjectTemplate   ?? '',
      bodyTemplate:      t.bodyTemplate      ?? '',
      isDefault:         t.isDefault         ?? false,
    }
  } else {
    form.value = defaultForm()
  }
  errors.value  = {}
  showModal.value = true
}

// ── Validation ────────────────────────────────────────────────────────────────
function clearError(f) { delete errors.value[f] }

function validate() {
  const e = {}
  if (!form.value.name?.trim())             e.name             = 'Il nome è obbligatorio'
  if (!editing.value && !form.value.communicationType) e.communicationType = 'Seleziona il tipo'
  if (!form.value.subjectTemplate?.trim())  e.subjectTemplate  = "L'oggetto è obbligatorio"
  if (!form.value.bodyTemplate?.trim())     e.bodyTemplate     = 'Il corpo è obbligatorio'
  errors.value = e
  return Object.keys(e).length === 0
}

// ── Save ──────────────────────────────────────────────────────────────────────
async function save() {
  if (!validate()) return
  saving.value = true
  try {
    const payload = { ...form.value, condominiumId: store.selectedCondominioId }
    if (editing.value) {
      await notificationTemplateApi.update(editing.value, { name: form.value.name,
        subjectTemplate: form.value.subjectTemplate, bodyTemplate: form.value.bodyTemplate,
        isDefault: form.value.isDefault })
      store.toast('Template aggiornato', 'success')
    } else {
      await notificationTemplateApi.create(payload)
      store.toast('Template creato', 'success')
    }
    showModal.value = false
    await loadTemplates()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
    else store.toast(err.response.data?.Errors?.[0] ?? err.response.data?.message ?? 'Errore nel salvataggio', 'error')
  } finally {
    saving.value = false
  }
}

// ── Seed defaults ─────────────────────────────────────────────────────────────
async function seedDefaults() {
  if (!store.selectedCondominioId) return
  seeding.value = true
  try {
    const { data } = await notificationTemplateApi.seedDefaults(store.selectedCondominioId)
    if (data?.length) {
      store.toast(`${data.length} template standard creati`, 'success')
    } else {
      store.toast('Tutti i template standard sono già presenti', 'info')
    }
    await loadTemplates()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    seeding.value = false
  }
}

// ── Delete ────────────────────────────────────────────────────────────────────
async function deleteTemplate(id) {
  if (!confirm('Eliminare questo template?')) return
  try {
    await notificationTemplateApi.delete(id)
    store.toast('Template eliminato', 'success')
    await loadTemplates()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function typeBadge(t) {
  const map = { Notice: 'badge-blue', Meeting: 'badge-purple', Maintenance: 'badge-yellow',
                Emergency: 'badge-red', Info: 'badge-muted', FeeNotice: 'badge-green' }
  return map[t] ?? 'badge-muted'
}

function typeLabel(t) {
  return commTypes.find(c => c.value === t)?.label ?? t
}

function previewBody(body) {
  const lines = body?.split('\n') ?? []
  return lines.slice(0, 3).join(' ').substring(0, 120) + (body?.length > 120 ? '…' : '')
}

function wrapVar(v) { return `{{${v}}}` }

function copyVar(v) {
  navigator.clipboard?.writeText(`{{${v}}}`)
  store.toast(`{{${v}}} copiato`, 'info')
}

function insertVar(v) {
  const ta = bodyRef.value
  if (!ta) { form.value.bodyTemplate += `{{${v}}}`; return }
  const start = ta.selectionStart
  const end   = ta.selectionEnd
  const text  = form.value.bodyTemplate
  form.value.bodyTemplate = text.slice(0, start) + `{{${v}}}` + text.slice(end)
  setTimeout(() => { ta.focus(); ta.setSelectionRange(start + v.length + 4, start + v.length + 4) }, 0)
}

const previewCtx = {
  RecipientName: 'Mario Rossi', UnitNumber: 'A3', CondominiumName: 'Condominio Esempio',
  CommunicationTitle: 'Avviso di pagamento', CommunicationBody: '…',
  AdministratorName: 'Dott. Bianchi', AdministratorEmail: 'admin@example.com',
  AdministratorPhone: '02 1234567', FiscalYearCode: '2025', DueDate: '31/01/2025',
  Amount: '€ 350,00', PaymentCode: 'DW20250101ABCXYZ', Iban: 'IT60X0542811101000000123456',
}

function resolvePreview(tpl) {
  if (!tpl) return ''
  return Object.entries(previewCtx).reduce((s, [k, v]) => s.replaceAll(`{{${k}}}`, v), tpl)
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(loadTemplates)
watch(() => store.selectedCondominioId, loadTemplates)
</script>

<style scoped>
.template-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 12px;
}

.template-card {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 14px 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.template-card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 8px;
}

.template-card-title {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.template-name {
  font-weight: 600;
  font-size: 0.92rem;
  color: var(--text);
}

.template-subject {
  font-size: 0.82rem;
  color: var(--text-secondary);
  border-left: 3px solid var(--accent);
  padding-left: 8px;
}

.template-body-preview {
  font-size: 0.8rem;
  color: var(--text-muted);
  white-space: pre-wrap;
  overflow: hidden;
  max-height: 60px;
}

.var-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-top: 4px;
}

.var-chip {
  display: inline-block;
  background: var(--accent-glow);
  color: var(--accent);
  border: 1px solid var(--accent);
  border-radius: 4px;
  padding: 1px 6px;
  font-size: 0.72rem;
  font-family: monospace;
  cursor: pointer;
  user-select: none;
}
.var-chip:hover { background: var(--accent); color: #fff; }

.preview-box {
  margin-top: 16px;
  border: 1px dashed var(--border-active);
  border-radius: 8px;
  padding: 12px 16px;
  background: var(--bg-base);
}
.preview-title { font-size: 0.75rem; font-weight: 700; color: var(--text-muted); margin-bottom: 6px; text-transform: uppercase; letter-spacing: 0.05em; }
.preview-subject { font-weight: 600; font-size: 0.9rem; margin-bottom: 8px; }
.preview-body { font-size: 0.85rem; white-space: pre-wrap; color: var(--text-secondary); }

.badge-blue   { background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; }
.badge-purple { background: #f5f3ff; color: #6d28d9; border: 1px solid #ddd6fe; }
.badge-yellow { background: #fffbeb; color: #b45309; border: 1px solid #fde68a; }
.badge-red    { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
</style>
