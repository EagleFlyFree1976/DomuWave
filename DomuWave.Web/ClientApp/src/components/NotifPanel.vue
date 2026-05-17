<template>
  <div class="notif-panel-inner">

    <!-- Toolbar azioni -->
    <div class="notif-toolbar">
      <div class="notif-toolbar-left">
        <span class="text-secondary" style="font-size:0.85rem">
          {{ notifications.length }} notifiche
          <span v-if="pendingCount > 0" style="color:var(--accent)">· {{ pendingCount }} da inviare</span>
        </span>
      </div>
      <div v-if="!readonly" class="notif-toolbar-right">
        <!-- Genera -->
        <div class="notif-generate-row">
          <select class="form-select form-select-sm" v-model="genMethod">
            <option :value="0">Email</option>
            <option :value="1">Raccomandata</option>
          </select>
          <button class="btn btn-sm btn-ghost" @click="isFeeNotice ? openFeeGenModal() : generate()" :disabled="generating">
            <span v-if="generating" class="spinner" style="width:11px;height:11px"></span>
            <span v-else>↺ Genera</span>
          </button>
        </div>
        <!-- Rigenera testi -->
        <div v-if="hasPendingNotifications" class="notif-generate-row">
          <select class="form-select form-select-sm" v-model="regenTemplateId" style="min-width:140px">
            <option :value="null">— template automatico —</option>
            <option v-for="t in availableTemplates" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <button class="btn btn-sm btn-ghost" @click="regenerateTexts" :disabled="regenerating">
            <span v-if="regenerating" class="spinner" style="width:11px;height:11px"></span>
            <span v-else>✎ Rigenera testi</span>
          </button>
        </div>
        <!-- Invia tutto — comportamento basato su genMethod -->
        <button v-if="hasPendingForMethod" class="btn btn-sm btn-primary"
                @click="genMethod === 0 ? sendAll() : downloadBatchPdf()"
                :disabled="sending || sendingId !== null || downloadingPdf">
          <span v-if="sending || downloadingPdf" class="spinner" style="width:11px;height:11px"></span>
          <span v-else>{{ genMethod === 0 ? '📧' : '🖨' }} Invia tutto</span>
        </button>
      </div>
    </div>

    <!-- Risultato invio -->
    <div v-if="sendResult" class="send-result" :class="sendResult.failed > 0 ? 'result-warn' : 'result-ok'">
      Inviate {{ sendResult.sent }}<span v-if="sendResult.failed > 0"> · {{ sendResult.failed }} errori</span>
      <span v-if="sendResult.errors?.length"> — {{ sendResult.errors.join('; ') }}</span>
    </div>

    <!-- Filtri -->
    <div v-if="notifications.length" class="notif-filters">
      <input class="form-input form-input-sm" v-model="filterSearch" placeholder="🔍 Cerca destinatario…" style="flex:1;min-width:140px" />
      <select class="form-select form-select-sm" v-model="filterMethod" style="min-width:130px">
        <option value="">Tutti i metodi</option>
        <option value="0">Email</option>
        <option value="1">Raccomandata</option>
      </select>
      <select class="form-select form-select-sm" v-model="filterStatus" style="min-width:130px">
        <option value="">Tutti gli stati</option>
        <option value="0">Bozza</option>
        <option value="1">Pianificata</option>
        <option value="2">Inviata</option>
        <option value="3">Consegnata</option>
        <option value="4">Fallita</option>
        <option value="5">Stampata</option>
      </select>
      <button v-if="filterSearch || filterMethod !== '' || filterStatus !== ''" class="btn btn-sm btn-ghost" @click="clearFilters" title="Rimuovi filtri">✕</button>
    </div>

    <!-- Tabella notifiche -->
    <div v-if="loading" class="loading-state" style="padding:8px">Caricamento…</div>
    <div v-else-if="!notifications.length" class="empty-state" style="padding:12px 0">
      Nessuna notifica generata. Clicca "Genera" per creare le notifiche per tutti i condomini.
    </div>
    <div v-else-if="!filteredGroups.length" class="empty-state" style="padding:12px 0">
      Nessun risultato per i filtri selezionati.
    </div>
    <div v-else class="table-wrap" style="margin-top:4px">
      <table>
        <thead>
          <tr>
            <th>Destinatario</th>
            <th>Unità</th>
            <th>Metodo</th>
            <th>Stato</th>
            <th>Inviata</th>
            <th>Consegnata</th>
            <th>N° Raccomandata</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <template v-for="group in filteredGroups" :key="group.recipient">
            <!-- Prima notifica del gruppo: mostra destinatario -->
            <tr v-for="(n, idx) in group.notifications" :key="n.id" :class="idx > 0 ? 'grouped-row' : 'group-first-row'">
              <td>
                <template v-if="idx === 0">
                  <span class="recipient-name">{{ group.recipient || '—' }}</span>
                  <span v-if="n.emailAddress" class="text-muted" style="display:block;font-size:0.72rem">{{ n.emailAddress }}</span>
                </template>
                <span v-else class="text-muted" style="font-size:0.78rem;padding-left:12px">↳</span>
              </td>
              <td class="text-muted mono" style="font-size:0.78rem">{{ n.unitsDisplay || n.unitDisplayName || '—' }}</td>
              <td>
                <span class="badge" :class="n.deliveryMethod === 0 ? 'badge-blue' : 'badge-purple'">
                  {{ n.deliveryMethodName }}
                </span>
              </td>
              <td><span class="badge" :class="statusBadge(n.status)">{{ n.statusName }}</span></td>
              <td class="text-muted mono" style="font-size:0.75rem">{{ fmtDt(n.sentAt) }}</td>
              <td class="text-muted mono" style="font-size:0.75rem">{{ fmtDt(n.deliveredAt) }}</td>
              <td>
                <span v-if="n.deliveryMethod === 1">
                  <span v-if="n.trackingNumber" class="mono text-secondary" style="font-size:0.78rem">{{ n.trackingNumber }}</span>
                  <span v-else class="text-muted" style="font-size:0.78rem">—</span>
                </span>
                <span v-else class="text-muted" style="font-size:0.78rem">—</span>
              </td>
              <td class="row-actions">
                <!-- Preview — sempre visibile -->
                <button class="btn-icon" title="Anteprima messaggio" style="font-size:0.78rem" @click="openPreview(n)">👁</button>
                <!-- Azioni operative — solo se non readonly -->
                <template v-if="!readonly">
                  <button v-if="n.status === 0" class="btn-icon" title="Modifica testo" style="font-size:0.78rem" @click="openEditText(n)">✎</button>
                  <button v-if="n.status === 0" class="btn-icon" title="Allegati" style="font-size:0.78rem" @click="openAttachModal(n)">📎</button>
                  <button v-if="n.deliveryMethod === 0 && n.status === 0" class="btn-icon" title="Invia email"
                          style="font-size:0.78rem" :disabled="sendingId === n.id || sending" @click="sendSingleEmail(n)">
                    <span v-if="sendingId === n.id" class="spinner" style="width:10px;height:10px;display:inline-block"></span>
                    <span v-else>📧</span>
                  </button>
                  <template v-if="n.deliveryMethod === 1">
                    <button v-if="n.status === 0" class="btn-icon" title="Segna come spedita"
                            style="font-size:0.78rem" @click="openTrackingModal(n, 'sent')">✉</button>
                    <button v-if="n.status === 2" class="btn-icon" title="Segna come consegnata"
                            style="font-size:0.78rem;color:var(--accent-green)" @click="openTrackingModal(n, 'delivered')">✓</button>
                  </template>
                  <button v-if="n.status === 0" class="btn-icon" style="color:var(--accent-red);font-size:0.78rem" title="Elimina"
                          :disabled="sending || sendingId !== null"
                          @click="deleteNotif(n.id)">✕</button>
                </template>
              </td>
            </tr>
            <!-- Separatore visivo tra gruppi -->
            <tr class="group-separator"><td colspan="8"></td></tr>
          </template>
        </tbody>
      </table>
    </div>

    <!-- Modal modifica testo notifica -->
    <div class="modal-overlay" v-if="editTextModal.show" @mousedown.self="editTextModal.show=false">
      <div class="modal" style="max-width:640px;width:95vw">
        <div class="modal-header">
          <h2>Modifica testo — {{ editTextModal.notif?.recipientFullName }}</h2>
          <button class="btn-icon" @click="editTextModal.show=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Oggetto</label>
            <input class="form-input" v-model="editTextModal.subject" placeholder="Oggetto del messaggio" />
          </div>
          <div class="form-group">
            <label class="form-label">Testo</label>
            <textarea class="form-textarea" v-model="editTextModal.body" rows="12" />
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="editTextModal.show=false">Annulla</button>
          <button class="btn btn-primary" @click="saveEditText" :disabled="editTextModal.saving">
            <span v-if="editTextModal.saving" class="spinner" style="width:14px;height:14px"></span>
            Salva
          </button>
        </div>
      </div>
    </div>

    <!-- Modal tracking raccomandata -->
    <div class="modal-overlay" v-if="trackingModal.show" @mousedown.self="trackingModal.show=false">
      <div class="modal" style="max-width:420px">
        <div class="modal-header">
          <h2>{{ trackingModal.action === 'sent' ? 'Segna come spedita' : 'Segna come consegnata' }}</h2>
          <button class="btn-icon" @click="trackingModal.show=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">N° raccomandata (opzionale)</label>
            <input class="form-input" v-model="trackingModal.trackingNumber" placeholder="es. RA123456789IT" />
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="trackingModal.show=false">Annulla</button>
          <button class="btn btn-primary" @click="confirmTracking" :disabled="trackingModal.saving">
            <span v-if="trackingModal.saving" class="spinner" style="width:14px;height:14px"></span>
            Conferma
          </button>
        </div>
      </div>
    </div>

    <!-- Modal preview messaggio -->
    <div class="modal-overlay" v-if="previewModal.show" @mousedown.self="previewModal.show=false">
      <div class="modal" style="max-width:620px">
        <div class="modal-header">
          <h2>Anteprima messaggio</h2>
          <button class="btn-icon" @click="previewModal.show=false">✕</button>
        </div>
        <div class="modal-body" style="padding:0">
          <!-- Email preview -->
          <div v-if="previewModal.notif?.deliveryMethod === 0" class="preview-email">
            <div class="preview-email-header">
              <div class="preview-email-row">
                <span class="preview-label">A:</span>
                <span>{{ previewModal.notif.recipientFullName }}</span>
                <span v-if="previewModal.notif.recipientEmail" class="text-muted" style="margin-left:6px">&lt;{{ previewModal.notif.recipientEmail }}&gt;</span>
              </div>
              <div class="preview-email-row">
                <span class="preview-label">Oggetto:</span>
                <span>{{ previewModal.notif.subjectResolved || '—' }}</span>
              </div>
            </div>
            <div class="preview-email-body" v-html="emailBody(previewModal.notif.bodyResolved)"></div>
          </div>
          <!-- Raccomandata preview -->
          <div v-else class="preview-letter">
            <div class="preview-letter-inner">
              <div class="preview-letter-address">
                <strong>{{ previewModal.notif?.recipientFullName }}</strong>
                <span v-if="previewModal.notif?.unitDisplayName" class="text-muted" style="font-size:0.85rem;display:block">{{ previewModal.notif.unitDisplayName }}</span>
              </div>
              <div class="preview-letter-subject" v-if="previewModal.notif?.subjectResolved">
                <strong>Oggetto:</strong> {{ previewModal.notif.subjectResolved }}
              </div>
              <div class="preview-letter-body">{{ previewModal.notif?.bodyResolved || '—' }}</div>
            </div>
          </div>
        </div>
        <!-- Allegati -->
        <div class="preview-attachments">
          <div class="preview-attach-label">📎 Allegati</div>
          <div v-if="previewModal.loadingAttachments" class="preview-attach-empty">Caricamento…</div>
          <div v-else-if="isFeeNotice || previewModal.attachments.length" class="preview-attach-list">
            <button v-if="isFeeNotice" class="preview-attach-chip preview-attach-dl"
                    @click="downloadAttachment(previewModal.notif)" :disabled="previewModal.downloading">
              <span v-if="previewModal.downloading" class="spinner" style="width:10px;height:10px"></span>
              <span v-else>📕</span> cedolini.pdf ⬇
            </button>
            <button v-for="att in previewModal.attachments" :key="att.id"
                    class="preview-attach-chip preview-attach-dl"
                    :disabled="previewModal.downloadingAttId === att.id"
                    @click="downloadDoc(att)">
              <span v-if="previewModal.downloadingAttId === att.id" class="spinner" style="width:10px;height:10px"></span>
              <span v-else>{{ attIcon(att.mimeType) }}</span> {{ att.fileName }} ⬇
            </button>
          </div>
          <div v-else class="preview-attach-empty">Nessun allegato</div>
        </div>
        <div class="modal-footer">
          <button v-if="isFeeNotice" class="btn btn-ghost" @click="downloadAttachment(previewModal.notif)" :disabled="previewModal.downloading">
            <span v-if="previewModal.downloading" class="spinner" style="width:13px;height:13px"></span>
            <span v-else>⬇ Scarica cedolini PDF</span>
          </button>
          <button class="btn btn-ghost" @click="previewModal.show=false">Chiudi</button>
        </div>
      </div>
    </div>

    <!-- Modal allegati notifica -->
    <div class="modal-overlay" v-if="attachModal.show" @mousedown.self="attachModal.show=false">
      <div class="modal" style="max-width:600px;width:95vw">
        <div class="modal-header">
          <h2>Allegati — {{ attachModal.notif?.recipientFullName }}</h2>
          <button class="btn-icon" @click="attachModal.show=false">✕</button>
        </div>
        <div class="modal-body">
          <AttachPanel
            v-if="attachModal.notif"
            :notification-ids="[attachModal.notif.id]"
            :condominium-id="props.condominiumId"
          />
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="attachModal.show=false">Chiudi</button>
        </div>
      </div>
    </div>

    <!-- Modal selezione rate per FeeNotice -->
    <div class="modal-overlay" v-if="feeModal.show" @mousedown.self="feeModal.show=false">
      <div class="modal" style="max-width:520px">
        <div class="modal-header">
          <h2>Genera notifiche avvisi di pagamento</h2>
          <button class="btn-icon" @click="feeModal.show=false">✕</button>
        </div>
        <div class="modal-body">
          <div v-if="feeModal.loading" class="loading-state">Caricamento…</div>
          <template v-else>
            <div class="form-group">
              <label class="form-label">Rate</label>
              <div style="display:flex;flex-direction:column;gap:6px;max-height:150px;overflow-y:auto;padding:8px;background:var(--bg-base);border:1px solid var(--border);border-radius:6px">
                <label v-for="inst in feeModal.installments" :key="inst.id" style="display:flex;align-items:center;gap:8px;cursor:pointer">
                  <input type="checkbox" :value="inst.id" v-model="feeModal.selectedInst" />
                  <span>Rata {{ inst.installmentNumber }} — scad. {{ inst.dueDate?.slice(0,10) }}</span>
                </label>
                <div v-if="!feeModal.installments.length" class="text-muted" style="font-size:0.85rem">Nessuna rata trovata</div>
              </div>
            </div>
            <div class="form-group">
              <label class="form-label">Destinatari</label>
              <div style="display:flex;gap:16px">
                <label style="display:flex;align-items:center;gap:6px;cursor:pointer">
                  <input type="radio" :value="true" v-model="feeModal.allUnits" /> Tutte le unità
                </label>
                <label style="display:flex;align-items:center;gap:6px;cursor:pointer">
                  <input type="radio" :value="false" v-model="feeModal.allUnits" /> Seleziona unità
                </label>
              </div>
            </div>
            <div class="form-group">
              <label class="form-label">Template</label>
              <select class="form-select" v-model="feeModal.templateId">
                <option :value="null">— automatico —</option>
                <option v-for="t in feeModal.templates" :key="t.id" :value="t.id">{{ t.name }}</option>
              </select>
            </div>
          </template>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="feeModal.show=false">Annulla</button>
          <button class="btn btn-primary" @click="confirmFeeGen" :disabled="generating || feeModal.loading">
            <span v-if="generating" class="spinner" style="width:14px;height:14px"></span>
            Genera
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { communicationNotificationApi, installmentApi, notificationTemplateApi, notificationAttachmentApi, documentApi } from '@/services/api'
import AttachPanel from '@/components/AttachPanel.vue'

const props = defineProps({
  communication:  { type: Object, required: true },
  condominiumId:  { type: Number, required: true },
  readonly:       { type: Boolean, default: false },
})
const emit = defineEmits(['close'])

const store          = useAppStore()
const loading        = ref(false)
const generating     = ref(false)
const regenerating   = ref(false)
const sending        = ref(false)
const sendingId      = ref(null)
const downloadingPdf = ref(false)
const notifications  = ref([])
const sendResult     = ref(null)
const genMethod      = ref(0)
const regenTemplateId    = ref(null)
const availableTemplates = ref([])

// ── Filtri ────────────────────────────────────────────────────────────────────
const filterSearch = ref('')
const filterMethod = ref('')
const filterStatus = ref('')

function clearFilters() {
  filterSearch.value = ''
  filterMethod.value = ''
  filterStatus.value = ''
}

// ── Grouping + filtering ──────────────────────────────────────────────────────
const filteredGroups = computed(() => {
  let list = notifications.value

  if (filterSearch.value.trim()) {
    const q = filterSearch.value.trim().toLowerCase()
    list = list.filter(n => (n.recipientFullName ?? '').toLowerCase().includes(q))
  }
  if (filterMethod.value !== '') {
    const m = parseInt(filterMethod.value)
    list = list.filter(n => n.deliveryMethod === m)
  }
  if (filterStatus.value !== '') {
    const s = parseInt(filterStatus.value)
    list = list.filter(n => n.status === s)
  }

  // Group by recipientFullName
  const map = new Map()
  for (const n of list) {
    const key = n.recipientFullName ?? ''
    if (!map.has(key)) map.set(key, [])
    map.get(key).push(n)
  }
  return Array.from(map.entries()).map(([recipient, notifs]) => ({ recipient, notifications: notifs }))
})

const trackingModal  = ref({ show: false, notif: null, action: '', trackingNumber: '', saving: false })
const previewModal   = ref({ show: false, notif: null, downloading: false, attachments: [], loadingAttachments: false, downloadingAttId: null })
const editTextModal  = ref({ show: false, notif: null, subject: '', body: '', saving: false })

// ── Attachment modal ──────────────────────────────────────────────────────────
const attachModal = ref({ show: false, notif: null })

function openAttachModal(n) {
  attachModal.value = { show: true, notif: n }
}

// ── Modal selezione rate (solo per FeeNotice) ─────────────────────────────────
const isFeeNotice = computed(() => props.communication.communicationType === 'FeeNotice')
const feeModal    = ref({ show: false, installments: [], selectedInst: [], allUnits: true, selectedUnits: [], availableUnits: [], templates: [], templateId: null, loading: false })

async function openFeeGenModal() {
  feeModal.value.loading = true
  feeModal.value.show    = true
  try {
    const [instRes, tmplRes] = await Promise.all([
      installmentApi.getByCondominium(props.condominiumId),
      notificationTemplateApi.getByCondominium(props.condominiumId),
    ])
    feeModal.value.installments  = instRes.data ?? []
    feeModal.value.selectedInst  = feeModal.value.installments.map(i => i.id)
    feeModal.value.templates     = (tmplRes.data ?? []).filter(t => t.communicationType === 'FeeNotice')
    const def = feeModal.value.templates.find(t => t.isDefault)
    feeModal.value.templateId    = def?.id ?? null
    feeModal.value.allUnits      = true
    feeModal.value.selectedUnits = []
    feeModal.value.availableUnits = []
  } catch { /* errors handled globally */ } finally { feeModal.value.loading = false }
}

async function confirmFeeGen() {
  if (!feeModal.value.selectedInst.length) { store.toast('Seleziona almeno una rata', 'error'); return }
  generating.value = true
  try {
    const payload = {
      communicationId:        props.communication.id,
      installmentIds:         feeModal.value.selectedInst,
      unitIds:                feeModal.value.allUnits ? null : feeModal.value.selectedUnits,
      deliveryMethod:         genMethod.value,
      notificationTemplateId: feeModal.value.templateId ?? null,
    }
    await communicationNotificationApi.generateFromFees(payload)
    store.toast('Notifiche generate', 'success')
    feeModal.value.show = false
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { generating.value = false }
}

const pendingCount            = computed(() => notifications.value.filter(n => n.status <= 1).length)
const hasPendingNotifications = computed(() => notifications.value.some(n => n.status <= 1))
const hasPendingForMethod     = computed(() => notifications.value.some(n => n.deliveryMethod === genMethod.value && n.status <= 1))

// ── Load ──────────────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const [notifRes, tmplRes] = await Promise.all([
      communicationNotificationApi.getByCommunication(props.communication.id),
      notificationTemplateApi.getByCondominium(props.condominiumId),
    ])
    notifications.value  = notifRes.data ?? []
    availableTemplates.value = (tmplRes.data ?? []).filter(t => t.communicationType === props.communication.communicationType)
  } catch { notifications.value = [] } finally { loading.value = false }
}

// ── Generate ──────────────────────────────────────────────────────────────────
async function generate() {
  if (!confirm(`Generare le notifiche (${genMethod.value === 0 ? 'Email' : 'Raccomandata'}) per tutti i condomini?\nLe notifiche in bozza esistenti saranno sostituite.`)) return
  generating.value = true
  try {
    await communicationNotificationApi.generate(props.communication.id, { deliveryMethod: genMethod.value })
    store.toast('Notifiche generate', 'success')
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    generating.value = false
  }
}

// ── Regenerate texts ──────────────────────────────────────────────────────────
async function regenerateTexts() {
  if (!confirm(`Rigenerare il testo di tutte le notifiche non ancora inviate (${pendingCount.value})?`)) return
  regenerating.value = true
  try {
    const { data } = await communicationNotificationApi.regenerateTexts(props.communication.id, regenTemplateId.value)
    store.toast(`Testo rigenerato per ${data} notifiche`, 'success')
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    regenerating.value = false
  }
}

// ── Send all email ────────────────────────────────────────────────────────────
async function sendAll() {
  if (!confirm(`Inviare tutte le email in sospeso (${pendingCount.value} notifiche)?`)) return
  sending.value = true
  sendResult.value = null
  try {
    const { data } = await communicationNotificationApi.sendEmail(props.communication.id)
    sendResult.value = data
    store.toast(`${data.sent} email inviate${data.failed ? `, ${data.failed} errori` : ''}`,
                data.failed ? 'error' : 'success')
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    sending.value = false
  }
}

// ── PDF batch raccomandate ────────────────────────────────────────────────────
async function downloadBatchPdf() {
  downloadingPdf.value = true
  try {
    const { data } = await communicationNotificationApi.getBatchPdf(props.communication.id)
    const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const a = document.createElement('a')
    a.href = url; a.download = `raccomandate-${props.communication.id}.pdf`; a.click()
    URL.revokeObjectURL(url)
    store.toast('PDF generato e marcato come stampato', 'success')
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    downloadingPdf.value = false
  }
}

// ── Tracking ──────────────────────────────────────────────────────────────────
function openTrackingModal(n, action) {
  trackingModal.value = { show: true, notif: n, action, trackingNumber: n.trackingNumber ?? '', saving: false }
}

async function confirmTracking() {
  const m = trackingModal.value
  m.saving = true
  try {
    const dto = { trackingNumber: m.trackingNumber || null }
    if (m.action === 'sent')
      await communicationNotificationApi.markSent(m.notif.id, dto)
    else
      await communicationNotificationApi.markDelivered(m.notif.id, dto)
    store.toast('Stato aggiornato', 'success')
    m.show = false
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    m.saving = false
  }
}

// ── Edit text ─────────────────────────────────────────────────────────────────
function openEditText(n) {
  editTextModal.value = { show: true, notif: n, subject: n.subjectResolved ?? '', body: n.bodyResolved ?? '', saving: false }
}

async function saveEditText() {
  const m = editTextModal.value
  if (!m.body.trim()) { store.toast('Il testo non può essere vuoto', 'error'); return }
  m.saving = true
  try {
    await communicationNotificationApi.updateText(m.notif.id, { subjectResolved: m.subject, bodyResolved: m.body })
    store.toast('Testo aggiornato', 'success')
    m.show = false
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    m.saving = false
  }
}

// ── Preview ───────────────────────────────────────────────────────────────────
async function openPreview(n) {
  previewModal.value = { show: true, notif: n, downloading: false, attachments: [], loadingAttachments: true }
  try {
    const { data } = await notificationAttachmentApi.getByNotification(n.id)
    previewModal.value.attachments = data ?? []
  } catch { /* global handler */ } finally { previewModal.value.loadingAttachments = false }
}

async function downloadAttachment(n) {
  if (!n) return
  previewModal.value.downloading = true
  try {
    const { data } = await communicationNotificationApi.getAttachmentPdf(n.id)
    const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const a = document.createElement('a')
    a.href = url
    a.download = `cedolini-${(n.recipientFullName ?? n.id).replace(/\s+/g, '-')}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    previewModal.value.downloading = false
  }
}

async function downloadDoc(att) {
  previewModal.value.downloadingAttId = att.id
  try {
    const { data } = await documentApi.download(att.documentId)
    const url = URL.createObjectURL(new Blob([data], { type: att.mimeType || 'application/octet-stream' }))
    const a = document.createElement('a')
    a.href = url; a.download = att.fileName; a.click()
    URL.revokeObjectURL(url)
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { previewModal.value.downloadingAttId = null }
}

function emailBody(body) {
  if (!body) return '<em style="color:var(--text-muted)">Nessun contenuto</em>'
  return body.replace(/\n/g, '<br>')
}

// ── Send single email ─────────────────────────────────────────────────────────
async function sendSingleEmail(n) {
  sendingId.value = n.id
  try {
    await communicationNotificationApi.sendSingleEmail(n.id)
    store.toast(`Email inviata a ${n.recipientFullName}`, 'success')
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    sendingId.value = null
  }
}

// ── Delete ────────────────────────────────────────────────────────────────────
async function deleteNotif(id) {
  if (!confirm('Eliminare questa notifica?')) return
  try {
    await communicationNotificationApi.delete(id)
    store.toast('Notifica eliminata', 'success')
    await load()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function statusBadge(s) {
  return [
    'badge-muted',   // 0 Draft
    'badge-blue',    // 1 Scheduled
    'badge-green',   // 2 Sent
    'badge-green',   // 3 Delivered
    'badge-red',     // 4 Failed
    'badge-purple',  // 5 Printed
  ][s] ?? 'badge-muted'
}

function attIcon(mime) {
  if (!mime) return '📄'
  if (mime.includes('pdf')) return '📕'
  if (mime.includes('image')) return '🖼'
  if (mime.includes('word') || mime.includes('document')) return '📝'
  if (mime.includes('sheet') || mime.includes('excel')) return '📊'
  return '📄'
}

function fmtDt(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('it-IT', { day: '2-digit', month: '2-digit', year: '2-digit', hour: '2-digit', minute: '2-digit' })
}

onMounted(load)
</script>

<style scoped>
.notif-panel-inner { display: flex; flex-direction: column; gap: 10px; }

.notif-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  flex-wrap: wrap;
}
.notif-toolbar-left { display: flex; align-items: center; gap: 8px; }
.notif-toolbar-right { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.notif-generate-row { display: flex; align-items: center; gap: 4px; }
.form-select-sm { height: 30px; font-size: 0.8rem; padding: 0 8px; }
.form-input-sm  { height: 30px; font-size: 0.8rem; padding: 0 8px; }

.notif-filters {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  padding: 8px;
  background: var(--bg-base);
  border: 1px solid var(--border);
  border-radius: 6px;
}

.send-result {
  font-size: 0.82rem;
  padding: 8px 12px;
  border-radius: 6px;
}
.result-ok   { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
.result-warn { background: #fff7ed; color: #9a3412; border: 1px solid #fed7aa; }

.badge-purple { background: #f5f3ff; color: #6d28d9; border: 1px solid #ddd6fe; }

/* Grouping rows */
.group-first-row td:first-child { border-top: 1px solid var(--border); }
.grouped-row td { background: var(--bg-base); }
.grouped-row td:first-child { border-left: 2px solid var(--border); }
.group-separator td { height: 4px; background: transparent; border: none; padding: 0; }
.recipient-name { font-weight: 500; font-size: 0.85rem; }

/* Email preview */
.preview-email { display: flex; flex-direction: column; }
.preview-email-header {
  padding: 14px 20px;
  background: var(--bg-base);
  border-bottom: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.preview-email-row { display: flex; align-items: center; gap: 8px; font-size: 0.88rem; }
.preview-label { color: var(--text-muted); min-width: 60px; font-size: 0.82rem; }
.preview-email-body {
  padding: 20px;
  font-size: 0.9rem;
  line-height: 1.6;
  white-space: pre-wrap;
  min-height: 120px;
  max-height: 360px;
  overflow-y: auto;
}

/* Attachments strip */
.preview-attachments {
  padding: 10px 20px;
  border-top: 1px solid var(--border);
  background: var(--bg-base);
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.preview-attach-label { font-size: 0.78rem; font-weight: 600; color: var(--text-muted); white-space: nowrap; }
.preview-attach-empty { font-size: 0.82rem; color: var(--text-muted); }
.preview-attach-list  { display: flex; flex-wrap: wrap; gap: 6px; }
.preview-attach-chip {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 2px 8px; border-radius: 4px;
  border: 1px solid var(--border);
  background: var(--bg-surface);
  font-size: 0.78rem; color: var(--text-secondary);
}
.preview-attach-dl {
  cursor: pointer; transition: border-color 0.15s, color 0.15s;
}
.preview-attach-dl:hover:not(:disabled) { border-color: var(--accent); color: var(--accent); }
.preview-attach-dl:disabled { opacity: 0.6; cursor: default; }

/* Raccomandata / letter preview */
.preview-letter {
  padding: 24px;
  background: #fafafa;
}
.preview-letter-inner {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  padding: 32px 36px;
  max-width: 560px;
  margin: 0 auto;
  font-family: 'Times New Roman', serif;
  font-size: 0.92rem;
  line-height: 1.65;
}
.preview-letter-address {
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e5e7eb;
}
.preview-letter-subject {
  margin-bottom: 16px;
  font-size: 0.9rem;
}
.preview-letter-body {
  white-space: pre-wrap;
  min-height: 80px;
}

</style>
