<template>
  <div>
    <div class="page-header">
      <h1>Condomini</h1>
      <button v-if="canCreate" class="btn btn-primary" @click="openModal()">+ Nuovo condominio</button>
    </div>

    <!-- Search -->
    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca per nome o codice…" />
      <select class="form-select" v-model="filterActive" style="width:160px">
        <option value="">Tutti</option>
        <option value="true">Attivi</option>
        <option value="false">Inattivi</option>
      </select>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon">⬡</div>
        <div>Nessun condominio trovato</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Nome</th>
              <th>Codice</th>
              <th>Email</th>
              <th>Unità</th>
              <th>Rate</th>
              <th>Stato</th>
              <th class="text-center">Setup</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in filtered" :key="c.id">
              <td>
                <router-link :to="`/condomini/${c.id}`" class="text-accent" style="font-weight:500">{{ c.name }}</router-link>
              </td>
              <td class="mono text-muted">{{ c.code || '—' }}</td>
              <td class="text-secondary">{{ c.email || '—' }}</td>
              <td>{{ c.numberOfUnits }}</td>
              <td class="text-secondary">{{ c.installmentFrequency }}</td>
              <td><span class="badge" :class="c.isActive ? 'badge-green' : 'badge-muted'">{{ c.isActive ? 'Attivo' : 'Inattivo' }}</span></td>
              <td class="text-center setup-cell">
                <template v-if="setupStatus[c.id] === null">
                  <span class="setup-loading">…</span>
                </template>
                <template v-else-if="setupStatus[c.id]">
                  <router-link :to="`/condomini/${c.id}/setup`" class="setup-pill" :class="setupStatus[c.id].ok === setupStatus[c.id].total ? 'setup-ok' : 'setup-ko'">
                    <span class="setup-icon">{{ setupStatus[c.id].ok === setupStatus[c.id].total ? '✓' : '✕' }}</span>
                    {{ setupStatus[c.id].ok }}/{{ setupStatus[c.id].total }}
                  </router-link>
                </template>
              </td>
              <td>
                <div v-if="canEdit || canDelete" class="row-actions">
                  <button v-if="canEdit" class="btn-icon" @click="openModal(c)" title="Modifica">✎</button>
                  <button v-if="canDelete" class="btn-icon" @click="deleteItem(c.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal modal--wide">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuovo' }} condominio</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">

          <!-- Dati anagrafici -->
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Dati anagrafici</legend>
            <div class="form-grid">
              <div class="form-group" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="form.name" placeholder="Es. Condominio Roma" @input="clearError('name')" />
                <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Codice</label>
                <input class="form-input" v-model="form.code" placeholder="COD-001" />
              </div>
              <div class="form-group">
                <label class="form-label">Codice fiscale</label>
                <input class="form-input" v-model="form.taxCode" maxlength="16" />
              </div>
              <div class="form-group">
                <label class="form-label">Partita IVA</label>
                <input class="form-input" v-model="form.vatNumber" maxlength="11" />
              </div>
            </div>
          </fieldset>

          <!-- Contatti -->
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Contatti</legend>
            <div class="form-grid">
              <div class="form-group" :class="{ 'has-error': errors.email }">
                <label class="form-label">Email</label>
                <input class="form-input" type="email" v-model="form.email" @input="clearError('email')" />
                <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Telefono</label>
                <input class="form-input" v-model="form.phone" />
              </div>
              <div class="form-group">
                <label class="form-label">PEC</label>
                <input class="form-input" type="email" v-model="form.pec" />
              </div>
            </div>
          </fieldset>

          <!-- Dati tecnici -->
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Dati tecnici</legend>
            <div class="form-grid">
              <div class="form-group" :class="{ 'has-error': errors.numberOfUnits }">
                <label class="form-label">N° unità *</label>
                <input class="form-input" type="number" min="1" v-model.number="form.numberOfUnits" @input="clearError('numberOfUnits')" />
                <span v-if="errors.numberOfUnits" class="field-error">{{ errors.numberOfUnits }}</span>
              </div>
              <div class="form-group" :class="{ 'has-error': errors.numberOfStaircases }">
                <label class="form-label">N° scale *</label>
                <input class="form-input" type="number" min="1" v-model.number="form.numberOfStaircases" @input="clearError('numberOfStaircases')" />
                <span v-if="errors.numberOfStaircases" class="field-error">{{ errors.numberOfStaircases }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">N° piani</label>
                <input class="form-input" type="number" min="1" v-model.number="form.numberOfFloors" />
              </div>
              <div class="form-group" :class="{ 'has-error': errors.yearOfConstruction }">
                <label class="form-label">Anno di costruzione</label>
                <input class="form-input" type="number" min="1700" :max="currentYear" v-model.number="form.yearOfConstruction" @input="clearError('yearOfConstruction')" />
                <span v-if="errors.yearOfConstruction" class="field-error">{{ errors.yearOfConstruction }}</span>
              </div>
              <div class="form-group" :class="{ 'has-error': errors.totalMillesimal }">
                <label class="form-label">Millesimi totali</label>
                <input class="form-input" type="number" min="0" step="0.001" v-model.number="form.totalMillesimal" @input="clearError('totalMillesimal')" />
                <span v-if="errors.totalMillesimal" class="field-error">{{ errors.totalMillesimal }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Mq aree comuni</label>
                <input class="form-input" type="number" min="0" step="0.01" v-model.number="form.commonAreasSqm" />
              </div>
              <div class="form-group" style="grid-column:span 2">
                <div class="check-row">
                  <label class="check-item">
                    <input type="checkbox" v-model="form.hasElevator" />
                    <span>Ascensore</span>
                  </label>
                  <label class="check-item">
                    <input type="checkbox" v-model="form.hasCentralHeating" />
                    <span>Riscaldamento centralizzato</span>
                  </label>
                  <label class="check-item">
                    <input type="checkbox" v-model="form.hasConcierge" />
                    <span>Portineria</span>
                  </label>
                </div>
              </div>
              <div class="form-group" v-if="form.hasElevator">
                <label class="form-label">N° ascensori</label>
                <input class="form-input" type="number" min="1" v-model.number="form.numberOfElevators" />
              </div>
            </div>
          </fieldset>

          <!-- Gestione amministrativa -->
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Gestione amministrativa</legend>
            <div class="form-grid">
              <div class="form-group">
                <label class="form-label">Frequenza rate</label>
                <select class="form-select" v-model="form.installmentFrequency">
                  <option value="Monthly">Mensile</option>
                  <option value="Quarterly">Trimestrale</option>
                  <option value="Biannual">Semestrale</option>
                  <option value="Annual">Annuale</option>
                </select>
              </div>
              <div class="form-group" :class="{ 'has-error': errors.installmentDueDay }">
                <label class="form-label">Giorno scadenza rata *</label>
                <input class="form-input" type="number" min="1" max="31" v-model.number="form.installmentDueDay" @input="clearError('installmentDueDay')" />
                <span v-if="errors.installmentDueDay" class="field-error">{{ errors.installmentDueDay }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Inizio mandato</label>
                <input class="form-input" type="date" v-model="form.mandateStartDate" />
              </div>
              <div class="form-group" :class="{ 'has-error': errors.mandateEndDate }">
                <label class="form-label">Fine mandato</label>
                <input class="form-input" type="date" v-model="form.mandateEndDate" @input="clearError('mandateEndDate')" />
                <span v-if="errors.mandateEndDate" class="field-error">{{ errors.mandateEndDate }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Ultima assemblea</label>
                <input class="form-input" type="date" v-model="form.lastAssemblyDate" />
              </div>
            </div>
          </fieldset>

          <!-- Note e stato -->
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.notes" rows="3"></textarea>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem">
            <input type="checkbox" id="isActive" v-model="form.isActive" />
            <label for="isActive" style="font-size:0.875rem;cursor:pointer">Condominio attivo</label>
          </div>

          <!-- Indirizzo -->
          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Indirizzo</legend>
            <div class="form-grid">
              <div class="form-group" style="grid-column:span 2">
                <label class="form-label">Via / Piazza</label>
                <input class="form-input" v-model="form.address.street" placeholder="Via Roma" />
              </div>
              <div class="form-group">
                <label class="form-label">Civico</label>
                <input class="form-input" v-model="form.address.streetNumber" placeholder="1/A" />
              </div>
              <div class="form-group">
                <label class="form-label">CAP</label>
                <input class="form-input" v-model="form.address.postalCode" placeholder="00100" maxlength="10" />
              </div>
              <div class="form-group">
                <label class="form-label">Città</label>
                <input class="form-input" v-model="form.address.city" placeholder="Roma" />
              </div>
              <div class="form-group">
                <label class="form-label">Provincia</label>
                <input class="form-input" v-model="form.address.province" placeholder="RM" maxlength="2" />
              </div>
              <div class="form-group">
                <label class="form-label">Nazione (cod. ISO)</label>
                <input class="form-input" v-model="form.address.country" placeholder="IT" maxlength="2" style="text-transform:uppercase" />
              </div>
            </div>
          </fieldset>

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
import { ref, reactive, computed, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { condominiumApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const store  = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

// { [condominiumId]: { ok: number, total: number } | null }
const setupStatus = reactive({})
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterActive = ref('')
const errors = ref({})
const currentYear = new Date().getFullYear()

const defaultAddress = () => ({
  street: '', streetNumber: '', postalCode: '', city: '', province: '', country: '',
})
const defaultForm = () => ({
  name: '', code: '', taxCode: '', vatNumber: '',
  email: '', phone: '', pec: '',
  numberOfUnits: 1, numberOfStaircases: 1, numberOfFloors: null,
  yearOfConstruction: null, totalMillesimal: 1000,
  hasElevator: false, numberOfElevators: null,
  hasCentralHeating: false, hasConcierge: false, commonAreasSqm: null,
  mandateStartDate: null, mandateEndDate: null, lastAssemblyDate: null,
  installmentFrequency: 'Monthly', installmentDueDay: 1,
  notes: '', isActive: true,
  address: defaultAddress(),
})
const form = ref(defaultForm())

const filtered = computed(() => {
  let list = store.condomini
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(c => c.name?.toLowerCase().includes(q) || c.code?.toLowerCase().includes(q))
  }
  if (filterActive.value !== '') list = list.filter(c => String(c.isActive) === filterActive.value)
  return list
})

async function loadData() {
  loading.value = true
  await store.loadCondomini()
  loading.value = false
  store.condomini.forEach(c => loadSetupStatus(c.id))
}

function loadSetupStatus(id) {
  setupStatus[id] = null
  condominiumApi.getSetupStatus(id)
    .then(r => {
      const s = r.data
      setupStatus[id] = { ok: s.completedSections, total: s.totalSections }
    })
    .catch(() => { setupStatus[id] = undefined })
}

function openModal(item = null) {
  editing.value = item?.id ?? null
  if (item) {
    form.value = {
      ...item,
      mandateStartDate: toDateInput(item.mandateStartDate),
      mandateEndDate:   toDateInput(item.mandateEndDate),
      lastAssemblyDate: toDateInput(item.lastAssemblyDate),
      address: item.address ? { ...item.address } : defaultAddress(),
    }
  } else {
    form.value = defaultForm()
  }
  errors.value = {}
  showModal.value = true
}

function toDateInput(val) {
  if (!val) return null
  return new Date(val).toISOString().substring(0, 10)
}

function clearError(field) {
  delete errors.value[field]
}

function validate() {
  const e = {}
  const f = form.value

  if (!f.name?.trim())
    e.name = 'Il nome è obbligatorio'

  if (f.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(f.email))
    e.email = 'Indirizzo email non valido'

  if (!f.numberOfUnits || f.numberOfUnits < 1)
    e.numberOfUnits = 'Deve essere almeno 1'

  if (!f.numberOfStaircases || f.numberOfStaircases < 1)
    e.numberOfStaircases = 'Deve essere almeno 1'

  if (f.yearOfConstruction != null && (f.yearOfConstruction < 1700 || f.yearOfConstruction > currentYear + 1))
    e.yearOfConstruction = `Deve essere compreso tra 1700 e ${currentYear + 1}`

  if (f.totalMillesimal != null && f.totalMillesimal < 0)
    e.totalMillesimal = 'Non può essere negativo'

  if (!f.installmentDueDay || f.installmentDueDay < 1 || f.installmentDueDay > 31)
    e.installmentDueDay = 'Deve essere compreso tra 1 e 31'

  if (f.mandateStartDate && f.mandateEndDate && f.mandateEndDate < f.mandateStartDate)
    e.mandateEndDate = 'Non può essere precedente alla data di inizio mandato'

  errors.value = e
  return Object.keys(e).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await condominiumApi.update(editing.value, form.value)
      store.toast('Condominio aggiornato', 'success')
    } else {
      await condominiumApi.create(form.value)
      store.toast('Condominio creato', 'success')
    }
    showModal.value = false
    await loadData()
  } catch (err) {
    // Gli errori HTTP (400, 422, 5xx…) sono già gestiti dall'interceptor via api:error.
    // Mostriamo un toast solo per errori di rete (nessuna risposta dal server).
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

async function deleteItem(id) {
  if (!confirm('Eliminare il condominio?')) return
  try {
    await condominiumApi.delete(id)
    store.toast('Condominio eliminato', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

onMounted(loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; }
.search-input { flex: 1; max-width: 360px; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }

.setup-cell { width: 80px; }
.setup-loading { font-size: 0.8rem; color: var(--text-muted); }
.setup-pill {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.78rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 999px;
  text-decoration: none;
  transition: opacity 0.15s;
}
.setup-pill:hover { opacity: 0.8; }
.setup-ok  { background: #dcfce7; color: #16a34a; }
.setup-ko  { background: #fee2e2; color: #dc2626; }
.setup-icon { font-size: 0.7rem; }

.modal--wide { width: min(760px, 96vw); }

.form-fieldset { border: 1px solid var(--border); border-radius: 6px; padding: 1rem; margin-top: 1rem; }
.form-fieldset-legend { font-size: 0.8125rem; font-weight: 600; color: var(--text-secondary); padding: 0 0.4rem; }

.check-row { display: flex; flex-wrap: wrap; gap: 1.25rem; }
.check-item { display: flex; align-items: center; gap: 0.4rem; font-size: 0.875rem; cursor: pointer; }
.check-item input[type="checkbox"] { cursor: pointer; }

.has-error .form-input,
.has-error .form-select { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; }
</style>
