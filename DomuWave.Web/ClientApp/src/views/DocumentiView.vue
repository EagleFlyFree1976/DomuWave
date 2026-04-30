<template>
  <div>
    <div class="page-header">
      <h1>Documenti</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openModal()">+ Nuovo documento</button>
    </div>

    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca per titolo, tag…" @input="onSearch" />
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
            </div>
            <div v-if="d.tags" class="doc-tags">
              <span v-for="tag in d.tags.split(',').slice(0,3)" :key="tag" class="tag">{{ tag.trim() }}</span>
            </div>
          </div>
          <div class="doc-actions">
            <span v-if="d.isVisibleToOwners" class="badge badge-blue" style="font-size:0.7rem">👁 Proprietari</span>
            <button v-if="canEdit" class="btn-icon" @click="openModal(d)">✎</button>
            <button v-if="canDelete" class="btn-icon" @click="deleteItem(d.id)" style="color:var(--accent-red)">✕</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
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
            <div class="form-group">
              <label class="form-label">Nome file</label>
              <input class="form-input" v-model="form.fileName" />
            </div>
            <div class="form-group">
              <label class="form-label">Percorso file</label>
              <input class="form-input" v-model="form.filePath" />
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
            {{ editing ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { documentApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()
const documents = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterCategory = ref('')
const onlyVisible = ref(false)

const defaultForm = () => ({ title: '', category: 'Altro', description: '', fileName: '', filePath: '', fileSize: 0, mimeType: 'application/pdf', documentDate: '', isVisibleToOwners: false, isArchived: false, tags: '' })
const form = ref(defaultForm())

const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT') : '—'
const fmtSize = (s) => s ? (s < 1024 ? s + ' B' : s < 1048576 ? (s/1024).toFixed(1) + ' KB' : (s/1048576).toFixed(1) + ' MB') : '—'
function fileIcon(mime) {
  if (!mime) return '📄'
  if (mime.includes('pdf')) return '📕'
  if (mime.includes('image')) return '🖼'
  if (mime.includes('word') || mime.includes('document')) return '📝'
  if (mime.includes('sheet') || mime.includes('excel')) return '📊'
  return '📄'
}

const filtered = computed(() => {
  let list = documents.value
  if (filterCategory.value) list = list.filter(d => d.category === filterCategory.value)
  if (onlyVisible.value) list = list.filter(d => d.isVisibleToOwners)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(d => d.title?.toLowerCase().includes(q) || d.tags?.toLowerCase().includes(q))
  }
  return list
})

async function loadData() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await documentApi.getByCondominium(store.selectedCondominioId)
    documents.value = data
  } catch { documents.value = [] } finally { loading.value = false }
}

function openModal(d = null) {
  editing.value = d?.id ?? null
  form.value = d ? { ...d } : defaultForm()
  showModal.value = true
}

async function save() {
  if (!form.value.title) return store.toast('Il titolo è obbligatorio', 'error')
  saving.value = true
  try {
    if (editing.value) await documentApi.update(editing.value, form.value)
    else await documentApi.create({ ...form.value, condominiumId: store.selectedCondominioId })
    store.toast('Documento salvato', 'success')
    showModal.value = false
    loadData()
  } catch { store.toast('Errore', 'error') } finally { saving.value = false }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questo documento?')) return
  try { await documentApi.delete(id); store.toast('Documento eliminato', 'success'); loadData() }
  catch { store.toast('Errore', 'error') }
}

function onSearch() {}

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
.doc-tags { display: flex; gap: 0.3rem; flex-wrap: wrap; }
.tag { font-size: 0.72rem; padding: 0.1rem 0.45rem; border-radius: 99px; background: rgba(255,255,255,0.05); color: var(--text-muted); }
.doc-actions { display: flex; align-items: center; gap: 0.4rem; flex-shrink: 0; }
</style>
