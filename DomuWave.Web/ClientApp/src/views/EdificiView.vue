<template>
  <div>
    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca edificio…" />
      <button v-if="canCreate && store.selectedCondominioId" class="btn btn-primary" style="margin-left:auto" @click="openModal()">+ Nuovo edificio</button>
    </div>

    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon">🏢</div>
        <div>Nessun edificio trovato</div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Nome</th>
              <th>Codice</th>
              <th>Indirizzo</th>
              <th>Anno costr.</th>
              <th>Piani</th>
              <th>Ascensore</th>
              <th class="text-center">Unità</th>
              <th>Stato</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="b in filtered" :key="b.id">
              <td style="font-weight:500">{{ b.name }}</td>
              <td class="mono text-secondary">{{ b.code || '—' }}</td>
              <td class="text-secondary">{{ b.address || '—' }}</td>
              <td class="text-secondary">{{ b.yearOfConstruction || '—' }}</td>
              <td class="text-secondary">{{ b.numberOfFloors != null ? b.numberOfFloors : '—' }}</td>
              <td>
                <span class="badge" :class="b.hasElevator ? 'badge-green' : 'badge-muted'">
                  {{ b.hasElevator ? 'Sì' : 'No' }}
                </span>
              </td>
              <td class="text-center text-secondary">{{ b.unitCount }}</td>
              <td>
                <span class="badge" :class="b.isActive ? 'badge-green' : 'badge-muted'">
                  {{ b.isActive ? 'Attivo' : 'Inattivo' }}
                </span>
              </td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit" class="btn-icon" @click="openModal(b)" title="Modifica">✎</button>
                  <button v-if="canDelete" class="btn-icon" @click="deleteItem(b.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
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
          <h2>{{ editing ? 'Modifica' : 'Nuovo' }} edificio</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">

          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Dati generali</legend>
            <div class="form-grid">
              <div class="form-group form-group--full" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="form.name" placeholder="Es. Corpo A" @input="clearError('name')" />
                <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Codice</label>
                <input class="form-input" v-model="form.code" placeholder="Es. A" maxlength="50" />
              </div>
              <div class="form-group">
                <label class="form-label">Anno costruzione</label>
                <input class="form-input" type="number" min="1800" max="2100" v-model.number="form.yearOfConstruction" />
              </div>
              <div class="form-group form-group--full">
                <label class="form-label">Indirizzo</label>
                <input class="form-input" v-model="form.address" placeholder="Via, numero civico…" />
              </div>
              <div class="form-group">
                <label class="form-label">Numero piani</label>
                <input class="form-input" type="number" min="1" v-model.number="form.numberOfFloors" />
              </div>
            </div>
          </fieldset>

          <div style="display:flex;gap:1.5rem;margin-top:0.5rem">
            <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem">
              <input type="checkbox" id="buildingHasElevator" v-model="form.hasElevator" />
              <label for="buildingHasElevator" style="font-size:0.875rem;cursor:pointer">Ascensore</label>
            </div>
            <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem">
              <input type="checkbox" id="buildingIsActive" v-model="form.isActive" />
              <label for="buildingIsActive" style="font-size:0.875rem;cursor:pointer">Attivo</label>
            </div>
          </div>

          <div class="form-group" style="margin-top:0.5rem">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.description" rows="2"></textarea>
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
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { buildingApi } from '@/services/api'
import { usePermissions } from '@/composables/usePermissions'

const route  = useRoute()
const store  = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const condominiumId = computed(() => Number(route.params.id) || store.selectedCondominioId)

const loading   = ref(false)
const saving    = ref(false)
const showModal = ref(false)
const editing   = ref(null)
const search    = ref('')
const errors    = ref({})
const buildings = ref([])

const defaultForm = () => ({
  condominiumId:     condominiumId.value,
  name:              '',
  description:       '',
  code:              '',
  address:           '',
  yearOfConstruction: null,
  numberOfFloors:    null,
  hasElevator:       false,
  isActive:          true,
})
const form = ref(defaultForm())

const filtered = computed(() => {
  if (!search.value) return buildings.value
  const q = search.value.toLowerCase()
  return buildings.value.filter(b =>
    b.name?.toLowerCase().includes(q) ||
    b.code?.toLowerCase().includes(q) ||
    b.address?.toLowerCase().includes(q)
  )
})

async function loadData() {
  if (!condominiumId.value) { buildings.value = []; return }
  loading.value = true
  try {
    const { data } = await buildingApi.getByCondominium(condominiumId.value)
    buildings.value = data ?? []
  } catch {
    // handled by interceptor
  } finally {
    loading.value = false
  }
}

function openModal(item = null) {
  editing.value   = item?.id ?? null
  form.value      = item ? { ...item, condominiumId: condominiumId.value } : defaultForm()
  errors.value    = {}
  showModal.value = true
}

function clearError(field) { delete errors.value[field] }

function validate() {
  const e = {}
  if (!form.value.name?.trim()) e.name = 'Il nome è obbligatorio'
  errors.value = e
  return Object.keys(e).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await buildingApi.update(editing.value, form.value)
      store.toast('Edificio aggiornato', 'success')
    } else {
      await buildingApi.create(form.value)
      store.toast('Edificio creato', 'success')
    }
    showModal.value = false
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

async function deleteItem(id) {
  if (!confirm('Eliminare questo edificio?')) return
  try {
    await buildingApi.delete(id)
    store.toast('Edificio eliminato', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

onMounted(loadData)
watch(condominiumId, loadData)
</script>
