<template>
  <div>
    <div class="page-header">
      <h1>Documenti</h1>
      <FeatureGate v-if="canCreate && store.selectedCondominioId"
        feature="ATTACHMENTS"
        unit-label="allegati"
        exhausted-label="Allegati esauriti — Acquista"
      >
        <button class="btn btn-primary" @click="openModal()">+ Nuovo documento</button>
      </FeatureGate>
    </div>

    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca per titolo, tag…" />
      <select class="form-select" v-model="filterCategory" style="width:180px">
        <option value="">Tutte le categorie</option>
        <option value="Verbale">Verbale assemblea</option>
        <option value="Bilancio">Bilancio</option>
        <option value="Contratto">Contratto</option>
        <option value="Fattura">Fattura</option>
        <option value="Regolamento">Regolamento</option>
        <option value="Altro">Altro</option>
      </select>
      <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;color:var(--text-secondary);cursor:pointer">
        <input type="checkbox" v-model="onlyVisible" />
        Solo visibili ai proprietari
      </label>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon">▤</div><div>Nessun documento trovato</div>
      </div>
      <div v-else class="docs-grid">
        <div v-for="d in filtered" :key="d.id" class="doc-card">
          <div class="doc-icon">{{ fileIcon(d.mimeType) }}</div>
          <div class="doc-body">
            <div class="doc-title">{{ d.title }}</div>
            <div class="doc-meta">
              <span class="badge badge-muted">{{ d.category }}</span>
              <span class="text-muted" style="font-size:0.75rem">{{ fmtSize(d.fileSize) }}</span>
              <span class="text-muted" style="font-size:0.75rem">{{ fmtDate(d.documentDate) }}</span>
              <span class="text-muted mono" style="font-size:0.72rem">{{ d.fileName }}</span>
            </div>
            <div class="doc-trace">
              <span title="Caricato da">⬆ {{ d.createdByFullName || '—' }} · {{ fmtDateTime(d.creationDate) }}</span>
              <span v-if="d.lastUpdatedByFullName" class="doc-trace-sep">·</span>
              <span v-if="d.lastUpdatedByFullName" title="Ultima modifica">✎ {{ d.lastUpdatedByFullName }} · {{ fmtDateTime(d.lastUpdateDate) }}</span>
            </div>
            <div v-if="d.tags" class="doc-tags">
              <span v-for="tag in d.tags.split(',').slice(0,3)" :key="tag" class="tag">{{ tag.trim() }}</span>
            </div>
            <div v-if="d.entityId && d.entityFullName" class="doc-entity">
              <span class="doc-entity-label">{{ entityLabel(d.entityFullName) }}</span>
              <button class="doc-entity-link" @click="goToEntity(d)">→ Vai al dettaglio</button>
            </div>
          </div>
          <div class="doc-actions">
            <span v-if="d.isVisibleToOwners" class="badge badge-blue" style="font-size:0.7rem">👁 Proprietari</span>
            <button class="btn-icon" title="Scarica" @click="downloadDoc(d)" :disabled="downloadingId === d.id">
              <span v-if="downloadingId === d.id" class="spinner" style="width:12px;height:12px"></span>
              <span v-else>⬇</span>
            </button>
            <button v-if="canEdit" class="btn-icon" @click="openModal(d)">✎</button>
            <button v-if="canDelete" class="btn-icon" @click="deleteItem(d.id)" style="color:var(--accent-red)">✕</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @mousedown.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuovo' }} documento</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Titolo *</label>
            <input class="form-input" v-model="form.title" />
          </div>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Categoria</label>
              <select class="form-select" v-model="form.category">
                <option value="Verbale">Verbale assemblea</option>
                <option value="Bilancio">Bilancio</option>
                <option value="Contratto">Contratto</option>
                <option value="Fattura">Fattura</option>
                <option value="Regolamento">Regolamento</option>
                <option value="Altro">Altro</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Data documento</label>
              <input class="form-input" type="date" v-model="form.documentDate" />
            </div>
          </div>

          <!-- File picker — solo in creazione -->
          <div v-if="!editing" class="form-group">
            <label class="form-label">File *</label>
            <div class="file-drop-area" :class="{ 'has-file': selectedFile, 'drag-over': isDragging }"
                 @dragover.prevent="isDragging=true" @dragleave="isDragging=false"
                 @drop.prevent="onFileDrop" @click="$refs.fileInput.click()">
              <input ref="fileInput" type="file" style="display:none" @change="onFileChange" />
              <template v-if="selectedFile">
                <span style="font-size:1.4rem">{{ fileIcon(selectedFile.type) }}</span>
                <span style="font-size:0.875rem;font-weight:500">{{ selectedFile.name }}</span>
                <span style="font-size:0.75rem;color:var(--text-muted)">{{ fmtSize(selectedFile.size) }}</span>
              </template>
              <template v-else>
                <span style="font-size:1.8rem;opacity:0.4">📂</span>
                <span style="font-size:0.85rem;color:var(--text-muted)">Clicca o trascina un file qui</span>
              </template>
            </div>
            <div v-if="fileError" class="field-error" style="margin-top:0.3rem">{{ fileError }}</div>
          </div>

          <!-- In modifica mostra il file attuale (readonly) -->
          <div v-else class="form-group">
            <label class="form-label">File corrente</label>
            <div style="font-size:0.875rem;color:var(--text-secondary);display:flex;align-items:center;gap:0.5rem">
              <span>{{ fileIcon(form.mimeType) }}</span>
              <span>{{ form.fileName }}</span>
              <span class="text-muted" style="font-size:0.75rem">({{ fmtSize(form.fileSize) }})</span>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Descrizione</label>
            <textarea class="form-textarea" v-model="form.description" rows="2"></textarea>
          </div>
          <div class="form-group">
            <label class="form-label">Tag (separati da virgola)</label>
            <input class="form-input" v-model="form.tags" placeholder="assemblea, 2024, bilancio" />
          </div>
          <div style="display:flex;gap:1.5rem">
            <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;cursor:pointer">
              <input type="checkbox" v-model="form.isVisibleToOwners" />
              Visibile ai proprietari
            </label>
            <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;cursor:pointer">
              <input type="checkbox" v-model="form.isArchived" />
              Archiviato
            </label>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
          <button class="btn btn-primary" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            {{ editing ? 'Salva' : 'Carica' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { documentApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'
import FeatureGate from '@/components/FeatureGate.vue'

const store  = useAppStore()
const router = useRouter()
const { canCreate, canEdit, canDelete } = usePermissions()

// ── Entity link ───────────────────────────────────────────────────────────────
const ENTITY_META = {
  'DomuWave.Services.Models.Expense':   { label: 'Spesa',    route: (condId) => `/condomini/${condId}/spese` },
  'DomuWave.Services.Models.WorkQuote': { label: 'Lavoro',   route: (condId) => `/condomini/${condId}/lavori` },
}

function entityLabel(entityFullName) {
  return ENTITY_META[entityFullName]?.label ?? entityFullName.split('.').pop()
}

function goToEntity(doc) {
  const meta = ENTITY_META[doc.entityFullName]
  if (!meta) return
  const condId = doc.condominiumId || store.selectedCondominioId
  router.push(meta.route(condId))
}

const documents     = ref([])
const loading       = ref(false)
const saving        = ref(false)
const showModal     = ref(false)
const editing       = ref(null)
const search        = ref('')
const filterCategory = ref('')
const onlyVisible   = ref(false)
const downloadingId = ref(null)

// ── File picker ───────────────────────────────────────────────────────────────
const fileInput   = ref(null)
const selectedFile = ref(null)
const fileError   = ref('')
const isDragging  = ref(false)

function onFileChange(e) {
  const file = e.target.files?.[0]
  if (file) setFile(file)
}
function onFileDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer.files?.[0]
  if (file) setFile(file)
}
function setFile(file) {
  selectedFile.value = file
  fileError.value = ''
  if (!form.value.title) form.value.title = file.name.replace(/\.[^.]+$/, '')
}

// ── Form ──────────────────────────────────────────────────────────────────────
const defaultForm = () => ({
  title: '', category: 'Altro', description: '', tags: '',
  documentDate: '', isVisibleToOwners: false, isArchived: false,
  fileName: '', mimeType: '', fileSize: 0,
})
const form = ref(defaultForm())

// ── Helpers ───────────────────────────────────────────────────────────────────
const fmtDate     = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const fmtDateTime = (d) => d ? new Date(d).toLocaleString('it-IT', { day:'2-digit', month:'2-digit', year:'2-digit', hour:'2-digit', minute:'2-digit' }) : '—'
const fmtSize = (s) => {
  if (!s) return '—'
  if (s < 1024) return s + ' B'
  if (s < 1048576) return (s / 1024).toFixed(1) + ' KB'
  return (s / 1048576).toFixed(1) + ' MB'
}
function fileIcon(mime) {
  if (!mime) return '📄'
  if (mime.includes('pdf')) return '📕'
  if (mime.includes('image')) return '🖼'
  if (mime.includes('word') || mime.includes('document')) return '📝'
  if (mime.includes('sheet') || mime.includes('excel')) return '📊'
  return '📄'
}

// ── Filtered list ─────────────────────────────────────────────────────────────
const filtered = computed(() => {
  let list = documents.value
  if (filterCategory.value) list = list.filter(d => d.category === filterCategory.value)
  if (onlyVisible.value)    list = list.filter(d => d.isVisibleToOwners)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(d => d.title?.toLowerCase().includes(q) || d.tags?.toLowerCase().includes(q))
  }
  return list
})

// ── Data ──────────────────────────────────────────────────────────────────────
async function loadData() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await documentApi.getByCondominium(store.selectedCondominioId)
    documents.value = data ?? []
  } catch { documents.value = [] } finally { loading.value = false }
}

function openModal(d = null) {
  editing.value  = d?.id ?? null
  form.value     = d ? { ...d } : defaultForm()
  selectedFile.value = null
  fileError.value    = ''
  showModal.value    = true
}

// ── Save (upload + update) ────────────────────────────────────────────────────
async function save() {
  if (!form.value.title) return store.toast('Il titolo è obbligatorio', 'error')
  if (!editing.value && !selectedFile.value) {
    fileError.value = 'Seleziona un file da caricare'
    return
  }
  saving.value = true
  try {
    if (editing.value) {
      await documentApi.update(editing.value, {
        title:             form.value.title,
        category:          form.value.category,
        documentDate:      form.value.documentDate || null,
        isVisibleToOwners: form.value.isVisibleToOwners,
        isArchived:        form.value.isArchived,
        tags:              form.value.tags,
        description:       form.value.description,
      })
    } else {
      const fileContent = await toBase64(selectedFile.value)
      await documentApi.upload({
        condominiumId:     store.selectedCondominioId,
        title:             form.value.title,
        category:          form.value.category,
        documentDate:      form.value.documentDate || null,
        isVisibleToOwners: form.value.isVisibleToOwners,
        tags:              form.value.tags,
        description:       form.value.description,
        fileName:          selectedFile.value.name,
        mimeType:          selectedFile.value.type || 'application/octet-stream',
        fileSize:          selectedFile.value.size,
        fileContent,
      })
    }
    store.toast('Documento salvato', 'success')
    showModal.value = false
    loadData()
  } catch { store.toast('Errore durante il salvataggio', 'error') } finally { saving.value = false }
}

function toBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload  = () => resolve(reader.result.split(',')[1]) // strip data:...;base64,
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

// ── Download ──────────────────────────────────────────────────────────────────
async function downloadDoc(d) {
  downloadingId.value = d.id
  try {
    const { data } = await documentApi.download(d.id)
    const url = URL.createObjectURL(new Blob([data], { type: d.mimeType || 'application/octet-stream' }))
    const a   = document.createElement('a')
    a.href = url; a.download = d.fileName; a.click()
    URL.revokeObjectURL(url)
  } catch { store.toast('Errore durante il download', 'error') }
  finally { downloadingId.value = null }
}

// ── Delete ────────────────────────────────────────────────────────────────────
async function deleteItem(id) {
  if (!confirm('Eliminare questo documento?')) return
  try { await documentApi.delete(id); store.toast('Documento eliminato', 'success'); loadData() }
  catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, loadData)
onMounted(loadData)
onUnmounted(() => window.removeEventListener('app:refresh', loadData))
window.addEventListener('app:refresh', loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; align-items: center; }
.search-input { flex: 1; min-width: 200px; max-width: 320px; }
.docs-grid { display: flex; flex-direction: column; gap: 0; }
.doc-card {
  display: flex; align-items: flex-start; gap: 1rem;
  padding: 0.9rem 0.5rem;
  border-bottom: 1px solid var(--border);
  transition: background 0.1s;
}
.doc-card:last-child { border-bottom: none; }
.doc-card:hover { background: var(--bg-hover); border-radius: var(--radius-sm); }
.doc-icon { font-size: 1.6rem; flex-shrink: 0; line-height: 1; padding-top: 0.1rem; }
.doc-body { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.3rem; }
.doc-title { font-weight: 500; font-size: 0.9rem; }
.doc-meta { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
.doc-tags  { display: flex; gap: 0.3rem; flex-wrap: wrap; }
.doc-trace { display: flex; align-items: center; gap: 0.35rem; flex-wrap: wrap; font-size: 0.72rem; color: var(--text-muted); }
.doc-trace-sep { color: var(--border); }
.tag { font-size: 0.72rem; padding: 0.1rem 0.45rem; border-radius: 99px; background: rgba(255,255,255,0.05); color: var(--text-muted); }
.doc-actions { display: flex; align-items: center; gap: 0.4rem; flex-shrink: 0; }
.doc-entity { display: flex; align-items: center; gap: 0.4rem; font-size: 0.75rem; }
.doc-entity-label { color: var(--text-muted); }
.doc-entity-link {
  background: none; border: none; padding: 0; cursor: pointer;
  color: var(--accent); font-size: 0.75rem;
  text-decoration: underline; text-underline-offset: 2px;
}
.doc-entity-link:hover { opacity: 0.8; }

/* File drop area */
.file-drop-area {
  border: 2px dashed var(--border);
  border-radius: var(--radius-md);
  padding: 1.5rem 1rem;
  display: flex; flex-direction: column; align-items: center; gap: 0.4rem;
  cursor: pointer; transition: border-color 0.15s, background 0.15s;
  text-align: center;
}
.file-drop-area:hover,
.file-drop-area.drag-over { border-color: var(--accent); background: var(--accent-glow); }
.file-drop-area.has-file   { border-color: var(--accent-green); background: rgba(34,197,94,0.05); }
</style>
