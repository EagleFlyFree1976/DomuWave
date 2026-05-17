<template>
  <div>
    <div class="page-header">
      <h1>Comunicazioni</h1>
      <button v-if="canCreate && store.selectedCondominioId" class="btn btn-primary" @click="openModal()">+ Nuova comunicazione</button>
    </div>

    <div class="toolbar">
      <div class="tab-toggle">
        <button class="tab-btn" :class="{ active: !showArchived }" @click="switchTab(false)">Attive</button>
        <button v-if="canEdit" class="tab-btn" :class="{ active: showArchived }" @click="switchTab(true)">Archiviate</button>
      </div>
      <input class="form-input search-input" v-model="search" placeholder="Cerca per titolo…" />
      <select class="form-select" v-model="filterType" style="width:160px">
        <option value="">Tutti i tipi</option>
        <option value="Notice">Avviso</option>
        <option value="Meeting">Assemblea</option>
        <option value="Maintenance">Manutenzione</option>
        <option value="Emergency">Urgente</option>
        <option value="Info">Informazione</option>
        <option value="FeeNotice">Avviso di pagamento</option>
      </select>
      <select class="form-select" v-model="filterPriority" style="width:140px">
        <option value="">Tutte le priorità</option>
        <option value="High">Alta</option>
        <option value="Normal">Normale</option>
        <option value="Low">Bassa</option>
      </select>
    </div>

    <!-- Cards layout -->
    <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
    <div v-else-if="!filtered.length" class="empty-state">
      <div class="empty-icon">◉</div><div>Nessuna comunicazione trovata</div>
    </div>
    <div v-else class="comms-list">
      <div v-for="c in filtered" :key="c.id" class="comm-card" :class="{ 'comm-urgent': c.priority === 'High' }">
        <div class="comm-left">
          <div class="comm-priority-bar" :class="priorityBar(c.priority)"></div>
        </div>
        <div class="comm-body">
          <div class="comm-header">
            <span class="comm-title">{{ c.title }}</span>
            <div class="comm-badges">
              <span class="badge" :class="typeBadge(c.communicationType)">{{ typeLabel(c.communicationType) }}</span>
              <span class="badge" :class="priorityBadge(c.priority)">{{ c.priority }}</span>
              <span class="badge badge-muted" v-if="!c.isVisible">Nascosta</span>
              <span class="badge badge-amber" v-if="c.isArchived">Archiviata</span>
            </div>
          </div>
          <div class="comm-preview">{{ preview(c.content) }}</div>
          <div class="comm-meta">
            <span class="text-muted mono" style="font-size:0.75rem">{{ fmtDate(c.publicationDate) }}</span>
            <span v-if="c.expirationDate" class="text-muted" style="font-size:0.75rem">
              Scade: {{ fmtDate(c.expirationDate) }}
            </span>
            <span v-if="c.sendEmail" class="badge badge-blue" style="font-size:0.7rem">📧 Email inviata</span>
            <router-link
              v-if="c.assemblyId"
              class="comm-assembly-link"
              :to="`/condomini/${store.selectedCondominioId}/assemblee/${c.assemblyId}`"
            >
              <i class="pi pi-microphone"></i> {{ c.assemblyTitle || 'Vai all\'assemblea' }}
            </router-link>
          </div>
        </div>
        <div class="comm-actions">
          <template v-if="!c.isArchived">
            <button v-if="canEdit && !c.isVisible" class="btn btn-sm btn-ghost" @click="publishComm(c.id)">Pubblica</button>
            <button v-if="canEdit" class="btn btn-sm btn-ghost" @click="toggleNotifPanel(c.id)" :class="{ 'btn-active': notifPanelId === c.id }">
              🔔 Notifiche
            </button>
            <button v-if="canEdit" class="btn btn-sm btn-ghost btn-send-email" @click="sendEmail(c.id)" :disabled="sendingEmail === c.id" title="Invia email">
              <span v-if="sendingEmail === c.id" class="spinner" style="width:12px;height:12px"></span>
              <span v-else>📧</span>
              Invia email
            </button>
            <button v-if="canEdit" class="btn-icon" @click="openModal(c)">✎</button>
            <button v-if="canEdit" class="btn-icon" title="Archivia" @click="archiveItem(c.id)">🗂</button>
            <button v-if="canDelete && !c.hasSentNotifications" class="btn-icon" @click="deleteItem(c.id)" style="color:var(--accent-red)">✕</button>
          </template>
          <template v-else>
            <button v-if="canEdit" class="btn btn-sm btn-ghost" @click="toggleNotifPanel(c.id)" :class="{ 'btn-active': notifPanelId === c.id }">
              🔔 Notifiche
            </button>
            <button v-if="canEdit" class="btn btn-sm btn-ghost" @click="unarchiveItem(c.id)">↩ Ripristina</button>
          </template>
        </div>

        <!-- Pannello notifiche inline -->
        <div v-if="notifPanelId === c.id" class="notif-panel">
          <NotifPanel :communication="c" :condominiumId="store.selectedCondominioId" :readonly="c.isArchived" @close="notifPanelId = null" />
        </div>
      </div>
    </div>

    <!-- Modal wizard -->
    <div class="modal-overlay" v-if="showModal" @mousedown.self="showModal=false">
      <div class="modal modal-wide">
        <div class="modal-header">
          <div style="display:flex;flex-direction:column;gap:0.25rem">
            <h2>{{ editing ? 'Modifica' : 'Nuova' }} comunicazione</h2>
            <div class="wizard-steps">
              <span class="wizard-step" :class="{ active: wizardStep === 1, done: wizardStep > 1 }">1. Destinatari</span>
              <span class="wizard-sep">›</span>
              <span class="wizard-step" :class="{ active: wizardStep === 2 }">2. Messaggio</span>
            </div>
          </div>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>

        <!-- Step 1: destinatari -->
        <div v-if="wizardStep === 1" class="modal-body">
          <div class="form-group">
            <label class="form-label">Destinatari</label>
            <div class="recipient-options">
              <label class="recipient-option" :class="{ selected: recipientMode === 'all' }">
                <input type="radio" v-model="recipientMode" value="all" />
                <span>Tutti i condòmini</span>
              </label>
              <label class="recipient-option" :class="{ selected: recipientMode === 'select' }">
                <input type="radio" v-model="recipientMode" value="select" />
                <span>Seleziona unità specifiche</span>
              </label>
            </div>
          </div>

          <div v-if="recipientMode === 'select'" class="form-group">
            <label class="form-label">Unità immobiliari</label>
            <div v-if="loadingUnits" style="font-size:0.85rem;color:var(--text-muted)">Caricamento unità…</div>
            <div v-else-if="!availableUnits.length" style="font-size:0.85rem;color:var(--text-muted)">Nessuna unità disponibile</div>
            <div v-else class="unit-checklist-scroll">
              <div v-for="group in unitGroups" :key="group.label" class="unit-group">
                <div class="unit-group-header">
                  <label class="unit-group-label" :class="{ 'is-billing': group.isBillingGroup, 'is-ungrouped': !group.isBillingGroup }">
                    <input type="checkbox" :checked="isGroupChecked(group)" :indeterminate.prop="isGroupIndeterminate(group)" @change="toggleGroup(group)" />
                    <span>{{ group.label }}</span>
                    <span v-if="group.isBillingGroup" style="font-weight:400;text-transform:none;letter-spacing:0;font-style:normal;color:var(--text-muted)">({{ group.units.length }} unità)</span>
                  </label>
                </div>
                <div class="unit-group-items">
                  <label v-for="u in group.units" :key="u.id" class="unit-check-item">
                    <input type="checkbox" :value="u.id" v-model="selectedUnitIds" />
                    <div class="unit-check-info">
                      <span class="unit-code">{{ u.internalNumber }}</span>
                      <span class="unit-owners text-muted">{{ ownerNames(u) }}</span>
                    </div>
                  </label>
                </div>
              </div>
            </div>
            <div v-if="recipientMode === 'select' && !selectedUnitIds.length" class="field-error" style="margin-top:0.4rem">Seleziona almeno un'unità</div>
          </div>

          <div class="form-group">
            <label class="form-label">Metodo di consegna</label>
            <div class="recipient-options">
              <label class="recipient-option" :class="{ selected: deliveryMethod === 0 }">
                <input type="radio" v-model="deliveryMethod" :value="0" />
                <span>📧 Email</span>
              </label>
              <label class="recipient-option" :class="{ selected: deliveryMethod === 1 }">
                <input type="radio" v-model="deliveryMethod" :value="1" />
                <span>✉ Raccomandata</span>
              </label>
            </div>
          </div>
        </div>

        <!-- Step 2: messaggio -->
        <div v-if="wizardStep === 2" class="modal-body">
          <div class="form-group">
            <label class="form-label">Titolo *</label>
            <input class="form-input" v-model="form.title" />
          </div>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Tipo</label>
              <select class="form-select" v-model="form.communicationType">
                <option value="Notice">Avviso</option>
                <option value="Meeting">Assemblea</option>
                <option value="Maintenance">Manutenzione</option>
                <option value="Emergency">Urgente</option>
                <option value="Info">Informazione</option>
                <option value="FeeNotice">Avviso di pagamento</option>
              </select>
            </div>
            <div v-if="form.communicationType === 'Meeting'" class="form-group form-group--full">
              <label class="form-label">Assemblea collegata *</label>
              <select class="form-select" v-model.number="form.assemblyId">
                <option :value="null">— Seleziona assemblea —</option>
                <option v-for="a in assembliesBozza" :key="a.id" :value="a.id">{{ a.title }} ({{ fmtDate(a.scheduledDate) }})</option>
              </select>
              <span v-if="!assembliesBozza.length" class="field-error">Nessuna assemblea in stato Bozza disponibile</span>
            </div>
            <div class="form-group">
              <label class="form-label">Priorità</label>
              <select class="form-select" v-model="form.priority">
                <option value="High">Alta</option>
                <option value="Normal">Normale</option>
                <option value="Low">Bassa</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Data pubblicazione</label>
              <input class="form-input" type="datetime-local" v-model="form.publicationDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Data scadenza</label>
              <input class="form-input" type="datetime-local" v-model="form.expirationDate" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Contenuto *</label>
            <textarea class="form-textarea" v-model="form.content" rows="5"></textarea>
          </div>
          <div style="display:flex;gap:1.5rem">
            <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;cursor:pointer">
              <input type="checkbox" v-model="form.isVisible" />
              Visibile ai condòmini
            </label>
            <label style="display:flex;align-items:center;gap:0.4rem;font-size:0.875rem;cursor:pointer">
              <input type="checkbox" v-model="form.sendEmail" />
              Invia email notifica
            </label>
          </div>

          <!-- Allegati comunicazione (solo in modifica, se esistono bozze) -->
          <div v-if="editing && commDraftNotifIds.length" style="border-top:1px solid var(--border);margin-top:0.75rem;padding-top:0.75rem">
            <button class="attach-toggle-btn" @click="showAttachSection = !showAttachSection">
              <span>{{ showAttachSection ? '▲' : '▼' }}</span>
              📎 Allegati comunicazione
            </button>
            <div v-if="showAttachSection" style="margin-top:0.75rem">
              <div class="text-muted" style="font-size:0.8rem;margin-bottom:0.5rem">
                Gli allegati vengono applicati a tutte le notifiche in bozza ({{ commDraftNotifIds.length }}).
              </div>
              <AttachPanel
                :notification-ids="commDraftNotifIds"
                :condominium-id="store.selectedCondominioId"
              />
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
          <template v-if="wizardStep === 1">
            <button class="btn btn-primary" @click="goToStep2">Avanti →</button>
          </template>
          <template v-else>
            <button class="btn btn-ghost" @click="wizardStep = 1">← Indietro</button>
            <button class="btn btn-primary" @click="save" :disabled="saving">
              <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
              {{ editing ? 'Salva' : 'Crea e genera notifiche' }}
            </button>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { communicationApi, communicationNotificationApi, assemblyApi, unitApi, billingGroupApi } from '@/services/api'
import NotifPanel from '@/components/NotifPanel.vue'
import AttachPanel from '@/components/AttachPanel.vue'

const store = useAppStore()

const perms = ref({ canView: false, canCreate: false, canModify: false, canDelete: false, canAction: false })
const canCreate = computed(() => perms.value.canCreate)
const canEdit   = computed(() => perms.value.canModify)
const canDelete = computed(() => perms.value.canDelete)

async function loadPermissions() {
  try {
    const { data } = await communicationApi.getPermissions()
    perms.value = data
  } catch { /* permessi negati — tutto disabilitato */ }
}
const notifPanelId  = ref(null)
const sendingEmail  = ref(null)
const showArchived  = ref(false)

function toggleNotifPanel(id) {
  notifPanelId.value = notifPanelId.value === id ? null : id
}

async function sendEmail(id) {
  if (!confirm('Inviare le email per questa comunicazione?')) return
  sendingEmail.value = id
  try {
    const { data } = await communicationNotificationApi.sendEmail(id)
    store.toast(`Email inviate: ${data.sent}, fallite: ${data.failed}`, data.failed ? 'error' : 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    sendingEmail.value = null
  }
}
const communications = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterType = ref('')
const filterPriority = ref('')

const assembliesBozza    = ref([])
const wizardStep         = ref(1)
const recipientMode      = ref('all')   // 'all' | 'select'
const deliveryMethod     = ref(0)       // 0=Email 1=Raccomandata
const selectedUnitIds    = ref([])
const availableUnits     = ref([])
const billingGroups      = ref([])
const loadingUnits       = ref(false)
const commDraftNotifIds  = ref([])    // draft notification IDs for comm-level attach panel
const showAttachSection  = ref(false)

async function loadAssemblies() {
  if (!store.selectedCondominioId) return
  try {
    const { data } = await assemblyApi.getByCondominium(store.selectedCondominioId)
    assembliesBozza.value = (data ?? []).filter(a => a.statusId === 1)
  } catch { assembliesBozza.value = [] }
}

async function loadUnits() {
  if (!store.selectedCondominioId) return
  loadingUnits.value = true
  try {
    const [unitsRes, groupsRes] = await Promise.all([
      unitApi.getPanoramica(store.selectedCondominioId),
      billingGroupApi.getByCondominium(store.selectedCondominioId),
    ])
    availableUnits.value = unitsRes.data ?? []
    billingGroups.value  = groupsRes.data ?? []
  } catch { availableUnits.value = []; billingGroups.value = [] }
  finally { loadingUnits.value = false }
}

const unitGroups = computed(() => {
  const allUnits = availableUnits.value
  const groups   = billingGroups.value

  // Unità già assegnate a un gruppo di fatturazione
  const assignedIds = new Set(groups.flatMap(g => g.units?.map(u => u.unitId) ?? []))

  // Gruppi di fatturazione con le unità complete (panoramica)
  const billingEntries = groups.map(g => ({
    label:   g.name,
    isBillingGroup: true,
    units:   (g.units ?? [])
      .map(gu => allUnits.find(u => u.id === gu.unitId))
      .filter(Boolean),
  })).filter(g => g.units.length > 0)

  // Unità senza gruppo
  const unassigned = allUnits.filter(u => !assignedIds.has(u.id))
  const ungroupedEntry = unassigned.length
    ? [{ label: 'Senza gruppo', isBillingGroup: false, units: unassigned }]
    : []

  return [...billingEntries, ...ungroupedEntry]
})

function ownerNames(unit) {
  if (!unit.owners?.length) return '—'
  return unit.owners
    .filter(o => o.isActive)
    .map(o => `${o.firstName ?? ''} ${o.lastName ?? ''}`.trim())
    .filter(Boolean)
    .join(', ') || '—'
}

function isGroupChecked(group) {
  return group.units.every(u => selectedUnitIds.value.includes(u.id))
}
function isGroupIndeterminate(group) {
  const some = group.units.some(u => selectedUnitIds.value.includes(u.id))
  return some && !isGroupChecked(group)
}
function toggleGroup(group) {
  if (isGroupChecked(group)) {
    selectedUnitIds.value = selectedUnitIds.value.filter(id => !group.units.some(u => u.id === id))
  } else {
    const toAdd = group.units.map(u => u.id).filter(id => !selectedUnitIds.value.includes(id))
    selectedUnitIds.value = [...selectedUnitIds.value, ...toAdd]
  }
}

function goToStep2() {
  if (recipientMode.value === 'select' && !selectedUnitIds.value.length) return
  wizardStep.value = 2
}

const defaultForm = () => ({
  title: '', content: '', communicationType: 'Notice', priority: 'Normal',
  publicationDate: new Date().toISOString().slice(0,16),
  expirationDate: '', sendEmail: false, isVisible: true, attachmentPath: '',
  assemblyId: null,
})
const form = ref(defaultForm())

const fmtDate = (d) => d ? new Date(d).toLocaleDateString('it-IT', { day:'2-digit', month:'short', year:'numeric' }) : '—'
const preview = (t) => t?.length > 120 ? t.slice(0, 120) + '…' : t

const commTypes = { Notice: 'Avviso', Meeting: 'Assemblea', Maintenance: 'Manutenzione', Emergency: 'Urgente', Info: 'Informazione', FeeNotice: 'Avviso di pagamento' }
const typeLabel = (t) => commTypes[t] ?? t
const typeBadge = (t) => ({ Notice: 'badge-blue', Meeting: 'badge-purple', Maintenance: 'badge-amber', Emergency: 'badge-red', Info: 'badge-muted', FeeNotice: 'badge-green' }[t] || 'badge-muted')
const priorityBadge = (p) => ({ High: 'badge-red', Normal: 'badge-blue', Low: 'badge-muted' }[p] || 'badge-muted')
const priorityBar = (p) => ({ High: 'bar-red', Normal: 'bar-blue', Low: 'bar-muted' }[p] || 'bar-muted')

const filtered = computed(() => {
  let list = communications.value
  if (filterType.value) list = list.filter(c => c.communicationType === filterType.value)
  if (filterPriority.value) list = list.filter(c => c.priority === filterPriority.value)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(c => c.title?.toLowerCase().includes(q))
  }
  return list
})

async function loadData() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = showArchived.value
      ? await communicationApi.getArchived(store.selectedCondominioId)
      : await communicationApi.getByCondominium(store.selectedCondominioId)
    communications.value = data
  } catch { communications.value = [] } finally { loading.value = false }
}

function switchTab(archived) {
  if (showArchived.value === archived) return
  showArchived.value = archived
  notifPanelId.value = null
  loadData()
}

async function openModal(c = null) {
  editing.value = c?.id ?? null
  form.value = c
    ? { ...c, publicationDate: c.publicationDate?.slice(0,16), expirationDate: c.expirationDate?.slice(0,16), assemblyId: c.assemblyId ?? null }
    : defaultForm()
  wizardStep.value       = 1
  recipientMode.value    = 'all'
  deliveryMethod.value   = 0
  selectedUnitIds.value  = []
  commDraftNotifIds.value = []
  showAttachSection.value = false
  await loadUnits()
  if (c) {
    // Pre-populate recipients + collect draft notification IDs for attach panel
    try {
      const { data: notifs } = await communicationNotificationApi.getByCommunication(c.id)
      const drafts = (notifs ?? []).filter(n => n.status === 0)
      commDraftNotifIds.value = drafts.map(n => n.id)
      if (drafts.length) {
        deliveryMethod.value = drafts[0].deliveryMethod ?? 0
        const unitIds = drafts.map(n => n.unitId).filter(id => id != null)
        if (unitIds.length < availableUnits.value.length) {
          recipientMode.value   = 'select'
          selectedUnitIds.value = unitIds
        }
      }
    } catch { /* lascia i default */ }
  }
  if (form.value.communicationType === 'Meeting') loadAssemblies()
  showModal.value = true
}

watch(() => form.value.communicationType, (val) => {
  if (val === 'Meeting') loadAssemblies()
  else form.value.assemblyId = null
})

async function save() {
  if (!form.value.title || !form.value.content) return store.toast('Titolo e contenuto sono obbligatori', 'error')
  if (form.value.communicationType === 'Meeting' && !form.value.assemblyId) return store.toast("Seleziona l'assemblea collegata", 'error')
  saving.value = true
  try {
    const payload = {
      ...form.value,
      condominiumId:  store.selectedCondominioId,
      expirationDate: form.value.expirationDate || null,
    }
    const generatePayload = {
      deliveryMethod: deliveryMethod.value,
      unitIds: recipientMode.value === 'select' ? selectedUnitIds.value : null,
    }
    if (editing.value) {
      await communicationApi.update(editing.value, payload)
      await communicationNotificationApi.generate(editing.value, generatePayload)
      store.toast('Comunicazione aggiornata e notifiche rigenerate', 'success')
    } else {
      const { data: created } = await communicationApi.create(payload)
      await communicationNotificationApi.generate(created.id, generatePayload)
      store.toast('Comunicazione creata e notifiche generate', 'success')
    }
    showModal.value = false
    loadData()
  } catch { store.toast('Errore durante il salvataggio', 'error') } finally { saving.value = false }
}

async function publishComm(id) {
  try { await communicationApi.publish(id); store.toast('Comunicazione pubblicata', 'success'); loadData() }
  catch { store.toast('Errore', 'error') }
}

async function archiveItem(id) {
  if (!confirm('Archiviare questa comunicazione?')) return
  try { await communicationApi.archive(id); store.toast('Comunicazione archiviata', 'success'); loadData() }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

async function unarchiveItem(id) {
  try { await communicationApi.unarchive(id); store.toast('Comunicazione ripristinata', 'success'); loadData() }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa comunicazione?')) return
  try { await communicationApi.delete(id); store.toast('Comunicazione eliminata', 'success'); loadData() }
  catch (err) { if (!err?.response) store.toast('Impossibile raggiungere il server', 'error') }
}

watch(() => store.selectedCondominioId, loadData)
onMounted(() => { loadPermissions(); loadData() })
onUnmounted(() => window.removeEventListener('app:refresh', loadData))
window.addEventListener('app:refresh', loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; align-items: center; }

.tab-toggle { display: flex; border: 1px solid var(--border); border-radius: 6px; overflow: hidden; flex-shrink: 0; }
.tab-btn { padding: 5px 14px; font-size: 0.82rem; background: transparent; border: none; cursor: pointer; color: var(--text-muted); transition: background 0.15s, color 0.15s; }
.tab-btn.active { background: var(--accent); color: #fff; }
.tab-btn:not(.active):hover { background: var(--bg-base); color: var(--text); }
.search-input { flex: 1; min-width: 200px; max-width: 320px; }

.comms-list { display: flex; flex-direction: column; gap: 0.5rem; }
.comm-card {
  display: flex; align-items: flex-start; gap: 0;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  transition: border-color 0.15s;
}
.comm-card:hover { border-color: rgba(255,255,255,0.12); }
.comm-urgent { border-color: rgba(252,129,129,0.2); }

.comm-left { flex-shrink: 0; width: 4px; }
.comm-priority-bar { width: 100%; height: 100%; min-height: 60px; }
.bar-red   { background: var(--accent-red); }
.bar-blue  { background: var(--accent); }
.bar-muted { background: var(--text-muted); }

.comm-body { flex: 1; min-width: 0; padding: 0.9rem 1rem; display: flex; flex-direction: column; gap: 0.4rem; }
.comm-header { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.comm-title { font-weight: 500; font-size: 0.95rem; }
.comm-badges { display: flex; gap: 0.35rem; flex-wrap: wrap; }
.comm-preview { font-size: 0.83rem; color: var(--text-secondary); line-height: 1.5; }
.comm-meta { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.comm-assembly-link { font-size: 0.75rem; color: var(--accent); text-decoration: none; display: flex; align-items: center; gap: .25rem; }
.comm-assembly-link:hover { text-decoration: underline; }

.comm-actions { display: flex; align-items: center; gap: 0.4rem; padding: 0.9rem 0.75rem; flex-shrink: 0; }
.btn-active { color: var(--accent) !important; border-color: var(--accent) !important; }

.notif-panel {
  border-top: 1px solid var(--border);
  padding: 16px;
  background: var(--bg-base);
}

/* Wizard */
.modal-wide { width: min(640px, 96vw); }
.wizard-steps { display: flex; align-items: center; gap: 0.4rem; font-size: 0.78rem; }
.wizard-step { color: var(--text-muted); }
.wizard-step.active { color: var(--accent); font-weight: 600; }
.wizard-step.done { color: var(--accent-green); }
.wizard-sep { color: var(--text-muted); }

/* Selezione destinatari */
.recipient-options { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.recipient-option {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: 0.875rem;
  transition: border-color 0.15s, background 0.15s;
}
.recipient-option input { display: none; }
.recipient-option:hover { border-color: var(--accent); }
.recipient-option.selected { border-color: var(--accent); background: var(--accent-glow); color: var(--accent); font-weight: 500; }

.attach-toggle-btn {
  display: flex; align-items: center; gap: 0.5rem;
  font-size: 0.875rem; color: var(--accent);
  background: none; border: none; cursor: pointer; padding: 0;
}
.attach-toggle-btn:hover { text-decoration: underline; }

/* Lista unità raggruppata */
.unit-checklist-scroll {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  max-height: 260px;
  overflow-y: auto;
}
.unit-group { border-bottom: 1px solid var(--border); }
.unit-group:last-child { border-bottom: none; }
.unit-group-header {
  padding: 0.4rem 0.75rem;
  background: var(--bg-base);
  position: sticky; top: 0; z-index: 1;
}
.unit-group-label {
  display: flex; align-items: center; gap: 0.5rem;
  font-size: 0.78rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.04em; color: var(--text-muted); cursor: pointer;
}
.unit-group-label.is-billing { color: var(--accent); }
.unit-group-label.is-ungrouped { color: var(--text-muted); font-style: italic; }
.unit-group-items { padding: 0.25rem 0; }
.unit-check-item {
  display: flex; align-items: flex-start; gap: 0.6rem;
  font-size: 0.85rem; cursor: pointer;
  padding: 0.35rem 0.75rem 0.35rem 1.5rem;
  transition: background 0.1s;
}
.unit-check-item:hover { background: var(--bg-base); }
.unit-check-info { display: flex; flex-direction: column; gap: 0.1rem; }
.unit-code { font-weight: 500; }
.unit-owners { font-size: 0.78rem; }
</style>
