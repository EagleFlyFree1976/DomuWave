<template>
  <div>
    <!-- ══════════════════════════════════════════════════
         MODALITÀ LISTA TABELLE
    ══════════════════════════════════════════════════ -->
    <template v-if="!editingTable">
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
          <table class="cards-on-mobile">
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
                <td data-label="Codice" class="mono">{{ t.code }}</td>
                <td data-label="Nome">{{ t.name || '—' }}</td>
                <td data-label="Totale definito" class="mono text-right">{{ fmtMillesimal(t.totalMillesimal) }}</td>
                <td data-label="Totale calcolato" class="mono text-right">
                  <span :class="hasAnomaly(t) ? 'text-warning' : 'text-success'">
                    {{ fmtMillesimal(t.calculatedMillesimal) }}
                  </span>
                  <span v-if="hasAnomaly(t)" class="anomaly-icon"
                        title="Anomalia: la somma dei millesimi delle unità non corrisponde al totale definito">⚠</span>
                </td>
                <td data-label="Stato">
                  <span class="badge" :class="t.isDraft ? 'badge-draft' : 'badge-green'">
                    {{ t.isDraft ? 'Bozza' : 'Attiva' }}
                  </span>
                </td>
                <td data-label="Abilitazione">
                  <span class="badge" :class="t.isEnabled ? 'badge-green' : 'badge-muted'">
                    {{ t.isEnabled ? 'Abilitata' : 'Disabilitata' }}
                  </span>
                </td>
                <td>
                  <div class="row-actions">
                    <button class="btn btn-sm btn-ghost" @click="openEditEntries(t)">Voci</button>
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
    </template>

    <!-- ══════════════════════════════════════════════════
         MODALITÀ EDIT VOCI MILLESIMALI (inline, full-page)
    ══════════════════════════════════════════════════ -->
    <template v-else>
      <!-- Header -->
      <div class="page-header">
        <div class="edit-header-left">
          <button class="btn btn-ghost" @click="exitEditEntries">← Indietro</button>
          <div>
            <h1 class="edit-title">Voci millesimali — {{ editingTable.code }}<span v-if="editingTable.name"> · {{ editingTable.name }}</span></h1>
            <p class="edit-subtitle">Inserisci il valore dei millesimi per ogni unità immobiliare</p>
          </div>
        </div>

        <!-- Totale ben visibile -->
        <div class="total-badge" :class="totalOk ? 'total-badge--ok' : 'total-badge--error'">
          <span class="total-badge__label">Totale assegnato</span>
          <span class="total-badge__value">{{ fmtMillesimal(totalAssigned) }}</span>
          <span class="total-badge__sep">/</span>
          <span class="total-badge__target">{{ fmtMillesimal(editingTable.totalMillesimal) }}</span>
          <span class="total-badge__icon">{{ totalOk ? '✓' : '✕' }}</span>
        </div>
      </div>

      <!-- Grid unità -->
      <div class="card">
        <div v-if="loadingEntries" class="loading-state"><div class="spinner"></div></div>
        <div v-else-if="!allUnits.length" class="empty-state">
          <div class="empty-icon">◎</div>
          <div>Nessuna unità immobiliare nel condominio.</div>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th class="th-sortable" @click="toggleSort('unit')">
                  Unità <span class="sort-icon">{{ sortIcon('unit') }}</span>
                </th>
                <th class="th-sortable" @click="toggleSort('unitType')">
                  Tipologia <span class="sort-icon">{{ sortIcon('unitType') }}</span>
                </th>
                <th class="th-sortable text-right col-millesimal" @click="toggleSort('millesimal')">
                  Millesimi <span class="sort-icon">{{ sortIcon('millesimal') }}</span>
                </th>
                <th class="th-sortable" @click="toggleSort('staircase')">
                  Scala <span class="sort-icon">{{ sortIcon('staircase') }}</span>
                </th>
                <th class="th-sortable" @click="toggleSort('floor')">
                  Piano <span class="sort-icon">{{ sortIcon('floor') }}</span>
                </th>
                <th>Note</th>
              </tr>
              <tr class="filter-row">
                <th>
                  <input class="filter-input" v-model="filterUnit" placeholder="Filtra unità…" />
                </th>
                <th>
                  <input class="filter-input" v-model="filterUnitType" placeholder="Filtra tipologia…" />
                </th>
                <th></th>
                <th>
                  <input class="filter-input" v-model="filterStaircase" placeholder="Filtra scala…" />
                </th>
                <th>
                  <input class="filter-input" v-model="filterFloor" placeholder="Filtra piano…" />
                </th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in filteredSortedRows" :key="row.unit.id"
                  :class="{ 'row-assigned': row.millesimal > 0 }">
                <td class="mono">
                  {{ row.unit.internalNumber }}
                  <span v-if="row.unit.displayName" class="unit-displayname"> – {{ row.unit.displayName }}</span>
                </td>
                <td class="text-secondary">{{ row.unit.unitType || '—' }}</td>
                <td class="col-millesimal">
                  <input
                    class="mill-input"
                    type="number"
                    step="0.001"
                    min="0"
                    :value="row.millesimal"
                    @change="onMillesimalChange(row, $event)"
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                  />
                  <span v-if="row.saving" class="saving-indicator">…</span>
                </td>
                <td class="text-secondary">{{ row.unit.staircase || '—' }}</td>
                <td class="text-secondary">{{ row.unit.floor }}</td>
                <td>
                  <input
                    class="notes-input"
                    type="text"
                    :value="row.notes"
                    @change="onNotesChange(row, $event)"
                    :disabled="!canEdit"
                    placeholder="—"
                  />
                </td>
              </tr>
            </tbody>
            <tfoot>
              <tr class="total-row">
                <td class="text-secondary">Totale assegnato</td>
                <td class="mono text-right col-millesimal"
                    :class="totalOk ? 'text-success' : 'text-error'">
                  {{ fmtMillesimal(totalAssigned) }}
                </td>
                <td colspan="4"></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </template>

    <!-- ══════════════════════════════════════════════════
         MODAL — Crea / Modifica Tabella
    ══════════════════════════════════════════════════ -->
    <div class="modal-overlay" v-if="showTableModal" @mousedown.self="showTableModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editingTableId ? 'Modifica tabella' : 'Nuova tabella millesimale' }}</h2>
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
            {{ editingTableId ? 'Salva' : 'Crea' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { millesimalTableApi, unitMillesimalApi, unitApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// ─── Filtri e ordinamento voci ─────────────────────────────
const filterUnit      = ref('')
const filterUnitType  = ref('')
const filterStaircase = ref('')
const filterFloor     = ref('')
const sortKey         = ref('unit')   // 'unit' | 'unitType' | 'millesimal' | 'staircase' | 'floor'
const sortDir         = ref(1)        // 1 = asc, -1 = desc

function toggleSort(key) {
  if (sortKey.value === key) sortDir.value = -sortDir.value
  else { sortKey.value = key; sortDir.value = 1 }
}

function sortIcon(key) {
  if (sortKey.value !== key) return '↕'
  return sortDir.value === 1 ? '↑' : '↓'
}

// ─── Tabelle (lista) ───────────────────────────────────────────
const tables      = ref([])
const loading     = ref(false)
const showTableModal    = ref(false)
const editingTableId    = ref(null)   // ID tabella in modifica (modal)
const editingTableCode  = ref(null)   // codice originale
const savingTable       = ref(false)
const tableErrors       = ref({})
const tableForm         = ref({ code: '', name: '', description: '', totalMillesimal: 1000 })

const isDefaultTable = computed(() => editingTableId.value !== null && editingTableCode.value === 'DEF')

function clearTableError(field) { delete tableErrors.value[field] }

function validateTableForm() {
  const e = {}
  const f = tableForm.value
  if (!f.code?.trim())                            e.code             = 'Campo obbligatorio'
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
  editingTableId.value   = t?.id ?? null
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
    if (editingTableId.value) {
      await millesimalTableApi.update(editingTableId.value, {
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

// ─── Edit voci inline (full-page) ─────────────────────────────
// editingTable = null → lista, object → modalità edit
const editingTable   = ref(null)
const allUnits       = ref([])
const existingEntries = ref([])   // UnitMillesimalReadDto[]
const loadingEntries  = ref(false)

// Per ogni unità un "row" reattivo: { unit, entryId, millesimal, notes, saving }
const unitRows = ref([])

const filteredSortedRows = computed(() => {
  let rows = unitRows.value
  const fu = filterUnit.value.trim().toLowerCase()
  const fut = filterUnitType.value.trim().toLowerCase()
  const fs = filterStaircase.value.trim().toLowerCase()
  const ff = filterFloor.value.trim().toLowerCase()
  if (fu) rows = rows.filter(r =>
    (r.unit.internalNumber ?? '').toLowerCase().includes(fu) ||
    (r.unit.displayName ?? '').toLowerCase().includes(fu)
  )
  if (fut) rows = rows.filter(r => (r.unit.unitType ?? '').toLowerCase().includes(fut))
  if (fs) rows = rows.filter(r => (r.unit.staircase ?? '').toLowerCase().includes(fs))
  if (ff) rows = rows.filter(r => String(r.unit.floor ?? '').toLowerCase().includes(ff))

  return [...rows].sort((a, b) => {
    let av, bv
    if (sortKey.value === 'millesimal') {
      av = Number(a.millesimal) || 0
      bv = Number(b.millesimal) || 0
    } else if (sortKey.value === 'unitType') {
      av = (a.unit.unitType ?? '').toLowerCase()
      bv = (b.unit.unitType ?? '').toLowerCase()
    } else if (sortKey.value === 'staircase') {
      av = (a.unit.staircase ?? '').toLowerCase()
      bv = (b.unit.staircase ?? '').toLowerCase()
    } else if (sortKey.value === 'floor') {
      av = Number(a.unit.floor) || 0
      bv = Number(b.unit.floor) || 0
    } else {
      av = (a.unit.internalNumber ?? '').toLowerCase()
      bv = (b.unit.internalNumber ?? '').toLowerCase()
    }
    if (av < bv) return -sortDir.value
    if (av > bv) return sortDir.value
    return 0
  })
})

const totalAssigned = computed(() =>
  unitRows.value.reduce((s, r) => s + (Number(r.millesimal) || 0), 0)
)

const totalOk = computed(() =>
  editingTable.value != null &&
  Math.abs(totalAssigned.value - editingTable.value.totalMillesimal) < 0.0001
)

async function openEditEntries(table) {
  editingTable.value  = table
  loadingEntries.value = true
  unitRows.value = []

  try {
    // carica unità e voci in parallelo
    const [unitsRes, entriesRes] = await Promise.all([
      unitApi.getByCondominium(store.selectedCondominioId),
      unitMillesimalApi.getByTable(table.id),
    ])
    allUnits.value       = unitsRes.data ?? []
    existingEntries.value = entriesRes.data ?? []

    // costruisce una riga per OGNI unità (assegnata o meno)
    const entryMap = new Map(existingEntries.value.map(e => [e.unitId, e]))
    unitRows.value = allUnits.value.map(unit => {
      const entry = entryMap.get(unit.id)
      return {
        unit,
        entryId:    entry?.id ?? null,
        millesimal: entry?.millesimal ?? 0,
        notes:      entry?.notes ?? '',
        saving:     false,
      }
    })
  } catch {
    store.toast('Errore nel caricamento dei dati', 'error')
  } finally {
    loadingEntries.value = false
  }
}

async function exitEditEntries() {
  editingTable.value = null
  await loadTables()
}

// Chiamato su @change dell'input millesimal
async function onMillesimalChange(row, event) {
  const newVal = parseFloat(event.target.value) || 0
  if (newVal === row.millesimal) return
  row.millesimal = newVal
  await persistRow(row)
}

// Chiamato su @change dell'input note
async function onNotesChange(row, event) {
  const newVal = event.target.value || ''
  if (newVal === row.notes) return
  row.notes = newVal
  await persistRow(row)
}

async function persistRow(row) {
  row.saving = true
  try {
    if (row.millesimal > 0 || row.entryId) {
      if (row.entryId) {
        // update
        if (row.millesimal <= 0) {
          // cancella la voce se si azzera il valore
          await unitMillesimalApi.delete(row.entryId)
          row.entryId = null
        } else {
          await unitMillesimalApi.update(row.entryId, {
            millesimal: row.millesimal,
            notes:      row.notes || null,
          })
        }
      } else if (row.millesimal > 0) {
        // crea nuova voce
        const { data } = await unitMillesimalApi.create({
          millesimalTableId: editingTable.value.id,
          unitId:            row.unit.id,
          millesimal:        row.millesimal,
          notes:             row.notes || null,
        })
        row.entryId = data?.id ?? null
      }
    }
  } catch (err) {
    if (!err?.response) store.toast('Errore di rete durante il salvataggio', 'error')
    else store.toast('Errore durante il salvataggio', 'error')
  } finally {
    row.saving = false
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
  editingTable.value = null
  allUnits.value = []
  loadTables()
})
onMounted(loadTables)
onUnmounted(() => window.removeEventListener('app:refresh', loadTables))
window.addEventListener('app:refresh', loadTables)
</script>

<style scoped>
/* ── Lista tabelle ────────────────────────────────────────── */
.row-actions   { display: flex; gap: .4rem; justify-content: flex-end; }
.row-anomaly td { background: color-mix(in srgb, var(--accent-amber, #f59e0b) 8%, transparent); }
.badge-draft   { background: var(--bg-surface); border: 1px solid var(--border); color: var(--text-muted); }
.anomaly-icon  { color: var(--accent-amber, #f59e0b); margin-left: .3rem; font-size: .9rem; cursor: help; }
.text-right    { text-align: right; }
.text-success  { color: var(--accent-green, #22c55e); }
.text-warning  { color: var(--accent-amber, #f59e0b); }
.text-error    { color: var(--accent-red, #e53e3e); }

/* Validation */
.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: .78rem; color: var(--accent-red, #e53e3e); margin-top: .2rem; display: block; }

/* ── Edit voci inline ─────────────────────────────────────── */
.edit-header-left {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.edit-title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0;
}
.edit-subtitle {
  font-size: .82rem;
  color: var(--text-muted);
  margin: .1rem 0 0;
}

/* Totale badge */
.total-badge {
  display: flex;
  align-items: center;
  gap: .5rem;
  padding: .6rem 1.2rem;
  border-radius: 10px;
  font-weight: 600;
  font-size: 1.05rem;
  border: 2px solid;
  transition: background 0.2s, border-color 0.2s, color 0.2s;
}
.total-badge--ok {
  background: color-mix(in srgb, #22c55e 12%, transparent);
  border-color: #22c55e;
  color: #16a34a;
}
.total-badge--error {
  background: color-mix(in srgb, #e53e3e 10%, transparent);
  border-color: #e53e3e;
  color: #dc2626;
}
.total-badge__label {
  font-size: .8rem;
  font-weight: 400;
  opacity: .8;
  margin-right: .2rem;
}
.total-badge__value {
  font-size: 1.3rem;
  font-variant-numeric: tabular-nums;
}
.total-badge__sep {
  opacity: .5;
}
.total-badge__target {
  font-size: .95rem;
  font-variant-numeric: tabular-nums;
  opacity: .75;
}
.total-badge__icon {
  font-size: 1rem;
  margin-left: .2rem;
}

/* Sorting / filtering */
.th-sortable {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
}
.th-sortable:hover { color: var(--accent); }
.sort-icon {
  font-size: .75rem;
  opacity: .55;
  margin-left: .2rem;
}
.filter-row th { padding-top: 0; padding-bottom: .4rem; }
.filter-input {
  width: 100%;
  padding: .2rem .4rem;
  font-size: .8rem;
  border: 1px solid var(--border);
  border-radius: 4px;
  background: var(--bg-surface);
  color: var(--text);
}
.filter-input::placeholder { color: var(--text-muted); }
.filter-input:focus { outline: none; border-color: var(--border-active); }

/* Grid voci */
.col-millesimal { text-align: right; min-width: 130px; }
.mill-input {
  width: 110px;
  text-align: right;
  padding: .25rem .4rem;
  border: 1px solid var(--border);
  border-radius: 4px;
  background: var(--bg);
  color: var(--text);
  font-size: .9rem;
  font-variant-numeric: tabular-nums;
}
.mill-input:focus {
  outline: none;
  border-color: #34d399;
}
.mill-input:disabled { opacity: .5; cursor: not-allowed; }

.notes-input {
  width: 100%;
  padding: .25rem .4rem;
  border: 1px solid transparent;
  border-radius: 4px;
  background: transparent;
  color: var(--text);
  font-size: .9rem;
}
.notes-input:focus {
  outline: none;
  border-color: var(--border);
  background: var(--bg-surface);
}
.notes-input:disabled { opacity: .5; cursor: not-allowed; }
.notes-input::placeholder { color: var(--text-muted); }

.saving-indicator {
  font-size: .75rem;
  color: var(--text-muted);
  margin-left: .3rem;
  animation: pulse 1s infinite;
}
@keyframes pulse { 0%,100% { opacity:1 } 50% { opacity:.3 } }

.row-assigned td { /* subtile highlight per righe con valore */ }
.total-row td { font-weight: 600; border-top: 2px solid var(--border); }
.unit-displayname { font-size: .82rem; color: var(--text-muted); font-style: italic; font-family: inherit; }

@media (max-width: 768px) {
  .edit-header-left {
    flex-wrap: wrap;
  }
  .total-badge {
    flex-wrap: wrap;
  }
  .page-header {
    flex-wrap: wrap;
    gap: 0.75rem;
  }
}
</style>
