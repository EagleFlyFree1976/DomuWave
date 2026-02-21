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
            <div class="form-group">
              <label class="form-label">Nome *</label>
              <input class="form-input" v-model="form.name" placeholder="Es. Condominio Roma" />
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
            <div class="form-group">
              <label class="form-label">Email</label>
              <input class="form-input" type="email" v-model="form.email" />
            </div>
            <div class="form-group">
              <label class="form-label">Telefono</label>
              <input class="form-input" v-model="form.phone" />
            </div>
            <div class="form-group">
              <label class="form-label">N° unità</label>
              <input class="form-input" type="number" v-model.number="form.numberOfUnits" />
            </div>
            <div class="form-group">
              <label class="form-label">N° scale</label>
              <input class="form-input" type="number" v-model.number="form.numberOfStaircases" />
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
            <div class="form-group">
              <label class="form-label">Giorno scadenza rata</label>
              <input class="form-input" type="number" min="1" max="31" v-model.number="form.installmentDueDay" />
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

const defaultForm = () => ({
  name: '', code: '', taxCode: '', vatNumber: '', email: '', phone: '', pec: '',
  numberOfUnits: 0, numberOfStaircases: 1, installmentFrequency: 'Monthly',
  installmentDueDay: 1, notes: '', isActive: true,
  hasElevator: false, hasCentralHeating: false, hasConcierge: false, totalMillesimal: 1000
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
  form.value = item ? { ...item } : defaultForm()
  showModal.value = true
}

async function save() {
  if (!form.value.name) return store.toast('Il nome è obbligatorio', 'error')
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
</style>
