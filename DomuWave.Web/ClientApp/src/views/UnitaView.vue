<template>
  <div>
    <div class="page-header">
      <h1>Unità Immobiliari</h1>
      <button class="btn btn-primary" @click="openModal()">+ Nuova unità</button>
    </div>

    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca per scala, piano, interno…" />
      <select class="form-select" v-model="filterType" style="width:160px">
        <option value="">Tutti i tipi</option>
        <option value="Residential">Residenziale</option>
        <option value="Commercial">Commerciale</option>
        <option value="Garage">Garage</option>
        <option value="Storage">Cantina</option>
      </select>
      <select class="form-select" v-model="filterStatus" style="width:160px">
        <option value="">Tutti gli stati</option>
        <option value="Occupied">Occupato</option>
        <option value="Vacant">Libero</option>
      </select>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon">⊞</div>
        <div>Nessuna unità trovata</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Scala</th>
              <th>Piano</th>
              <th>Interno</th>
              <th>Tipo</th>
              <th>Superficie</th>
              <th>Vani</th>
              <th>Stato</th>
              <th>Attivo</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in filtered" :key="u.id">
              <td class="mono">{{ u.staircase || '—' }}</td>
              <td>{{ u.floor }}</td>
              <td class="mono">{{ u.internalNumber }}</td>
              <td>{{ typeLabel(u.unitType) }}</td>
              <td class="text-secondary">{{ u.areaSqm ? u.areaSqm + ' m²' : '—' }}</td>
              <td class="text-secondary">{{ u.rooms || '—' }}</td>
              <td><span class="badge" :class="u.occupancyStatus === 'Occupied' ? 'badge-amber' : 'badge-blue'">{{ u.occupancyStatus === 'Occupied' ? 'Occupato' : 'Libero' }}</span></td>
              <td><span class="badge" :class="u.isActive ? 'badge-green' : 'badge-muted'">{{ u.isActive ? 'Sì' : 'No' }}</span></td>
              <td>
                <div class="row-actions">
                  <button class="btn-icon" @click="openModal(u)">✎</button>
                  <button class="btn-icon" @click="deleteItem(u.id)" style="color:var(--accent-red)">✕</button>
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
          <h2>{{ editing ? 'Modifica' : 'Nuova' }} unità immobiliare</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Scala</label>
              <input class="form-input" v-model="form.staircase" placeholder="A" />
            </div>
            <div class="form-group">
              <label class="form-label">Piano *</label>
              <input class="form-input" type="number" v-model.number="form.floor" />
            </div>
            <div class="form-group">
              <label class="form-label">Interno *</label>
              <input class="form-input" v-model="form.internalNumber" placeholder="01" />
            </div>
            <div class="form-group">
              <label class="form-label">Subalterno</label>
              <input class="form-input" v-model="form.subordinate" />
            </div>
            <div class="form-group">
              <label class="form-label">Categoria catastale</label>
              <input class="form-input" v-model="form.category" placeholder="A/2" />
            </div>
            <div class="form-group">
              <label class="form-label">Rendita catastale (€)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="form.cadastralIncome" />
            </div>
            <div class="form-group">
              <label class="form-label">Superficie (m²)</label>
              <input class="form-input" type="number" step="0.01" v-model.number="form.areaSqm" />
            </div>
            <div class="form-group">
              <label class="form-label">Vani</label>
              <input class="form-input" type="number" step="0.5" v-model.number="form.rooms" />
            </div>
            <div class="form-group">
              <label class="form-label">Tipo unità</label>
              <select class="form-select" v-model="form.unitType">
                <option value="Residential">Residenziale</option>
                <option value="Commercial">Commerciale</option>
                <option value="Garage">Garage</option>
                <option value="Storage">Cantina</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Stato occupazione</label>
              <select class="form-select" v-model="form.occupancyStatus">
                <option value="Occupied">Occupato</option>
                <option value="Vacant">Libero</option>
              </select>
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.notes" rows="2"></textarea>
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
import { ref, computed, onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'
import { unitApi } from '@/services/api'

const store = useAppStore()
const units = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editing = ref(null)
const search = ref('')
const filterType = ref('')
const filterStatus = ref('')

const typeLabels = { Residential: 'Residenziale', Commercial: 'Commerciale', Garage: 'Garage', Storage: 'Cantina' }
function typeLabel(t) { return typeLabels[t] || t }

const defaultForm = () => ({
  condominiumId: store.selectedCondominioId,
  staircase: '', floor: 0, internalNumber: '', subordinate: '',
  category: '', cadastralIncome: null, areaSqm: null, rooms: null,
  unitType: 'Residential', occupancyStatus: 'Occupied', notes: '', isActive: true
})
const form = ref(defaultForm())

const filtered = computed(() => {
  let list = units.value
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(u =>
      u.staircase?.toLowerCase().includes(q) ||
      u.internalNumber?.toLowerCase().includes(q) ||
      String(u.floor).includes(q)
    )
  }
  if (filterType.value)   list = list.filter(u => u.unitType === filterType.value)
  if (filterStatus.value) list = list.filter(u => u.occupancyStatus === filterStatus.value)
  return list
})

async function loadData() {
  if (!store.selectedCondominioId) return
  loading.value = true
  try {
    const { data } = await unitApi.getByCondominium(store.selectedCondominioId)
    units.value = data
  } catch { units.value = [] } finally { loading.value = false }
}

function openModal(item = null) {
  editing.value = item?.id ?? null
  form.value = item ? { ...item } : defaultForm()
  showModal.value = true
}

async function save() {
  if (!form.value.internalNumber) return store.toast('L\'interno è obbligatorio', 'error')
  saving.value = true
  try {
    if (editing.value) {
      await unitApi.update(editing.value, form.value)
      store.toast('Unità aggiornata', 'success')
    } else {
      await unitApi.create({ ...form.value, condominiumId: store.selectedCondominioId })
      store.toast('Unità creata', 'success')
    }
    showModal.value = false
    await loadData()
  } catch { store.toast('Errore durante il salvataggio', 'error') }
  finally { saving.value = false }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questa unità?')) return
  try {
    await unitApi.delete(id)
    store.toast('Unità eliminata', 'success')
    await loadData()
  } catch { store.toast('Errore', 'error') }
}

watch(() => store.selectedCondominioId, loadData)
onMounted(loadData)
</script>

<style scoped>
.toolbar { display: flex; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
.search-input { flex: 1; min-width: 200px; max-width: 320px; }
.row-actions { display: flex; gap: 0.4rem; justify-content: flex-end; }
</style>
