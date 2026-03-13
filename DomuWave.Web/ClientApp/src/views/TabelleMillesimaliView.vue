<template>
  <div>
    <!-- ── Header ──────────────────────────────────────────── -->
    <div class="page-header">
      <h1>Tabelle Millesimali</h1>
      <button v-if="canCreate && store.selectedCondominioId" class="btn btn-primary" @click="openTableModal()">
        + Nuova tabella
      </button>
    </div>

    <!-- ── Lista tabelle ───────────────────────────────────── -->
    <div class="card">
      <div v-if="!store.selectedCondominioId" class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Seleziona un condominio per visualizzare le tabelle millesimali.</div>
      </div>
      <div v-else-if="loading" class="loading-state"><div class="spinner"></div></div>
      <div v-else-if="!tables.length" class="empty-state">
        <div class="empty-icon">◎</div>
        <div>Nessuna tabella millesimale. Clicca "+ Nuova tabella" per iniziare.</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Codice</th>
              <th>Nome</th>
              <th class="text-right">Totale definito</th>
              <th class="text-right">Totale calcolato</th>
              <th>Stato</th>
              <th>Abilitazione</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in tables" :key="t.id" :class="{ 'row-anomaly': hasAnomaly(t) }">
              <td class="mono">{{ t.code }}</td>
              <td>{{ t.name || '—' }}</td>
              <td class="mono text-right">{{ fmtMillesimal(t.totalMillesimal) }}</td>
              <td class="mono text-right">
                <span :class="hasAnomaly(t) ? 'text-warning' : 'text-success'">
                  {{ fmtMillesimal(t.calculatedMillesimal) }}
                </span>
                <span v-if="hasAnomaly(t)" class="anomaly-icon"
                      title="Anomalia: la somma dei millesimi delle unità non corrisponde al totale definito">⚠</span>
              </td>
              <td>
                <span class="badge" :class="t.isDraft ? 'badge-draft' : 'badge-green'">
                  {{ t.isDraft ? 'Bozza' : 'Attiva' }}
                </span>
              </td>
              <td>
                <span class="badge" :class="t.isEnabled ? 'badge-green' : 'badge-muted'">
                  {{ t.isEnabled ? 'Abilitata' : 'Disabilitata' }}
                </span>
              </td>
              <td>
                <div class="row-actions">
                  <button class="btn btn-sm btn-ghost" @click="openEntriesModal(t)">Voci</button>
                  <button v-if="canEdit" class="btn btn-sm btn-ghost" @click="toggleEnabled(t)">
                    {{ t.isEnabled ? 'Disabilita' : 'Abilita' }}
                  </button>
                  <button v-if="canEdit" class="btn-icon" @click="openTableModal(t)" title="Modifica">✎</button>
                  <button v-if="canDelete && t.code !== 'DEF'" class="btn-icon" style="color:var(--accent-red)"
                          @click="deleteTable(t.id)" title="Elimina">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica Tabella
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showTableModal" @click.self="showTableModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingTable ? 'Modifica tabella' : 'Nuova tabella millesimale' }}</h2>
          <button class="btn-icon" @click="showTableModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group" :class="{ 'has-error': tableErrors.code }">
              <label class="form-label">Codice *</label>
              <input class="form-input" v-model="tableForm.code" placeholder="es. TAB-A"
                     :disabled="isDefaultTable" @input="clearTableError('code')" />
              <span v-if="tableErrors.code" class="field-error">{{ tableErrors.code }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Nome</label>
              <input class="form-input" v-model="tableForm.name" placeholder="es. Tabella proprietà"
                     :disabled="isDefaultTable" />
            </div>
            <div class="form-group" :class="{ 'has-error': tableErrors.totalMillesimal }">
              <label class="form-label">Totale millesimi *</label>
              <input class="form-input" type="number" step="0.001" v-model.number="tableForm.totalMillesimal"
                     @input="clearTableError('totalMillesimal')" />
              <span v-if="tableErrors.totalMillesimal" class="field-error">{{ tableErrors.totalMillesimal }}</span>
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Descrizione</label>
            <textarea class="form-textarea" v-model="tableForm.description" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showTableModal = false">Annulla</button>
          <button class="btn btn-primary" @click="saveTable" :disabled="savingTable">
            <span v-if="savingTable" class="spinner" style="width:14px;height:14px"></span>
            {{ editingTable ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════
         MODAL — Voci Millesimali
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showEntriesModal" @click.self="closeEntriesModal">
      <div class="modal modal-lg">
        <div class="modal-header">
          <div>
            <h2>Voci millesimali</h2>
            <p class="modal-subtitle" v-if="selectedTable">
              {{ selectedTable.code }}{{ selectedTable.name ? ' – ' + selectedTable.name : '' }}
              &nbsp;·&nbsp; Totale atteso:
              <strong>{{ fmtMillesimal(selectedTable.totalMillesimal) }}</strong>
              &nbsp;·&nbsp; Assegnato:
              <strong :class="totalAssigned !== selectedTable.totalMillesimal ? 'text-warning' : 'text-success'">
                {{ fmtMillesimal(totalAssigned) }}
              </strong>
            </p>
          </div>
          <button class="btn-icon" @click="closeEntriesModal">✕</button>
        </div>

        <div class="modal-body">
          <!-- Add entry form -->
          <div class="items-toolbar" v-if="canCreate && !showEntryForm">
            <button class="btn btn-primary btn-sm" @click="openEntryForm()">+ Aggiungi unità</button>
          </div>

          <div class="item-form-panel" v-if="showEntryForm">
            <div class="form-grid form-grid-3">
              <div class="form-group" style="grid-column: span 2" :class="{ 'has-error': entryErrors.unitId }">
                <label class="form-label">Unità immobiliare *</label>
                <select class="form-select" v-model.number="entryForm.unitId" @change="clearEntryError('unitId')">
                  <option :value="null" disabled>Seleziona unità…</option>
                  <option v-for="u in availableUnits" :key="u.id" :value="u.id">
                    {{ u.internalNumber }}{{ u.displayName ? ' – ' + u.displayName : '' }}
                    <template v-if="u.staircase"> · Scala {{ u.staircase }}</template>
                    · Piano {{ u.floor }}
                  </option>
                </select>
                <span v-if="entryErrors.unitId" class="field-error">{{ entryErrors.unitId }}</span>
              </div>
              <div class="form-group" :class="{ 'has-error': entryErrors.millesimal }">
                <label class="form-label">Millesimi *</label>
                <input class="form-input" type="number" step="0.001" v-model.number="entryForm.millesimal"
                       @input="clearEntryError('millesimal')" />
                <span v-if="entryErrors.millesimal" class="field-error">{{ entryErrors.millesimal }}</span>
              </div>
              <div class="form-group" style="grid-column: span 3">
                <label class="form-label">Note</label>
                <input class="form-input" v-model="entryForm.notes" />
              </div>
            </div>
            <div class="item-form-actions">
              <button class="btn btn-ghost btn-sm" @click="cancelEntryForm">Annulla</button>
              <button class="btn btn-primary btn-sm" @click="saveEntry" :disabled="savingEntry">
                <span v-if="savingEntry" class="spinner" style="width:12px;height:12px"></span>
                {{ editingEntry ? 'Salva' : 'Aggiungi' }}
              </button>
            </div>
          </div>

          <!-- Entries table -->
          <div v-if="loadingEntries" class="loading-state"><div class="spinner"></div></div>
          <div v-else-if="!entries.length && !showEntryForm" class="empty-state" style="padding:1.5rem">
            <div class="empty-icon">◎</div>
            <div>Nessuna voce. Clicca "+ Aggiungi unità" per iniziare.</div>
          </div>
          <div v-else-if="entries.length" class="table-wrap" style="margin-top:.5rem">
            <table>
              <thead>
                <tr>
                  <th>Unità</th>
                  <th>Scala</th>
                  <th>Piano</th>
                  <th class="text-right">Millesimi</th>
                  <th>Note</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="e in entries" :key="e.id">
                  <td class="mono">{{ e.unitNumber }}<span v-if="e.unitDisplayName" class="unit-displayname"> – {{ e.unitDisplayName }}</span></td>
                  <td class="text-secondary">{{ e.unitStaircase || '—' }}</td>
                  <td class="text-secondary">{{ e.unitFloor }}</td>
                  <td class="mono text-right">{{ fmtMillesimal(e.millesimal) }}</td>
                  <td class="text-secondary">{{ e.notes || '—' }}</td>
                  <td>
                    <div class="row-actions">
                      <button v-if="canEdit" class="btn-icon" @click="openEntryForm(e)" title="Modifica">✎</button>
                      <button v-if="canDelete" class="btn-icon" style="color:var(--accent-red)"
                              @click="deleteEntry(e.id)" title="Elimina">✕</button>
                    </div>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr class="total-row">
                  <td colspan="3" class="text-secondary">Totale assegnato</td>
                  <td class="mono text-right"
                      :class="totalAssigned !== selectedTable?.totalMillesimal ? 'text-warning' : 'text-success'">
                    {{ fmtMillesimal(totalAssigned) }}
                  </td>
                  <td colspan="2"></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-ghost" @click="closeEntriesModal">Chiudi</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { millesimalTableApi, unitMillesimalApi, unitApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ─── Tabelle ──────────────────────────────────────────────────
const tables      = ref([])
const loading     = ref(false)
const showTableModal    = ref(false)
const editingTable      = ref(null)
const editingTableCode  = ref(null)   // original code of the table being edited
const savingTable       = ref(false)
const tableErrors       = ref({})
const tableForm         = ref({ code: '', name: '', description: '', totalMillesimal: 1000 })

const isDefaultTable = computed(() => editingTable.value !== null && editingTableCode.value === 'DEF')

function clearTableError(field) { delete tableErrors.value[field] }

function validateTableForm() {
  const e = {}
  const f = tableForm.value
  if (!f.code?.trim())                        e.code             = 'Campo obbligatorio'
  if (!f.totalMillesimal || f.totalMillesimal <= 0) e.totalMillesimal = 'Inserire un valore maggiore di zero'
  tableErrors.value = e
  return Object.keys(e).length === 0
}

async function loadTables() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await millesimalTableApi.getByCondominium(store.selectedCondominioId)
    tables.value = data ?? []
  } catch { tables.value = [] } finally { loading.value = false }
}

function openTableModal(t = null) {
  tableErrors.value = {}
  editingTable.value     = t?.id ?? null
  editingTableCode.value = t?.code ?? null
  tableForm.value = t
    ? { code: t.code, name: t.name ?? '', description: t.description ?? '', totalMillesimal: t.totalMillesimal }
    : { code: '', name: '', description: '', totalMillesimal: 1000 }
  showTableModal.value = true
}

async function saveTable() {
  if (!validateTableForm()) return
  savingTable.value = true
  try {
    if (editingTable.value) {
      await millesimalTableApi.update(editingTable.value, {
        code: tableForm.value.code, name: tableForm.value.name,
        description: tableForm.value.description,
        totalMillesimal: tableForm.value.totalMillesimal,
      })
    } else {
      await millesimalTableApi.create({
        condominiumId: store.selectedCondominioId,
        code: tableForm.value.code, name: tableForm.value.name,
        description: tableForm.value.description,
        totalMillesimal: tableForm.value.totalMillesimal,
      })
    }
    showTableModal.value = false
    await loadTables()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingTable.value = false }
}

async function deleteTable(id) {
  if (!confirm('Eliminare questa tabella millesimale e tutte le sue voci?')) return
  try {
    await millesimalTableApi.delete(id)
    await loadTables()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

async function toggleEnabled(t) {
  try {
    await millesimalTableApi.setEnabled(t.id, !t.isEnabled)
    await loadTables()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Voci millesimali ─────────────────────────────────────────
const showEntriesModal = ref(false)
const selectedTable    = ref(null)
const entries          = ref([])
const loadingEntries   = ref(false)

const showEntryForm  = ref(false)
const editingEntry   = ref(null)
const savingEntry    = ref(false)
const entryErrors    = ref({})
const entryForm      = ref({ unitId: null, millesimal: 0, notes: '' })

// All units for the condominium (loaded once)
const allUnits   = ref([])
let   unitsLoaded = false

// Units not yet assigned to this table (for create dropdown)
const assignedUnitIds = computed(() => new Set(entries.value.map(e => e.unitId)))
const availableUnits  = computed(() =>
  editingEntry.value
    ? allUnits.value   // when editing, show all (current unit included)
    : allUnits.value.filter(u => !assignedUnitIds.value.has(u.id))
)

const totalAssigned = computed(() =>
  entries.value.reduce((s, e) => s + (e.millesimal ?? 0), 0)
)

function clearEntryError(field) { delete entryErrors.value[field] }

function validateEntryForm() {
  const e = {}
  const f = entryForm.value
  if (!f.unitId)                          e.unitId    = 'Selezionare un\'unità'
  if (!f.millesimal || f.millesimal <= 0) e.millesimal = 'Inserire un valore maggiore di zero'
  entryErrors.value = e
  return Object.keys(e).length === 0
}

async function openEntriesModal(table) {
  selectedTable.value  = table
  showEntriesModal.value = true
  showEntryForm.value  = false
  editingEntry.value   = null
  entries.value        = []

  // Load units once per condominium
  if (!unitsLoaded && store.selectedCondominioId) {
    try {
      const { data } = await unitApi.getByCondominium(store.selectedCondominioId)
      allUnits.value = data ?? []
      unitsLoaded = true
    } catch { allUnits.value = [] }
  }

  await loadEntries()
}

function closeEntriesModal() {
  showEntriesModal.value = false
  selectedTable.value    = null
  entries.value          = []
  showEntryForm.value    = false
  loadTables()
}

async function loadEntries() {
  if (!selectedTable.value) return
  loadingEntries.value = true
  try {
    const { data } = await unitMillesimalApi.getByTable(selectedTable.value.id)
    entries.value = data ?? []
  } catch { entries.value = [] } finally { loadingEntries.value = false }
}

function openEntryForm(e = null) {
  entryErrors.value = {}
  editingEntry.value = e?.id ?? null
  entryForm.value = e
    ? { unitId: e.unitId, millesimal: e.millesimal, notes: e.notes ?? '' }
    : { unitId: null, millesimal: 0, notes: '' }
  showEntryForm.value = true
}

function cancelEntryForm() {
  showEntryForm.value = false
  editingEntry.value  = null
}

async function saveEntry() {
  if (!validateEntryForm()) return
  savingEntry.value = true
  try {
    if (editingEntry.value) {
      await unitMillesimalApi.update(editingEntry.value, {
        millesimal: entryForm.value.millesimal,
        notes:      entryForm.value.notes || null,
      })
    } else {
      await unitMillesimalApi.create({
        millesimalTableId: selectedTable.value.id,
        unitId:            entryForm.value.unitId,
        millesimal:        entryForm.value.millesimal,
        notes:             entryForm.value.notes || null,
      })
    }
    showEntryForm.value = false
    editingEntry.value  = null
    await loadEntries()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally { savingEntry.value = false }
}

async function deleteEntry(id) {
  if (!confirm('Eliminare questa voce millesimale?')) return
  try {
    await unitMillesimalApi.delete(id)
    await loadEntries()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ─── Anomalia ─────────────────────────────────────────────────
function hasAnomaly(t) {
  return t.calculatedMillesimal != null && t.calculatedMillesimal !== t.totalMillesimal
}

// ─── Formatter ───────────────────────────────────────────────
const fmtMillesimal = (v) => v != null
  ? Number(v).toLocaleString('it-IT', { minimumFractionDigits: 3, maximumFractionDigits: 3 })
  : '—'

// ─── Init ─────────────────────────────────────────────────────
watch(() => store.selectedCondominioId, () => {
  unitsLoaded = false
  allUnits.value = []
  loadTables()
})
onMounted(loadTables)
</script>

<style scoped>
.row-actions   { display: flex; gap: .4rem; justify-content: flex-end; }
.row-anomaly td { background: color-mix(in srgb, var(--accent-amber, #f59e0b) 8%, transparent); }
.badge-draft   { background: var(--bg-surface); border: 1px solid var(--border); color: var(--text-muted); }
.anomaly-icon  { color: var(--accent-amber, #f59e0b); margin-left: .3rem; font-size: .9rem; cursor: help; }
.text-right    { text-align: right; }
.modal-lg      { max-width: 820px; width: 95vw; }
.modal-subtitle { margin: .15rem 0 0; font-size: .85rem; color: var(--text-muted); }
.items-toolbar { margin-bottom: .75rem; }
.text-success  { color: var(--accent-green, #22c55e); }
.text-warning  { color: var(--accent-amber, #f59e0b); }

.item-form-panel {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
}
.form-grid-3         { grid-template-columns: 1fr 1fr 1fr; }
.item-form-actions   { display: flex; justify-content: flex-end; gap: .5rem; margin-top: .5rem; }
.total-row td        { font-weight: 600; border-top: 2px solid var(--border); }

/* Validation */
.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: .78rem; color: var(--accent-red, #e53e3e); margin-top: .2rem; display: block; }
.unit-displayname { font-size: .82rem; color: var(--text-muted); font-style: italic; font-family: inherit; }
</style>
