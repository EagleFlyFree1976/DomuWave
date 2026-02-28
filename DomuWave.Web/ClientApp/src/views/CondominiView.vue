<template>
  <div>
    <div class="page-header">
      <h1>Condomini</h1>
      <button class="btn btn-primary" @click="openModal()">+ Nuovo condominio</button>
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
              <td>
                <div class="row-actions">
                  <button class="btn-icon" @click="openModal(c)" title="Modifica">✎</button>
                  <button class="btn-icon" @click="deleteItem(c.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica' : 'Nuovo' }} condominio</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
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
              <input class="form-input" v-model="form.taxCode" />
            </div>
            <div class="form-group">
              <label class="form-label">Partita IVA</label>
              <input class="form-input" v-model="form.vatNumber" />
            </div>
            <div class="form-group" :class="{ 'has-error': errors.email }">
              <label class="form-label">Email</label>
              <input class="form-input" type="email" v-model="form.email" @input="clearError('email')" />
              <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Telefono</label>
              <input class="form-input" v-model="form.phone" />
            </div>
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
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.notes" rows="3"></textarea>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem">
            <input type="checkbox" id="isActive" v-model="form.isActive" />
            <label for="isActive" style="font-size:0.875rem;cursor:pointer">Condominio attivo</label>
          </div>
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
                <input class="form-input" v-model="form.address.postalCode" placeholder="00100" />
              </div>
              <div class="form-group">
                <label class="form-label">Città</label>
                <input class="form-input" v-model="form.address.city" placeholder="Roma" />
              </div>
              <div class="form-group">
                <label class="form-label">Provincia</label>
                <input class="form-input" v-model="form.address.province" placeholder="RM" maxlength="2" />
              </div>
              <div class="form-group" style="grid-column:span 2">
                <label class="form-label">Nazione</label>
                <input class="form-input" v-model="form.address.country" placeholder="Italia" />
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
import { ref, computed, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { condominiumApi } from '@/services/api'

const store = useAppStore()
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterActive = ref('')
const errors = ref({})

const defaultAddress = () => ({ street: '', streetNumber: '', postalCode: '', city: '', province: '', country: '' })
const defaultForm = () => ({
  name: '', code: '', taxCode: '', vatNumber: '', email: '', phone: '', pec: '',
  numberOfUnits: 0, numberOfStaircases: 1, installmentFrequency: 'Monthly',
  installmentDueDay: 1, notes: '', isActive: true,
  hasElevator: false, hasCentralHeating: false, hasConcierge: false, totalMillesimal: 1000,
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
}

function openModal(item = null) {
  editing.value = item?.id ?? null
  form.value = item
    ? { ...item, address: item.address ? { ...item.address } : defaultAddress() }
    : defaultForm()
  errors.value = {}
  showModal.value = true
}

function clearError(field) {
  delete errors.value[field]
}

function validate() {
  const e = {}
  if (!form.value.name?.trim())
    e.name = 'Il nome è obbligatorio'
  if (form.value.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email))
    e.email = 'Indirizzo email non valido'
  if (!form.value.numberOfUnits || form.value.numberOfUnits < 1)
    e.numberOfUnits = 'Deve essere almeno 1'
  if (!form.value.numberOfStaircases || form.value.numberOfStaircases < 1)
    e.numberOfStaircases = 'Deve essere almeno 1'
  if (!form.value.installmentDueDay || form.value.installmentDueDay < 1 || form.value.installmentDueDay > 31)
    e.installmentDueDay = 'Deve essere compreso tra 1 e 31'
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
  } catch {
    store.toast('Errore durante il salvataggio', 'error')
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
  } catch {
    store.toast('Errore durante l\'eliminazione', 'error')
  }
}

onMounted(loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; }
.search-input { flex: 1; max-width: 360px; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
.form-fieldset { border: 1px solid var(--border); border-radius: 6px; padding: 1rem; margin-top: 1rem; }
.form-fieldset-legend { font-size: 0.8125rem; font-weight: 600; color: var(--text-secondary); padding: 0 0.4rem; }

.has-error .form-input,
.has-error .form-select {
  border-color: var(--accent-red, #e53e3e);
}
.field-error {
  font-size: 0.78rem;
  color: var(--accent-red, #e53e3e);
  margin-top: 0.2rem;
}
</style>
