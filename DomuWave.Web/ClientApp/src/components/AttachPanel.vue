<template>
  <div class="attach-panel">

    <!-- Current attachments -->
    <div class="ap-section">
      <div class="ap-section-label">Allegati correnti</div>
      <div v-if="loadingAttachments" class="ap-empty">Caricamento…</div>
      <div v-else-if="!attachments.length" class="ap-empty">Nessun allegato</div>
      <div v-else class="ap-list">
        <div v-for="att in attachments" :key="att.id" class="ap-item">
          <span class="ap-icon">{{ mimeIcon(att.mimeType) }}</span>
          <div class="ap-info">
            <span class="ap-title">{{ att.documentTitle }}</span>
            <span class="ap-sub">{{ att.fileName }}</span>
          </div>
          <button class="btn-icon ap-remove"
                  :disabled="removingId === att.id"
                  @click="removeAtt(att)" title="Rimuovi allegato">
            <span v-if="removingId === att.id" class="spinner" style="width:10px;height:10px"></span>
            <span v-else>✕</span>
          </button>
        </div>
      </div>
    </div>

    <div class="ap-divider"></div>

    <!-- Add from existing documents -->
    <div class="ap-section">
      <div class="ap-section-label">Aggiungi documento esistente</div>
      <input class="form-input ap-search" v-model="docSearch" placeholder="Cerca per titolo o file…" />
      <div v-if="!attachableDocuments.length" class="ap-empty">
        {{ docSearch ? 'Nessun risultato' : 'Tutti i documenti sono già allegati' }}
      </div>
      <div v-else class="ap-list ap-list-pick">
        <div v-for="doc in attachableDocuments" :key="doc.id" class="ap-item">
          <span class="ap-icon">{{ mimeIcon(doc.mimeType) }}</span>
          <div class="ap-info">
            <span class="ap-title">{{ doc.title }}</span>
            <span class="ap-sub">{{ doc.fileName }}</span>
          </div>
          <button class="btn btn-sm btn-ghost" :disabled="addingId === doc.id" @click="addExisting(doc)">
            <span v-if="addingId === doc.id" class="spinner" style="width:10px;height:10px"></span>
            <span v-else>+ Allega</span>
          </button>
        </div>
      </div>
    </div>

    <div class="ap-divider"></div>

    <!-- Upload + attach new document -->
    <div class="ap-section">
      <button class="ap-upload-toggle" @click="showUpload = !showUpload">
        <span>{{ showUpload ? '▲' : '▼' }}</span>
        Carica e allega nuovo documento
      </button>
      <div v-if="showUpload" class="ap-upload-form">
        <div class="form-grid ap-upload-grid">
          <div class="form-group">
            <label class="form-label">Titolo *</label>
            <input class="form-input" v-model="up.title" :class="{ 'input-error': upErr.title }" @input="upErr.title=''" />
            <span v-if="upErr.title" class="field-error">{{ upErr.title }}</span>
          </div>
          <div class="form-group">
            <label class="form-label">Categoria</label>
            <select class="form-select" v-model="up.category">
              <option value="Altro">Altro</option>
              <option value="Verbali">Verbali</option>
              <option value="Preventivi">Preventivi</option>
              <option value="Consuntivi">Consuntivi</option>
              <option value="Contratti">Contratti</option>
              <option value="Normativa">Normativa</option>
              <option value="Certificazioni">Certificazioni</option>
            </select>
          </div>
          <div class="form-group form-group--full">
            <label class="form-label">File *</label>
            <input type="file" class="form-input" ref="fileInput" @change="onFileChange" :class="{ 'input-error': upErr.file }" />
            <span v-if="upErr.file" class="field-error">{{ upErr.file }}</span>
          </div>
        </div>
        <div class="ap-upload-actions">
          <button class="btn btn-primary btn-sm" :disabled="uploading" @click="uploadAndAttach">
            <span v-if="uploading" class="spinner" style="width:12px;height:12px"></span>
            <span v-else>⬆ Carica e allega</span>
          </button>
          <button class="btn btn-ghost btn-sm" @click="resetUpload">Annulla</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { notificationAttachmentApi, documentApi } from '@/services/api'

const props = defineProps({
  // list of notification IDs to attach to (1 for single notif, N for comm-level)
  notificationIds: { type: Array, required: true },
  condominiumId:   { type: Number, required: true },
})

const emit = defineEmits(['changed'])
const store = useAppStore()

// ── State ─────────────────────────────────────────────────────────────────────
const attachments       = ref([])  // from first notificationId
const availableDocs     = ref([])
const loadingAttachments = ref(false)
const removingId        = ref(null)
const addingId          = ref(null)
const docSearch         = ref('')
const showUpload        = ref(false)

// upload form
const up       = ref({ title: '', category: 'Altro', file: null })
const upErr    = ref({ title: '', file: '' })
const uploading = ref(false)
const fileInput = ref(null)

// ── Load ──────────────────────────────────────────────────────────────────────
async function load() {
  if (!props.notificationIds.length || !props.condominiumId) return
  loadingAttachments.value = true
  try {
    const [attRes, docRes] = await Promise.all([
      notificationAttachmentApi.getByNotification(props.notificationIds[0]),
      documentApi.getByCondominium(props.condominiumId),
    ])
    attachments.value   = attRes.data  ?? []
    availableDocs.value = docRes.data  ?? []
  } catch { /* global handler */ } finally { loadingAttachments.value = false }
}

watch(() => props.notificationIds, load, { immediate: true })

// ── Computed ──────────────────────────────────────────────────────────────────
const attachableDocuments = computed(() => {
  const ids = new Set(attachments.value.map(a => a.documentId))
  let list  = availableDocs.value.filter(d => !ids.has(d.id))
  if (docSearch.value.trim()) {
    const q = docSearch.value.trim().toLowerCase()
    list = list.filter(d => d.title?.toLowerCase().includes(q) || d.fileName?.toLowerCase().includes(q))
  }
  return list
})

// ── Add existing ──────────────────────────────────────────────────────────────
async function addExisting(doc) {
  addingId.value = doc.id
  try {
    // add to all notificationIds; collect result from first only
    const results = await Promise.all(
      props.notificationIds.map(nid => notificationAttachmentApi.add(nid, doc.id))
    )
    attachments.value.push(results[0].data)
    emit('changed')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { addingId.value = null }
}

// ── Remove ────────────────────────────────────────────────────────────────────
async function removeAtt(att) {
  removingId.value = att.id
  try {
    // Remove from first notif (the one whose attachments we show)
    // For multi-notif (comm-level), also remove the matching doc from the others
    await notificationAttachmentApi.remove(att.id)

    if (props.notificationIds.length > 1) {
      // best-effort removal from sibling notifications
      const siblings = attachments.value  // only from notif[0], but try others via re-fetch
      // Re-fetch attachments for other notifs and remove matching documentId
      const docId = att.documentId
      await Promise.allSettled(
        props.notificationIds.slice(1).map(async nid => {
          const { data } = await notificationAttachmentApi.getByNotification(nid)
          const match = (data ?? []).find(a => a.documentId === docId)
          if (match) await notificationAttachmentApi.remove(match.id)
        })
      )
    }

    attachments.value = attachments.value.filter(a => a.id !== att.id)
    emit('changed')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { removingId.value = null }
}

// ── Upload + attach ───────────────────────────────────────────────────────────
function onFileChange(e) {
  up.value.file = e.target.files[0] ?? null
  upErr.value.file = ''
  if (up.value.file && !up.value.title)
    up.value.title = up.value.file.name.replace(/\.[^.]+$/, '')
}

function validateUpload() {
  const e = {}
  if (!up.value.title.trim()) e.title = 'Il titolo è obbligatorio'
  if (!up.value.file)         e.file  = 'Seleziona un file'
  upErr.value = e
  return !Object.keys(e).length
}

async function uploadAndAttach() {
  if (!validateUpload()) return
  uploading.value = true
  try {
    const file    = up.value.file
    const arrBuf  = await file.arrayBuffer()
    const bytes   = new Uint8Array(arrBuf)
    // encode to base64
    let binary = ''
    for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i])
    const base64 = btoa(binary)

    const payload = {
      condominiumId:     props.condominiumId,
      title:             up.value.title.trim(),
      category:          up.value.category,
      fileName:          file.name,
      mimeType:          file.type || 'application/octet-stream',
      fileSize:          file.size,
      fileContent:       base64,
      isVisibleToOwners: false,
    }
    const { data: doc } = await documentApi.upload(payload)

    // Attach to all notification IDs
    const results = await Promise.all(
      props.notificationIds.map(nid => notificationAttachmentApi.add(nid, doc.id))
    )
    attachments.value.push(results[0].data)
    availableDocs.value.push(doc)
    resetUpload()
    emit('changed')
    store.toast('Documento caricato e allegato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { uploading.value = false }
}

function resetUpload() {
  up.value    = { title: '', category: 'Altro', file: null }
  upErr.value = { title: '', file: '' }
  showUpload.value = false
  if (fileInput.value) fileInput.value.value = ''
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function mimeIcon(mime) {
  if (!mime) return '📄'
  if (mime.includes('pdf'))                                 return '📕'
  if (mime.includes('image'))                               return '🖼'
  if (mime.includes('word') || mime.includes('document'))  return '📝'
  if (mime.includes('sheet') || mime.includes('excel'))    return '📊'
  return '📄'
}
</script>

<style scoped>
.attach-panel { display: flex; flex-direction: column; gap: 0; }

.ap-section { padding: 0.75rem 0; }
.ap-section-label { font-size: 0.78rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.04em; color: var(--text-muted); margin-bottom: 0.5rem; }
.ap-divider { border-top: 1px solid var(--border); }
.ap-empty { font-size: 0.85rem; color: var(--text-muted); }
.ap-search { margin-bottom: 0.5rem; width: 100%; }

.ap-list {
  display: flex; flex-direction: column; gap: 0;
  border: 1px solid var(--border); border-radius: 6px;
  overflow: hidden; max-height: 200px; overflow-y: auto;
}
.ap-list-pick { max-height: 180px; }
.ap-item {
  display: flex; align-items: center; gap: 0.6rem;
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid var(--border);
  transition: background 0.1s;
}
.ap-item:last-child { border-bottom: none; }
.ap-item:hover { background: var(--bg-base); }
.ap-icon { font-size: 1.2rem; flex-shrink: 0; }
.ap-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 1px; }
.ap-title { font-size: 0.875rem; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ap-sub   { font-size: 0.75rem; color: var(--text-muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ap-remove { color: var(--accent-red); }

/* Upload toggle */
.ap-upload-toggle {
  display: flex; align-items: center; gap: 0.5rem;
  font-size: 0.85rem; color: var(--accent);
  background: none; border: none; cursor: pointer;
  padding: 0; margin-bottom: 0.5rem;
}
.ap-upload-toggle:hover { text-decoration: underline; }

.ap-upload-form {
  border: 1px solid var(--border);
  border-radius: 6px;
  padding: 0.75rem;
  background: var(--bg-base);
}
.ap-upload-grid { grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.ap-upload-actions { display: flex; gap: 0.5rem; margin-top: 0.5rem; }

.input-error { border-color: var(--accent-red) !important; }
.field-error { color: var(--accent-red); font-size: 0.78rem; }
</style>
