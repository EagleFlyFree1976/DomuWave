<template>
  <div class="scale-page">

    <!-- Pannello genera scale -->
    <div class="generate-panel" v-if="canCreate && condominiumId">
      <div class="generate-panel__icon">🪜</div>
      <div class="generate-panel__body">
        <div class="generate-panel__title">Genera scale automaticamente</div>
        <div class="generate-panel__desc text-secondary">
          Inserisci il numero di scale da creare — verranno aggiunte solo quelle non ancora esistenti.
        </div>
      </div>
      <div class="generate-panel__action">
        <input
          type="number"
          class="form-input"
          v-model.number="generateCount"
          min="1"
          max="99"
          placeholder="N°"
          style="width:80px;text-align:center"
        />
        <button class="btn btn-primary" @click="generateScales" :disabled="generating || !generateCount || generateCount < 1">
          <span v-if="generating" class="spinner" style="width:14px;height:14px"></span>
          <span v-else>⚡</span>
          Genera
        </button>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="toolbar">
      <input class="form-input search-input" v-model="search" placeholder="Cerca scala…" style="max-width:280px" />
      <button v-if="canCreate && condominiumId" class="btn btn-primary" style="margin-left:auto" @click="openModal()">+ Nuova scala</button>
    </div>

    <!-- Lista -->
    <div class="card">
      <div v-if="loading" class="loading-state"><div class="spinner"></div> Caricamento…</div>
      <div v-else-if="!filtered.length" class="empty-state">
        <div class="empty-icon">🪜</div>
        <div>Nessuna scala trovata</div>
        <div class="text-secondary" style="font-size:0.8125rem;margin-top:0.25rem">
          Usa il pannello sopra per generarle in automatico
        </div>
      </div>
      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th style="width:3rem">#</th>
              <th>Nome scala</th>
              <th>Stato</th>
              <th style="width:6rem"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(s, i) in filtered" :key="s.id">
              <td class="text-muted mono" style="font-size:0.8rem">{{ i + 1 }}</td>
              <td>
                <span class="scala-badge">{{ s.name }}</span>
              </td>
              <td>
                <span class="badge" :class="s.isActive ? 'badge-green' : 'badge-muted'">
                  {{ s.isActive ? 'Attiva' : 'Inattiva' }}
                </span>
              </td>
              <td>
                <div class="row-actions">
                  <button v-if="canEdit" class="btn-icon" @click="openModal(s)" title="Modifica">✎</button>
                  <button v-if="canDelete" class="btn-icon" @click="deleteItem(s.id)" title="Elimina" style="color:var(--accent-red)">✕</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal crea/modifica -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
      <div class="modal" style="max-width:420px">
        <div class="modal-header">
          <h2>{{ editing ? 'Modifica scala' : 'Nuova scala' }}</h2>
          <button class="btn-icon" @click="showModal=false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group" :class="{ 'has-error': errors.name }">
            <label class="form-label">Nome scala *</label>
            <input
              class="form-input"
              v-model="form.name"
              placeholder="Es. A oppure Scala 1"
              maxlength="50"
              @input="clearError('name')"
              autofocus
            />
            <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem;margin-top:0.75rem">
            <input type="checkbox" id="scalaIsActive" v-model="form.isActive" />
            <label for="scalaIsActive" style="font-size:0.875rem;cursor:pointer;margin:0">Scala attiva</label>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
          <button class="btn btn-primary" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            {{ editing ? 'Salva modifiche' : 'Crea scala' }}
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
import { usePermissions } from '@/composables/usePermissions'
import { staircaseApi } from '@/services/api'

const route = useRoute()
const store = useAppStore()
const { canCreate, canEdit, canDelete } = usePermissions()

const condominiumId = computed(() => Number(route.params.id) || store.selectedCondominioId)

const loading       = ref(false)
const saving        = ref(false)
const generating    = ref(false)
const showModal     = ref(false)
const editing       = ref(null)
const search        = ref('')
const errors        = ref({})
const items         = ref([])
const generateCount = ref(null)

const defaultForm = () => ({ name: '', isActive: true })
const form = ref(defaultForm())

const filtered = computed(() => {
  if (!search.value) return items.value
  const q = search.value.toLowerCase()
  return items.value.filter(s => s.name?.toLowerCase().includes(q))
})

const existingNames = computed(() => new Set(items.value.map(s => s.name?.toUpperCase())))

function buildNames(count) {
  const existing = existingNames.value
  const names    = []
  const letters  = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'

  for (let i = 0; i < letters.length && names.length < count; i++) {
    if (!existing.has(letters[i])) names.push(letters[i])
  }
  let n = 1
  while (names.length < count) {
    const candidate = String(n)
    if (!existing.has(candidate)) names.push(candidate)
    n++
  }
  return names
}

async function loadData() {
  if (!condominiumId.value) { items.value = []; return }
  loading.value = true
  try {
    const { data } = await staircaseApi.getByCondominium(condominiumId.value)
    items.value = data ?? []
  } catch {
  } finally {
    loading.value = false
  }
}

async function generateScales() {
  const count = generateCount.value
  if (!count || count < 1) return
  if (!confirm(`Verranno create fino a ${count} scale (quelle già esistenti saranno saltate). Continuare?`)) return
  generating.value = true
  try {
    const names = buildNames(count)
    for (const name of names) {
      await staircaseApi.create({ name, isActive: true, condominiumId: condominiumId.value })
    }
    store.toast(`${names.length} scale create con successo`, 'success')
    generateCount.value = null
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    generating.value = false
  }
}

function openModal(item = null) {
  editing.value   = item?.id ?? null
  form.value      = item ? { name: item.name, isActive: item.isActive } : defaultForm()
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
      await staircaseApi.update(editing.value, form.value)
      store.toast('Scala aggiornata', 'success')
    } else {
      await staircaseApi.create({ ...form.value, condominiumId: condominiumId.value })
      store.toast('Scala creata', 'success')
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
  if (!confirm('Eliminare questa scala?')) return
  try {
    await staircaseApi.delete(id)
    store.toast('Scala eliminata', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

onMounted(loadData)
watch(condominiumId, loadData)
</script>

<style scoped>
.scale-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* ── Pannello genera ── */
.generate-panel {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1rem 1.25rem;
  flex-wrap: wrap;
}

.generate-panel__icon {
  font-size: 2rem;
  line-height: 1;
  flex-shrink: 0;
}

.generate-panel__body {
  flex: 1;
  min-width: 0;
}

.generate-panel__title {
  font-weight: 600;
  font-size: 0.9375rem;
  color: var(--text);
  margin-bottom: 0.2rem;
}

.generate-panel__desc {
  font-size: 0.8125rem;
}

.generate-panel__action {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

/* ── Badge scala ── */
.scala-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 2rem;
  height: 2rem;
  padding: 0 0.6rem;
  background: var(--accent-glow);
  border: 1px solid var(--border-active);
  border-radius: 6px;
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text);
  letter-spacing: 0.02em;
}
</style>
