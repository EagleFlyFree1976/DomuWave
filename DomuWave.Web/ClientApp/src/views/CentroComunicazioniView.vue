<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import { useAuthStore } from '@/stores/authStore'
import { useSessionStore } from '@/stores/sessionStore'
import { boardPostApi, faultApi, privateThreadApi } from '@/services/api'
import { userApi } from '@/services/userService'

const store   = useAppStore()
const auth    = useAuthStore()
const session = useSessionStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const currentUserId = computed(() => auth.currentUser?.id)
const isAdminOrSuper = computed(() => session.isSuperAdmin || session.isTenantAdmin)

function canDeletePost(post)    { return isAdminOrSuper.value || post.authorUserId == currentUserId.value }
function canDeleteComment(c)    { return isAdminOrSuper.value || c.authorUserId    == currentUserId.value }
function canDeleteFault(fault)  { return isAdminOrSuper.value || fault.reporterUserId == currentUserId.value }

// ── Tab attiva ─────────────────────────────────────────────────────────────
const activeTab = ref('bacheca')

// ─────────────────────────────────────────────────────────────────────────────
// BACHECA
// ─────────────────────────────────────────────────────────────────────────────
const posts        = ref([])
const loadingPosts = ref(false)
const expandedPost = ref(null)
const postComments = ref({}) // postId -> []
const showPostModal = ref(false)
const editingPost   = ref(null)
const postErrors    = ref({})

const defaultPostForm = () => ({ title: '', body: '', isPinned: false })
const postForm = ref(defaultPostForm())

async function loadPosts() {
  if (!store.selectedCondominioId) return
  loadingPosts.value = true
  try {
    const { data } = await boardPostApi.getByCondominium(store.selectedCondominioId)
    posts.value = data ?? []
  } catch { } finally { loadingPosts.value = false }
}

async function togglePost(post) {
  if (expandedPost.value === post.id) { expandedPost.value = null; return }
  expandedPost.value = post.id
  if (!postComments.value[post.id]) await loadComments(post.id)
}

async function loadComments(postId) {
  try {
    const { data } = await boardPostApi.getComments(postId)
    postComments.value[postId] = data ?? []
  } catch { }
}

function openPostModal(post = null) {
  editingPost.value = post?.id ?? null
  postForm.value = post ? { title: post.title, body: post.body, isPinned: post.isPinned } : defaultPostForm()
  postErrors.value = {}
  showPostModal.value = true
}

function validatePost() {
  const e = {}
  if (!postForm.value.title?.trim()) e.title = 'Il titolo è obbligatorio'
  if (!postForm.value.body?.trim())  e.body  = 'Il testo è obbligatorio'
  postErrors.value = e
  return !Object.keys(e).length
}

const savingPost = ref(false)
async function savePost() {
  if (!validatePost()) return
  savingPost.value = true
  try {
    const payload = { ...postForm.value, condominiumId: store.selectedCondominioId }
    if (editingPost.value) {
      await boardPostApi.update(editingPost.value, payload)
      store.toast('Post aggiornato', 'success')
    } else {
      await boardPostApi.create(payload)
      store.toast('Post pubblicato', 'success')
    }
    showPostModal.value = false
    await loadPosts()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingPost.value = false }
}

async function deletePost(id) {
  if (!confirm('Eliminare questo post dalla bacheca?')) return
  try {
    await boardPostApi.delete(id)
    store.toast('Post eliminato', 'success')
    if (expandedPost.value === id) expandedPost.value = null
    await loadPosts()
  } catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

// Commenti
const newCommentBody = ref({}) // postId -> string
const savingComment  = ref(null)

async function submitComment(postId) {
  const body = (newCommentBody.value[postId] ?? '').trim()
  if (!body) return
  savingComment.value = postId
  try {
    await boardPostApi.createComment(postId, { body })
    newCommentBody.value[postId] = ''
    await loadComments(postId)
    const postObj = posts.value.find(p => p.id === postId)
    if (postObj) postObj.commentCount = (postComments.value[postId] ?? []).length
  } catch (err) { if (!err?.response) store.toast('Errore invio commento', 'error') }
  finally { savingComment.value = null }
}

async function deleteComment(postId, commentId) {
  if (!confirm('Eliminare questo commento?')) return
  try {
    await boardPostApi.deleteComment(commentId)
    await loadComments(postId)
    const postObj = posts.value.find(p => p.id === postId)
    if (postObj) postObj.commentCount = (postComments.value[postId] ?? []).length
  } catch (err) { if (!err?.response) store.toast('Errore eliminazione commento', 'error') }
}

// ─────────────────────────────────────────────────────────────────────────────
// GUASTI
// ─────────────────────────────────────────────────────────────────────────────
const faults        = ref([])
const loadingFaults = ref(false)
const expandedFault = ref(null)
const faultMessages = ref({}) // faultId -> []
const showFaultModal = ref(false)
const faultErrors    = ref({})

const defaultFaultForm = () => ({ title: '', description: '', unitId: null })
const faultForm = ref(defaultFaultForm())

const faultStatusLabels = { 0: 'Aperta', 1: 'In lavorazione', 2: 'Risolta' }
const faultStatusBadge  = (s) => s === 0 ? 'badge-red' : s === 1 ? 'badge-yellow' : 'badge-green'

async function loadFaults() {
  if (!store.selectedCondominioId) return
  loadingFaults.value = true
  try {
    const { data } = await faultApi.getByCondominium(store.selectedCondominioId)
    faults.value = data ?? []
  } catch { } finally { loadingFaults.value = false }
}

async function toggleFault(fault) {
  if (expandedFault.value === fault.id) { expandedFault.value = null; return }
  expandedFault.value = fault.id
  if (!faultMessages.value[fault.id]) await loadFaultMessages(fault.id)
}

async function loadFaultMessages(faultId) {
  try {
    const { data } = await faultApi.getMessages(faultId)
    faultMessages.value[faultId] = data ?? []
  } catch { }
}

function openFaultModal() {
  faultForm.value = defaultFaultForm()
  faultErrors.value = {}
  showFaultModal.value = true
}

function validateFault() {
  const e = {}
  if (!faultForm.value.title?.trim())       e.title       = 'Il titolo è obbligatorio'
  if (!faultForm.value.description?.trim()) e.description = 'La descrizione è obbligatoria'
  faultErrors.value = e
  return !Object.keys(e).length
}

const savingFault = ref(false)
async function saveFault() {
  if (!validateFault()) return
  savingFault.value = true
  try {
    const payload = { ...faultForm.value, condominiumId: store.selectedCondominioId }
    await faultApi.create(payload)
    store.toast('Segnalazione inviata', 'success')
    showFaultModal.value = false
    await loadFaults()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingFault.value = false }
}

async function updateFaultStatus(fault, statusId) {
  try {
    await faultApi.updateStatus(fault.id, { statusId })
    fault.statusId   = statusId
    fault.statusName = faultStatusLabels[statusId]
    store.toast('Stato aggiornato', 'success')
  } catch (err) { if (!err?.response) store.toast('Errore aggiornamento stato', 'error') }
}

async function deleteFault(id) {
  if (!confirm('Eliminare questa segnalazione?')) return
  try {
    await faultApi.delete(id)
    store.toast('Segnalazione eliminata', 'success')
    if (expandedFault.value === id) expandedFault.value = null
    await loadFaults()
  } catch (err) { if (!err?.response) store.toast('Errore eliminazione', 'error') }
}

const newFaultMsg     = ref({}) // faultId -> string
const savingFaultMsg  = ref(null)

async function submitFaultMessage(faultId) {
  const body = (newFaultMsg.value[faultId] ?? '').trim()
  if (!body) return
  savingFaultMsg.value = faultId
  try {
    await faultApi.createMessage(faultId, { body })
    newFaultMsg.value[faultId] = ''
    await loadFaultMessages(faultId)
    const f = faults.value.find(x => x.id === faultId)
    if (f) f.messageCount = (faultMessages.value[faultId] ?? []).length
  } catch (err) { if (!err?.response) store.toast('Errore invio messaggio', 'error') }
  finally { savingFaultMsg.value = null }
}

// ─────────────────────────────────────────────────────────────────────────────
// MESSAGGI PRIVATI
// ─────────────────────────────────────────────────────────────────────────────
const threads         = ref([])
const loadingThreads  = ref(false)
const activeThread    = ref(null)
const threadMessages  = ref([])
const loadingMessages = ref(false)
const newMsgBody      = ref('')
const sendingMsg      = ref(false)

// Nuova conversazione
const showNewThreadModal = ref(false)
const newThreadSearch    = ref('')
const newThreadResults   = ref([])
const searchingUsers     = ref(false)
const startingThread     = ref(false)

async function searchCondomini() {
  const q = newThreadSearch.value.trim()
  if (!q) { newThreadResults.value = []; return }
  searchingUsers.value = true
  try {
    const { data } = await userApi.search({ roles: 'Condomino', search: q })
    newThreadResults.value = Array.isArray(data) ? data : (data?.items ?? [])
  } catch { } finally { searchingUsers.value = false }
}

async function startThread(user) {
  startingThread.value = true
  try {
    const { data } = await privateThreadApi.getOrCreate({
      condominiumId:    store.selectedCondominioId,
      condominioUserId: user.id,
    })
    showNewThreadModal.value = false
    newThreadSearch.value    = ''
    newThreadResults.value   = []
    await loadThreads()
    const thread = threads.value.find(t => t.id === data.id) ?? data
    await openThread(thread)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { startingThread.value = false }
}

async function loadThreads() {
  if (!store.selectedCondominioId) return
  loadingThreads.value = true
  try {
    const { data } = await privateThreadApi.getByCondominium(store.selectedCondominioId)
    threads.value = data ?? []
  } catch { } finally { loadingThreads.value = false }
}

async function openThread(thread) {
  activeThread.value = thread
  loadingMessages.value = true
  try {
    const { data } = await privateThreadApi.getMessages(thread.id)
    threadMessages.value = data ?? []
    await privateThreadApi.markRead(thread.id)
    thread.unreadCount = 0
  } catch { } finally { loadingMessages.value = false }
}

async function sendMessage() {
  const body = newMsgBody.value.trim()
  if (!body || !activeThread.value) return
  sendingMsg.value = true
  try {
    const { data } = await privateThreadApi.createMessage(activeThread.value.id, { body })
    threadMessages.value.push(data)
    newMsgBody.value = ''
    activeThread.value.lastMessageBody = body
    activeThread.value.lastMessageDate = new Date().toISOString()
  } catch (err) { if (!err?.response) store.toast('Errore invio messaggio', 'error') }
  finally { sendingMsg.value = false }
}

// ─────────────────────────────────────────────────────────────────────────────
// Lifecycle
// ─────────────────────────────────────────────────────────────────────────────
function fmtDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('it-IT', { day:'2-digit', month:'2-digit', year:'numeric', hour:'2-digit', minute:'2-digit' })
}

async function loadTab(tab) {
  activeTab.value = tab
  if (tab === 'bacheca')  await loadPosts()
  if (tab === 'guasti')   await loadFaults()
  if (tab === 'messaggi') await loadThreads()
}

onMounted(() => loadPosts())
watch(() => store.selectedCondominioId, () => loadTab(activeTab.value))
</script>

<template>
  <div class="card">
    <div class="card-header" style="display:flex;align-items:center;justify-content:space-between;gap:12px;flex-wrap:wrap">
      <h1 class="card-title" style="margin:0">Bacheca & Messaggi</h1>
    </div>

    <!-- Tab bar -->
    <div class="tab-bar">
      <button :class="['tab-btn', activeTab === 'bacheca'  ? 'active' : '']" @click="loadTab('bacheca')">📌 Bacheca</button>
      <button :class="['tab-btn', activeTab === 'guasti'   ? 'active' : '']" @click="loadTab('guasti')">🔧 Segnalazioni Guasti</button>
      <button :class="['tab-btn', activeTab === 'messaggi' ? 'active' : '']" @click="loadTab('messaggi')">✉ Messaggi Privati</button>
    </div>

    <!-- ════════ BACHECA ════════ -->
    <div v-if="activeTab === 'bacheca'" class="tab-content">
      <div class="toolbar" style="justify-content:flex-end;margin-bottom:12px">
        <button class="btn btn-primary btn-sm" @click="openPostModal()">+ Nuovo post</button>
      </div>

      <div v-if="loadingPosts" class="loading-state">Caricamento…</div>
      <div v-else-if="!posts.length" class="empty-state">Nessun post in bacheca.</div>

      <div v-else class="posts-list">
        <div v-for="post in posts" :key="post.id" class="post-card" :class="{ pinned: post.isPinned }">
          <div class="post-header" @click="togglePost(post)" style="cursor:pointer">
            <div style="display:flex;align-items:center;gap:8px;flex:1;min-width:0">
              <span v-if="post.isPinned" style="color:var(--accent);font-size:0.8rem">📌</span>
              <strong class="post-title">{{ post.title }}</strong>
              <span class="badge badge-muted" style="font-size:0.7rem">{{ post.commentCount }} commenti</span>
            </div>
            <div style="display:flex;align-items:center;gap:6px;flex-shrink:0">
              <span class="text-muted" style="font-size:0.75rem">{{ post.authorFullName }} · {{ fmtDate(post.creationDate) }}</span>
              <button v-if="isAdminOrSuper" class="btn-icon" style="font-size:0.75rem" title="Modifica" @click.stop="openPostModal(post)">✎</button>
              <button v-if="canDeletePost(post)" class="btn-icon" style="color:var(--accent-red);font-size:0.75rem" title="Elimina" @click.stop="deletePost(post.id)">✕</button>
            </div>
          </div>

          <div v-if="expandedPost === post.id" class="post-body">
            <p style="white-space:pre-wrap;margin:0 0 12px">{{ post.body }}</p>

            <!-- Commenti -->
            <div class="comments-section">
              <div v-if="!postComments[post.id]" class="text-muted" style="font-size:0.8rem">Caricamento commenti…</div>
              <div v-else>
                <div v-for="c in postComments[post.id]" :key="c.id" class="comment-row">
                  <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:8px">
                    <div>
                      <span class="text-secondary" style="font-size:0.78rem;font-weight:600">{{ c.authorFullName }}</span>
                      <span class="text-muted" style="font-size:0.72rem;margin-left:6px">{{ fmtDate(c.creationDate) }}</span>
                      <p style="margin:4px 0 0;white-space:pre-wrap;font-size:0.85rem">{{ c.body }}</p>
                    </div>
                    <button v-if="canDeleteComment(c)" class="btn-icon" style="color:var(--accent-red);font-size:0.72rem;flex-shrink:0" @click="deleteComment(post.id, c.id)">✕</button>
                  </div>
                </div>
                <div v-if="!postComments[post.id].length" class="text-muted" style="font-size:0.8rem;margin-bottom:8px">Nessun commento.</div>
              </div>
              <div class="comment-input-row">
                <textarea v-model="newCommentBody[post.id]" class="form-textarea" rows="2" placeholder="Scrivi un commento…" style="flex:1;resize:vertical;min-height:48px"></textarea>
                <button class="btn btn-primary btn-sm" :disabled="savingComment === post.id || !(newCommentBody[post.id]?.trim())" @click="submitComment(post.id)">
                  <span v-if="savingComment === post.id" class="spinner" style="width:12px;height:12px"></span>
                  <span v-else>Invia</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ════════ GUASTI ════════ -->
    <div v-if="activeTab === 'guasti'" class="tab-content">
      <div class="toolbar" style="justify-content:flex-end;margin-bottom:12px">
        <button class="btn btn-primary btn-sm" @click="openFaultModal()">+ Nuova segnalazione</button>
      </div>

      <div v-if="loadingFaults" class="loading-state">Caricamento…</div>
      <div v-else-if="!faults.length" class="empty-state">Nessuna segnalazione presente.</div>

      <div v-else class="posts-list">
        <div v-for="fault in faults" :key="fault.id" class="post-card">
          <div class="post-header" @click="toggleFault(fault)" style="cursor:pointer">
            <div style="display:flex;align-items:center;gap:8px;flex:1;min-width:0">
              <span :class="['badge', faultStatusBadge(fault.statusId)]" style="flex-shrink:0">{{ fault.statusName }}</span>
              <strong class="post-title">{{ fault.title }}</strong>
              <span v-if="fault.unitDisplayName" class="text-muted" style="font-size:0.75rem">· {{ fault.unitDisplayName }}</span>
              <span class="badge badge-muted" style="font-size:0.7rem">{{ fault.messageCount }} messaggi</span>
            </div>
            <div style="display:flex;align-items:center;gap:6px;flex-shrink:0">
              <span class="text-muted" style="font-size:0.75rem">{{ fault.reporterFullName }} · {{ fmtDate(fault.creationDate) }}</span>
              <button v-if="canDeleteFault(fault)" class="btn-icon" style="color:var(--accent-red);font-size:0.75rem" title="Elimina" @click.stop="deleteFault(fault.id)">✕</button>
            </div>
          </div>

          <div v-if="expandedFault === fault.id" class="post-body">
            <p style="white-space:pre-wrap;margin:0 0 12px">{{ fault.description }}</p>

            <!-- Cambio stato (admin) -->
            <div v-if="isAdminOrSuper" style="display:flex;gap:6px;margin-bottom:12px;flex-wrap:wrap">
              <span class="text-muted" style="font-size:0.8rem;line-height:28px">Cambia stato:</span>
              <button v-for="(label, sid) in faultStatusLabels" :key="sid"
                      :class="['btn btn-sm', fault.statusId == sid ? 'btn-primary' : 'btn-ghost']"
                      @click="updateFaultStatus(fault, Number(sid))">{{ label }}</button>
            </div>

            <!-- Messaggi -->
            <div class="comments-section">
              <div v-if="!faultMessages[fault.id]" class="text-muted" style="font-size:0.8rem">Caricamento messaggi…</div>
              <div v-else>
                <div v-for="m in faultMessages[fault.id]" :key="m.id" class="comment-row">
                  <span class="text-secondary" style="font-size:0.78rem;font-weight:600">{{ m.authorFullName }}</span>
                  <span class="text-muted" style="font-size:0.72rem;margin-left:6px">{{ fmtDate(m.creationDate) }}</span>
                  <p style="margin:4px 0 0;white-space:pre-wrap;font-size:0.85rem">{{ m.body }}</p>
                </div>
                <div v-if="!faultMessages[fault.id].length" class="text-muted" style="font-size:0.8rem;margin-bottom:8px">Nessun messaggio.</div>
              </div>
              <div class="comment-input-row">
                <textarea v-model="newFaultMsg[fault.id]" class="form-textarea" rows="2" placeholder="Aggiungi un messaggio…" style="flex:1;resize:vertical;min-height:48px"></textarea>
                <button class="btn btn-primary btn-sm" :disabled="savingFaultMsg === fault.id || !(newFaultMsg[fault.id]?.trim())" @click="submitFaultMessage(fault.id)">
                  <span v-if="savingFaultMsg === fault.id" class="spinner" style="width:12px;height:12px"></span>
                  <span v-else>Invia</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ════════ MESSAGGI PRIVATI ════════ -->
    <div v-if="activeTab === 'messaggi'" class="tab-content">
      <div class="toolbar" style="justify-content:flex-end;margin-bottom:12px">
        <button class="btn btn-primary btn-sm" @click="showNewThreadModal=true">+ Nuova conversazione</button>
      </div>
      <div v-if="loadingThreads" class="loading-state">Caricamento…</div>
      <div v-else-if="!threads.length" class="empty-state">Nessuna conversazione.</div>
      <div v-else class="messages-layout">

        <!-- Lista conversazioni -->
        <div class="thread-list">
          <div v-for="t in threads" :key="t.id"
               :class="['thread-item', activeThread?.id === t.id ? 'active' : '']"
               @click="openThread(t)">
            <div style="display:flex;align-items:center;justify-content:space-between">
              <strong style="font-size:0.9rem">{{ t.condominioUserFullName }}</strong>
              <span v-if="t.unreadCount > 0" class="badge badge-blue" style="font-size:0.7rem">{{ t.unreadCount }}</span>
            </div>
            <div class="text-muted" style="font-size:0.75rem;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;margin-top:2px">
              {{ t.lastMessageBody ?? '—' }}
            </div>
            <div class="text-muted" style="font-size:0.7rem;margin-top:2px">{{ fmtDate(t.lastMessageDate) }}</div>
          </div>
        </div>

        <!-- Chat -->
        <div class="chat-panel">
          <div v-if="!activeThread" class="empty-state" style="height:100%;display:flex;align-items:center;justify-content:center">
            Seleziona una conversazione
          </div>
          <template v-else>
            <div class="chat-header">
              <strong>{{ activeThread.condominioUserFullName }}</strong>
            </div>
            <div v-if="loadingMessages" class="loading-state" style="flex:1">Caricamento…</div>
            <div v-else class="chat-messages">
              <div v-if="!threadMessages.length" class="empty-state">Nessun messaggio.</div>
              <div v-for="m in threadMessages" :key="m.id" class="chat-bubble-row">
                <div :class="['chat-bubble', m.senderUserId === activeThread.condominioUserId ? 'bubble-left' : 'bubble-right']">
                  <div class="bubble-meta">{{ m.senderFullName }} · {{ fmtDate(m.creationDate) }}</div>
                  <div class="bubble-body">{{ m.body }}</div>
                </div>
              </div>
            </div>
            <div class="chat-input-row">
              <textarea v-model="newMsgBody" class="form-textarea" rows="2" placeholder="Scrivi un messaggio…"
                        style="flex:1;resize:none" @keydown.enter.ctrl="sendMessage"></textarea>
              <button class="btn btn-primary" :disabled="sendingMsg || !newMsgBody.trim()" @click="sendMessage">
                <span v-if="sendingMsg" class="spinner" style="width:14px;height:14px"></span>
                <span v-else>Invia</span>
              </button>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>

  <!-- Modal: Nuovo/Modifica post bacheca -->
  <div class="modal-overlay" v-if="showPostModal" @mousedown.self="showPostModal=false">
    <div class="modal" style="max-width:600px;width:95vw">
      <div class="modal-header">
        <h2>{{ editingPost ? 'Modifica post' : 'Nuovo post' }}</h2>
        <button class="btn-icon" @click="showPostModal=false">✕</button>
      </div>
      <div class="modal-body form-grid">
        <div class="form-group form-group--full" :class="{ 'has-error': postErrors.title }">
          <label class="form-label">Titolo *</label>
          <input class="form-input" v-model="postForm.title" @input="delete postErrors.title" />
          <span v-if="postErrors.title" class="field-error">{{ postErrors.title }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': postErrors.body }">
          <label class="form-label">Testo *</label>
          <textarea class="form-textarea" v-model="postForm.body" rows="5" @input="delete postErrors.body"></textarea>
          <span v-if="postErrors.body" class="field-error">{{ postErrors.body }}</span>
        </div>
        <div class="form-group form-group--full">
          <label style="display:flex;align-items:center;gap:8px;cursor:pointer">
            <input type="checkbox" v-model="postForm.isPinned" />
            Fissa in cima alla bacheca
          </label>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="showPostModal=false">Annulla</button>
        <button class="btn btn-primary" @click="savePost" :disabled="savingPost">
          <span v-if="savingPost" class="spinner" style="width:14px;height:14px"></span>
          {{ editingPost ? 'Salva' : 'Pubblica' }}
        </button>
      </div>
    </div>
  </div>

  <!-- Modal: Nuova segnalazione guasto -->
  <div class="modal-overlay" v-if="showFaultModal" @mousedown.self="showFaultModal=false">
    <div class="modal" style="max-width:600px;width:95vw">
      <div class="modal-header">
        <h2>Nuova segnalazione</h2>
        <button class="btn-icon" @click="showFaultModal=false">✕</button>
      </div>
      <div class="modal-body form-grid">
        <div class="form-group form-group--full" :class="{ 'has-error': faultErrors.title }">
          <label class="form-label">Titolo *</label>
          <input class="form-input" v-model="faultForm.title" @input="delete faultErrors.title" placeholder="Es. Ascensore fuori servizio" />
          <span v-if="faultErrors.title" class="field-error">{{ faultErrors.title }}</span>
        </div>
        <div class="form-group form-group--full" :class="{ 'has-error': faultErrors.description }">
          <label class="form-label">Descrizione *</label>
          <textarea class="form-textarea" v-model="faultForm.description" rows="4" @input="delete faultErrors.description"
                    placeholder="Descrivi il problema nel dettaglio…"></textarea>
          <span v-if="faultErrors.description" class="field-error">{{ faultErrors.description }}</span>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="showFaultModal=false">Annulla</button>
        <button class="btn btn-primary" @click="saveFault" :disabled="savingFault">
          <span v-if="savingFault" class="spinner" style="width:14px;height:14px"></span>
          Invia segnalazione
        </button>
      </div>
    </div>
  </div>
  <!-- Modal: Nuova conversazione privata -->
  <div class="modal-overlay" v-if="showNewThreadModal" @mousedown.self="showNewThreadModal=false">
    <div class="modal" style="max-width:480px;width:95vw">
      <div class="modal-header">
        <h2>Nuova conversazione</h2>
        <button class="btn-icon" @click="showNewThreadModal=false">✕</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label class="form-label">Cerca condomino</label>
          <div style="display:flex;gap:8px">
            <input class="form-input" v-model="newThreadSearch" placeholder="Nome, cognome o email…"
                   @keydown.enter="searchCondomini" style="flex:1" />
            <button class="btn btn-ghost btn-sm" @click="searchCondomini" :disabled="searchingUsers">
              <span v-if="searchingUsers" class="spinner" style="width:12px;height:12px"></span>
              <span v-else>Cerca</span>
            </button>
          </div>
        </div>
        <div v-if="newThreadResults.length" class="user-results">
          <div v-for="u in newThreadResults" :key="u.id" class="user-result-row" @click="startThread(u)">
            <div style="font-size:0.9rem;font-weight:600">{{ u.name }} {{ u.lastName }}</div>
            <div class="text-muted" style="font-size:0.78rem">{{ u.email }}</div>
          </div>
        </div>
        <div v-else-if="!searchingUsers && newThreadSearch.trim()" class="text-muted" style="font-size:0.85rem;margin-top:8px">
          Nessun risultato.
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="showNewThreadModal=false">Annulla</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-header { padding: 16px 20px 0; }
.card-title  { font-size: 1.15rem; font-weight: 700; }

/* Tabs */
.tab-bar {
  display: flex;
  gap: 2px;
  padding: 12px 20px 0;
  border-bottom: 1px solid var(--border);
}
.tab-btn {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: 8px 16px;
  cursor: pointer;
  font-size: 0.88rem;
  color: var(--text-secondary);
  font-weight: 500;
  transition: color 0.15s, border-color 0.15s;
}
.tab-btn.active { color: var(--accent); border-bottom-color: var(--accent); }
.tab-content { padding: 16px 20px 20px; }

/* Post / Fault cards */
.posts-list { display: flex; flex-direction: column; gap: 8px; }
.post-card {
  border: 1px solid var(--border);
  border-radius: 8px;
  overflow: hidden;
  background: var(--bg-surface);
}
.post-card.pinned { border-color: var(--accent); }
.post-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 14px;
  background: var(--bg-base);
}
.post-title { font-size: 0.9rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.post-body { padding: 12px 14px; border-top: 1px solid var(--border); font-size: 0.88rem; }

/* Comments */
.comments-section { background: var(--bg-base); border-radius: 6px; padding: 10px; }
.comment-row { padding: 6px 0; border-bottom: 1px solid var(--border); }
.comment-row:last-of-type { border-bottom: none; }
.comment-input-row { display: flex; gap: 8px; margin-top: 10px; align-items: flex-end; }

/* Status badges extra */
.badge-red    { background: rgba(239,68,68,.12);  color: var(--accent-red); }
.badge-yellow { background: rgba(234,179,8,.12);  color: #ca8a04; }
.badge-blue   { background: rgba(99,102,241,.12); color: var(--accent); }

/* Messaggi privati layout */
.messages-layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 0;
  border: 1px solid var(--border);
  border-radius: 8px;
  overflow: hidden;
  height: 520px;
}
.thread-list { border-right: 1px solid var(--border); overflow-y: auto; }
.thread-item {
  padding: 12px 14px;
  cursor: pointer;
  border-bottom: 1px solid var(--border);
  transition: background 0.12s;
}
.thread-item:hover, .thread-item.active { background: var(--accent-glow); }

.chat-panel { display: flex; flex-direction: column; overflow: hidden; }
.chat-header { padding: 10px 14px; border-bottom: 1px solid var(--border); font-size: 0.9rem; background: var(--bg-base); }
.chat-messages { flex: 1; overflow-y: auto; padding: 12px 14px; display: flex; flex-direction: column; gap: 8px; }
.chat-bubble-row { display: flex; }
.bubble-left  { margin-right: auto; background: var(--bg-base); }
.bubble-right { margin-left: auto; background: var(--accent-glow); }
.chat-bubble  { max-width: 75%; border: 1px solid var(--border); border-radius: 10px; padding: 8px 12px; }
.bubble-meta  { font-size: 0.7rem; color: var(--text-muted); margin-bottom: 4px; }
.bubble-body  { font-size: 0.88rem; white-space: pre-wrap; }
.chat-input-row { display: flex; gap: 8px; padding: 10px 14px; border-top: 1px solid var(--border); align-items: flex-end; }

/* Ricerca condomino */
.user-results { margin-top: 10px; border: 1px solid var(--border); border-radius: 6px; overflow: hidden; }
.user-result-row {
  padding: 10px 12px;
  cursor: pointer;
  border-bottom: 1px solid var(--border);
  transition: background 0.12s;
}
.user-result-row:last-child { border-bottom: none; }
.user-result-row:hover { background: var(--accent-glow); }
</style>
