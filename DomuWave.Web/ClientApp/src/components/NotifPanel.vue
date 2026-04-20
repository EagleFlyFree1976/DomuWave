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
      <div class="notif-toolbar-right">
        <!-- Genera -->
        <div class="notif-generate-row">
          <select class="form-select form-select-sm" v-model="genMethod">
            <option :value="0">Email</option>
            <option :value="1">Raccomandata</option>
          </select>
          <button class="btn btn-sm btn-ghost" @click="generate" :disabled="generating">
            <span v-if="generating" class="spinner" style="width:11px;height:11px"></span>
            <span v-else>↺ Genera</span>
          </button>
        </div>
        <!-- Invia email -->
        <button v-if="hasEmailPending" class="btn btn-sm btn-primary" @click="sendAll" :disabled="sending">
          <span v-if="sending" class="spinner" style="width:11px;height:11px"></span>
          <span v-else>📧 Invia tutto</span>
        </button>
        <!-- PDF raccomandate -->
        <button v-if="hasRaccomandatePending" class="btn btn-sm btn-ghost" @click="downloadBatchPdf" :disabled="downloadingPdf">
          <span v-if="downloadingPdf" class="spinner" style="width:11px;height:11px"></span>
          <span v-else>🖨 Stampa raccomandate</span>
        </button>
      </div>
    </div>

    <!-- Risultato invio -->
    <div v-if="sendResult" class="send-result" :class="sendResult.failed > 0 ? 'result-warn' : 'result-ok'">
      Inviate {{ sendResult.sent }}<span v-if="sendResult.failed > 0"> · {{ sendResult.failed }} errori</span>
      <span v-if="sendResult.errors?.length"> — {{ sendResult.errors.join('; ') }}</span>
    </div>

    <!-- Tabella notifiche -->
    <div v-if="loading" class="loading-state" style="padding:8px">Caricamento…</div>
    <div v-else-if="!notifications.length" class="empty-state" style="padding:12px 0">
      Nessuna notifica generata. Clicca "Genera" per creare le notifiche per tutti i condomini.
    </div>
    <div v-else class="table-wrap" style="margin-top:8px">
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
          <tr v-for="n in notifications" :key="n.id">
            <td>{{ n.recipientFullName || '—' }}</td>
            <td class="text-muted mono" style="font-size:0.78rem">{{ n.unitDisplayName || '—' }}</td>
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
              <!-- Raccomandata: mark sent / delivered -->
              <template v-if="n.deliveryMethod === 1">
                <button v-if="n.status <= 5" class="btn-icon" title="Segna come spedita"
                        style="font-size:0.78rem" @click="openTrackingModal(n, 'sent')">✉</button>
                <button v-if="n.status === 2" class="btn-icon" title="Segna come consegnata"
                        style="font-size:0.78rem;color:var(--accent-green)" @click="openTrackingModal(n, 'delivered')">✓</button>
              </template>
              <button class="btn-icon" style="color:var(--accent-red);font-size:0.78rem" title="Elimina"
                      @click="deleteNotif(n.id)">✕</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal tracking raccomandata -->
    <div class="modal-overlay" v-if="trackingModal.show" @click.self="trackingModal.show=false">
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
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { communicationNotificationApi } from '@/services/api'

const props = defineProps({
  communication:  { type: Object, required: true },
  condominiumId:  { type: Number, required: true },
})
const emit = defineEmits(['close'])

const store         = useAppStore()
const loading       = ref(false)
const generating    = ref(false)
const sending       = ref(false)
const downloadingPdf = ref(false)
const notifications = ref([])
const sendResult    = ref(null)
const genMethod     = ref(0)

const trackingModal = ref({ show: false, notif: null, action: '', trackingNumber: '', saving: false })

const pendingCount      = computed(() => notifications.value.filter(n => n.status <= 1).length)
const hasEmailPending   = computed(() => notifications.value.some(n => n.deliveryMethod === 0 && n.status <= 1))
const hasRaccomandatePending = computed(() => notifications.value.some(n => n.deliveryMethod === 1 && n.status <= 1))

// ── Load ──────────────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const { data } = await communicationNotificationApi.getByCommunication(props.communication.id)
    notifications.value = data ?? []
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

.send-result {
  font-size: 0.82rem;
  padding: 8px 12px;
  border-radius: 6px;
}
.result-ok   { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
.result-warn { background: #fff7ed; color: #9a3412; border: 1px solid #fed7aa; }

.badge-purple { background: #f5f3ff; color: #6d28d9; border: 1px solid #ddd6fe; }
</style>
