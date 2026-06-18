<template>
  <div class="auth-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-shield page-title__icon" />
          Autorizzazioni
        </h1>
        <span class="page-subtitle">Gestione risorse, ruoli e permessi del modulo DomuWeb</span>
      </div>
    </div>

    <!-- ── MASTER-DETAIL ──────────────────────────────────────────────────── -->
    <div class="master-detail">

      <!-- ════════ MASTER (sinistra) ════════ -->
      <aside class="master">
        <div class="master__tabs">
          <button class="seg-btn" :class="{ 'seg-btn--active': masterTab === 'resources' }"
                  @click="masterTab = 'resources'">
            <i class="pi pi-key" /> Risorse
          </button>
          <button class="seg-btn" :class="{ 'seg-btn--active': masterTab === 'roles' }"
                  @click="masterTab = 'roles'">
            <i class="pi pi-id-card" /> Ruoli
          </button>
          <button class="seg-btn" :class="{ 'seg-btn--active': masterTab === 'groups' }"
                  @click="masterTab = 'groups'">
            <i class="pi pi-users" /> Gruppi
          </button>
        </div>

        <div class="master__search">
          <span class="p-input-icon-left w-full">
            <i class="pi pi-search" />
            <InputText v-model="masterSearch" placeholder="Cerca..." class="filter-input" />
          </span>
          <Button v-if="masterTab === 'resources'"
                  icon="pi pi-plus" class="btn-primary btn-new"
                  v-tooltip.left="'Nuova risorsa'"
                  @click="newResource" />
          <Button v-else-if="masterTab === 'roles'"
                  icon="pi pi-plus" class="btn-primary btn-new"
                  v-tooltip.left="'Nuovo ruolo'"
                  @click="openNewRole" />
          <Button v-else icon="pi pi-refresh" class="btn-ghost btn-new"
                  v-tooltip.left="'Ricarica'"
                  :loading="loadingGroups"
                  @click="reloadMaster" />
        </div>

        <div class="master__list">
          <!-- Loading -->
          <div v-if="masterLoading" class="list-loading">
            <i class="pi pi-spinner pi-spin" /> Caricamento...
          </div>

          <!-- Empty -->
          <div v-else-if="masterItems.length === 0" class="list-empty">
            <i class="pi pi-inbox" />
            <span>Nessun elemento</span>
          </div>

          <!-- Items -->
          <button v-for="it in masterItems" :key="masterTab + '-' + it.id"
                  class="list-item"
                  :class="{ 'list-item--active': isSelected(it) }"
                  @click="selectItem(it)">
            <span class="list-item__code">{{ it.code }}</span>
            <span class="list-item__desc">{{ it.description }}</span>
            <span v-if="masterTab === 'resources' && it.moduleCode" class="list-item__tag">{{ it.moduleCode }}</span>
          </button>
        </div>
      </aside>

      <!-- ════════ DETAIL (destra) ════════ -->
      <section class="detail">

        <!-- ── Nessuna selezione ── -->
        <div v-if="!selected" class="detail__placeholder">
          <i class="pi pi-arrow-left" />
          <span>Seleziona un elemento dalla lista per gestirlo</span>
        </div>

        <!-- ── DETTAGLIO RISORSA (form CRUD) ── -->
        <div v-else-if="selected.type === 'auth'" class="detail__pane">
          <div class="detail__header">
            <div class="detail__title">
              <i class="pi pi-key" />
              <span>{{ editingAuthId ? 'Modifica risorsa' : 'Nuova risorsa' }}</span>
            </div>
            <Button v-if="editingAuthId"
                    icon="pi pi-trash" label="Elimina"
                    class="btn-danger"
                    :loading="deletingAuth"
                    @click="deleteResource" />
          </div>

          <div class="detail__body form-pane">
            <div class="field">
              <label class="field__label">Codice <span class="required">*</span></label>
              <InputText v-model="authForm.code"
                         :disabled="!!editingAuthId"
                         placeholder="es. CondominiumReport"
                         class="field__input"
                         @input="authErrors.code = null" />
              <small v-if="editingAuthId" class="field__hint">Il codice non è modificabile dopo la creazione.</small>
              <small v-if="authErrors.code" class="field__error">{{ authErrors.code }}</small>
            </div>

            <div class="field">
              <label class="field__label">Descrizione <span class="required">*</span></label>
              <InputText v-model="authForm.description"
                         placeholder="Descrizione leggibile della risorsa"
                         class="field__input"
                         @input="authErrors.description = null" />
              <small v-if="authErrors.description" class="field__error">{{ authErrors.description }}</small>
            </div>

            <div class="field">
              <label class="field__label">Area <span class="required">*</span></label>
              <Select v-model="authForm.areaId"
                      :options="areas"
                      optionLabel="label"
                      optionValue="id"
                      placeholder="Seleziona un'area..."
                      class="field__input"
                      filter
                      @change="authErrors.areaId = null" />
              <small v-if="authErrors.areaId" class="field__error">{{ authErrors.areaId }}</small>
            </div>

            <div class="form-actions">
              <Button :label="editingAuthId ? 'Salva modifiche' : 'Crea risorsa'"
                      icon="pi pi-check"
                      class="btn-primary"
                      :loading="savingAuth"
                      @click="saveResource" />
            </div>
          </div>
        </div>

        <!-- ── DETTAGLIO RUOLO/GRUPPO (matrice permessi) ── -->
        <div v-else class="detail__pane">
          <div class="detail__header">
            <div class="detail__title">
              <i :class="selected.type === 'role' ? 'pi pi-id-card' : 'pi pi-users'" />
              <span>Permessi — {{ selected.item.description || selected.item.code }}</span>
            </div>
            <div class="detail__actions">
              <Button v-if="selected.type === 'role'"
                      icon="pi pi-pencil" label="Modifica"
                      class="btn-ghost btn-sm"
                      v-tooltip.bottom="'Modifica codice e descrizione del ruolo'"
                      @click="openEditRole" />
              <Button v-if="selected.type === 'role'"
                      icon="pi pi-copy" label="Copia da..."
                      class="btn-ghost btn-sm"
                      v-tooltip.bottom="'Copia i permessi da un altro ruolo'"
                      @click="openCopyPerms" />
              <Button v-if="selected.type === 'role'"
                      icon="pi pi-clone" label="Clona"
                      class="btn-ghost btn-sm"
                      v-tooltip.bottom="'Crea un nuovo ruolo copiando questo'"
                      @click="openCloneRole" />
              <Button icon="pi pi-plus" label="Aggiungi risorsa"
                      class="btn-primary btn-sm"
                      :disabled="availableAuthCodes.length === 0"
                      @click="openAddRow" />
            </div>
          </div>

          <div class="detail__body detail__body--matrix">

            <div v-if="loadingExisting" class="list-loading">
              <i class="pi pi-spinner pi-spin" /> Caricamento...
            </div>
            <div v-else-if="existingPermissions.length === 0" class="existing-empty">
              <i class="pi pi-info-circle" />
              <span>Nessun permesso assegnato a questo {{ selected.type === 'role' ? 'ruolo' : 'gruppo' }}</span>
            </div>

            <template v-else>
              <!-- Ricerca permessi -->
              <div class="matrix-search">
                <span class="p-input-icon-left w-full">
                  <i class="pi pi-search" />
                  <InputText v-model="permSearch" placeholder="Cerca risorsa..." class="filter-input" />
                </span>
                <span class="matrix-count">{{ filteredPermissions.length }} / {{ existingPermissions.length }}</span>
              </div>

              <!-- Matrice -->
              <div class="matrix">
                <div class="matrix__head">
                  <span class="matrix__h-res">Risorsa</span>
                  <span class="matrix__h-col" v-tooltip.top="'Visualizza'"><i class="pi pi-eye" /></span>
                  <span class="matrix__h-col" v-tooltip.top="'Crea'"><i class="pi pi-plus-circle" /></span>
                  <span class="matrix__h-col" v-tooltip.top="'Modifica'"><i class="pi pi-pencil" /></span>
                  <span class="matrix__h-col" v-tooltip.top="'Elimina'"><i class="pi pi-trash" /></span>
                  <span class="matrix__h-col" v-tooltip.top="'Azione'"><i class="pi pi-bolt" /></span>
                  <span class="matrix__h-act"></span>
                </div>

                <div v-for="perm in filteredPermissions" :key="perm.id ?? perm.authCode"
                     class="matrix__row" :class="{ 'matrix__row--saving': !!savingPerm[perm.id] }">
                  <div class="matrix__res">
                    <span class="matrix__code">{{ perm.authCode }}</span>
                    <span v-if="perm.authDescription" class="matrix__desc">{{ perm.authDescription }}</span>
                  </div>

                  <button v-for="key in permKeys" :key="key"
                          class="cell" :class="{ 'cell--on': perm.can[key], 'cell--off': !perm.can[key] }"
                          :disabled="!!savingPerm[perm.id]"
                          @click="togglePerm(perm, key)">
                    <i v-if="perm.can[key]" class="pi pi-check" />
                  </button>

                  <div class="matrix__act">
                    <i v-if="savingPerm[perm.id]" class="pi pi-spinner pi-spin matrix__saving" />
                    <Button v-else-if="perm.id" icon="pi pi-trash"
                            class="btn-icon-sm btn-icon-sm--danger"
                            v-tooltip.left="'Rimuovi risorsa'"
                            :loading="!!deleting[perm.id]"
                            @click="deletePermission(perm, existingPermissions)" />
                  </div>
                </div>

                <div v-if="filteredPermissions.length === 0 && matchingUnassigned.length === 0" class="matrix__empty">
                  Nessuna risorsa corrisponde a “{{ permSearch }}”.
                </div>
              </div>

              <!-- Risorse esistenti ma NON ancora assegnate che combaciano con la ricerca -->
              <div v-if="matchingUnassigned.length" class="unassigned">
                <span class="unassigned__label">
                  <i class="pi pi-plus-circle" /> Risorse disponibili non assegnate
                </span>
                <div v-for="res in matchingUnassigned" :key="res.code" class="unassigned__row">
                  <div class="matrix__res">
                    <span class="matrix__code">{{ res.code }}</span>
                    <span v-if="res.label && res.label !== res.code" class="matrix__desc">{{ res.label }}</span>
                  </div>
                  <Button icon="pi pi-plus" label="Aggiungi"
                          class="btn-primary btn-sm"
                          :loading="!!addingCode[res.code]"
                          @click="addResourceByCode(res.code)" />
                </div>
              </div>
            </template>
          </div>
        </div>

      </section>
    </div>

    <!-- ── DIALOG: aggiungi risorsa a ruolo/gruppo ──────────────────────────── -->
    <Dialog v-model:visible="addRowVisible"
            header="Aggiungi risorsa"
            :modal="true" :draggable="false"
            style="width: min(440px, 95vw)">
      <div class="field">
        <label class="field__label">Risorsa <span class="required">*</span></label>
        <Select v-model="addRowCode"
                :options="availableAuthCodes"
                optionLabel="label"
                optionValue="code"
                placeholder="Seleziona una risorsa..."
                class="field__input"
                filter />
        <small class="field__hint">Verrà aggiunta con la sola <b>Visualizza</b>; regola gli altri livelli dalla matrice.</small>
      </div>
      <template #footer>
        <Button label="Annulla" class="btn-ghost" @click="addRowVisible = false" />
        <Button label="Aggiungi" icon="pi pi-plus" class="btn-primary"
                :loading="adding" :disabled="!addRowCode || adding"
                @click="confirmAddRow" />
      </template>
    </Dialog>

    <!-- ── DIALOG: nuovo / clona / modifica ruolo ───────────────────────────── -->
    <Dialog v-model:visible="roleDialogVisible"
            :header="roleDialogTitle"
            :modal="true" :draggable="false"
            style="width: min(460px, 95vw)">
      <p v-if="roleDialogMode === 'clone'" class="dialog-hint">
        Crea un nuovo ruolo copiando codice di partenza e <b>tutti i permessi</b> di
        «{{ roleSource?.description || roleSource?.code }}».
      </p>
      <div class="field">
        <label class="field__label">Codice <span class="required">*</span></label>
        <InputText v-model="roleForm.code" placeholder="es. Contabile" class="field__input"
                   @input="roleErrors.code = null" />
        <small v-if="roleDialogMode === 'edit'" class="field__hint">
          Il codice è univoco; modificandolo potresti incidere su integrazioni che lo referenziano.
        </small>
        <small v-if="roleErrors.code" class="field__error">{{ roleErrors.code }}</small>
      </div>
      <div class="field">
        <label class="field__label">Descrizione</label>
        <InputText v-model="roleForm.description" placeholder="Descrizione del ruolo" class="field__input" />
      </div>
      <div v-if="roleDialogMode !== 'edit'" class="field">
        <label class="field__label">Modulo</label>
        <Select v-model="roleForm.moduleCode"
                :options="modules"
                optionLabel="label"
                optionValue="code"
                placeholder="Seleziona un modulo..."
                class="field__input" />
        <small class="field__hint">Determina in quale modulo comparirà il ruolo (predefinito: DomuWeb).</small>
      </div>
      <template #footer>
        <Button label="Annulla" class="btn-ghost" @click="roleDialogVisible = false" />
        <Button :label="roleDialogConfirmLabel" :icon="roleDialogConfirmIcon"
                class="btn-primary"
                :loading="savingRole" :disabled="savingRole"
                @click="confirmRoleDialog" />
      </template>
    </Dialog>

    <!-- ── DIALOG: copia permessi da un altro ruolo ─────────────────────────── -->
    <Dialog v-model:visible="copyPermsVisible"
            header="Copia permessi da un altro ruolo"
            :modal="true" :draggable="false"
            style="width: min(460px, 95vw)">
      <p class="dialog-hint">
        I permessi del ruolo scelto verranno <b>uniti</b> a quelli di
        «{{ selected?.item?.description || selected?.item?.code }}»: le risorse in comune
        vengono sovrascritte, le altre restano invariate.
      </p>
      <div class="field">
        <label class="field__label">Ruolo di origine <span class="required">*</span></label>
        <Select v-model="copySourceId"
                :options="otherRoles"
                optionLabel="label"
                optionValue="id"
                placeholder="Seleziona un ruolo..."
                class="field__input"
                filter />
      </div>
      <template #footer>
        <Button label="Annulla" class="btn-ghost" @click="copyPermsVisible = false" />
        <Button label="Copia permessi" icon="pi pi-copy" class="btn-primary"
                :loading="copying" :disabled="!copySourceId || copying"
                @click="confirmCopyPerms" />
      </template>
    </Dialog>

  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'

import InputText from 'primevue/inputtext'
import Button    from 'primevue/button'
import Select    from 'primevue/select'
import Dialog    from 'primevue/dialog'

import { rolesAuthApi, groupsAuthApi, authorizationsApi } from '@/services/authorizationService'

const toast = useToast()

// ─── MASTER STATE ──────────────────────────────────────────────────────────
const masterTab    = ref('resources')   // 'resources' | 'roles' | 'groups'
const masterSearch = ref('')

const resources     = ref([])
const roles         = ref([])
const groups        = ref([])
const areas         = ref([])
const authCodes     = ref([])

const loadingResources = ref(false)
const loadingRoles     = ref(false)
const loadingGroups    = ref(false)

// ─── DETAIL STATE ──────────────────────────────────────────────────────────
const selected = ref(null)   // { type: 'auth'|'role'|'group', item }

// Resource form
const editingAuthId = ref(null)
const savingAuth    = ref(false)
const deletingAuth  = ref(false)
const authForm      = reactive({ code: '', description: '', areaId: null })
const authErrors    = reactive({ code: null, description: null, areaId: null })

// Permission management (ruoli/gruppi)
const adding              = ref(false)
const deleting            = reactive({})
const savingPerm          = reactive({})
const existingPermissions = ref([])
const loadingExisting     = ref(false)
const permSearch          = ref('')
const permKeys = ['canView', 'canCreate', 'canModify', 'canDelete', 'canAction']

// Aggiunta risorsa (dialog)
const addRowVisible = ref(false)
const addRowCode    = ref(null)
const addingCode    = reactive({})   // { [code]: true } — aggiunta inline dalla ricerca

// Moduli (per creazione/clonazione ruolo)
const modules = ref([])

// Dialog nuovo ruolo / clona
const roleDialogVisible = ref(false)
const roleDialogMode    = ref('new')   // 'new' | 'clone'
const roleSource        = ref(null)    // ruolo di partenza in clonazione
const savingRole        = ref(false)
const roleForm          = reactive({ code: '', description: '', moduleCode: null })
const roleErrors        = reactive({ code: null })

// Dialog copia permessi
const copyPermsVisible = ref(false)
const copySourceId     = ref(null)
const copying          = ref(false)

// ─── COMPUTED ──────────────────────────────────────────────────────────────
const masterLoading = computed(() =>
  masterTab.value === 'resources' ? loadingResources.value
  : masterTab.value === 'roles'   ? loadingRoles.value
  : loadingGroups.value
)

const masterItems = computed(() => {
  const src = masterTab.value === 'resources' ? resources.value
            : masterTab.value === 'roles'     ? roles.value
            : groups.value
  if (!masterSearch.value) return src
  const q = masterSearch.value.toLowerCase()
  return src.filter(i =>
    i.code?.toLowerCase().includes(q) || i.description?.toLowerCase().includes(q)
  )
})

const availableAuthCodes = computed(() => {
  const assigned = new Set(existingPermissions.value.map(p => p.authCode))
  return authCodes.value.filter(a => !assigned.has(a.code))
})

const filteredPermissions = computed(() => {
  const q = permSearch.value.toLowerCase()
  return existingPermissions.value
    .filter(p =>
      !q ||
      p.authCode?.toLowerCase().includes(q) ||
      p.authDescription?.toLowerCase().includes(q)
    )
    .sort((a, b) => (a.authCode || '').localeCompare(b.authCode || ''))
})

// Risorse esistenti che combaciano con la ricerca ma NON sono assegnate al gruppo/ruolo.
// Mostrate solo quando l'utente sta cercando, per offrire l'aggiunta inline.
const matchingUnassigned = computed(() => {
  if (!permSearch.value) return []
  const q = permSearch.value.toLowerCase()
  return availableAuthCodes.value
    .filter(a => a.code?.toLowerCase().includes(q) || a.label?.toLowerCase().includes(q))
    .sort((a, b) => (a.code || '').localeCompare(b.code || ''))
})

// Ruoli diversi da quello selezionato (per "copia permessi da...")
const otherRoles = computed(() =>
  roles.value
    .filter(r => r.id !== selected.value?.item?.id)
    .map(r => ({ id: r.id, label: r.description ? `${r.code} — ${r.description}` : r.code }))
)

const roleDialogTitle = computed(() =>
  roleDialogMode.value === 'clone' ? 'Clona ruolo'
  : roleDialogMode.value === 'edit' ? 'Modifica ruolo'
  : 'Nuovo ruolo'
)
const roleDialogConfirmLabel = computed(() =>
  roleDialogMode.value === 'clone' ? 'Clona'
  : roleDialogMode.value === 'edit' ? 'Salva'
  : 'Crea ruolo'
)
const roleDialogConfirmIcon = computed(() =>
  roleDialogMode.value === 'clone' ? 'pi pi-clone'
  : roleDialogMode.value === 'edit' ? 'pi pi-check'
  : 'pi pi-plus'
)

// ─── LIFECYCLE ─────────────────────────────────────────────────────────────
onMounted(() => {
  loadResources()
  loadRoles()
  loadGroups()
  loadAreas()
  loadModules()
})

// ─── MASTER LOADERS ────────────────────────────────────────────────────────
async function loadResources() {
  loadingResources.value = true
  try {
    const { data } = await authorizationsApi.getAll()
    resources.value = (Array.isArray(data) ? data : [])
      .map(a => ({
        id: a.id, code: a.code, description: a.description,
        areaId: a.areaId, areaCode: a.areaCode, moduleCode: a.moduleCode,
      }))
      .sort((x, y) => (x.code || '').localeCompare(y.code || ''))
    // mantieni l'elenco usato dalla select dei permessi allineato
    authCodes.value = resources.value.map(a => ({ code: a.code, label: a.description ?? a.code }))
  } catch {
    resources.value = []
  } finally {
    loadingResources.value = false
  }
}

async function loadRoles() {
  loadingRoles.value = true
  try {
    const { data } = await rolesAuthApi.getByModule('DomuWeb')
    roles.value = Array.isArray(data) ? data : []
  } catch {
    roles.value = []
  } finally {
    loadingRoles.value = false
  }
}

async function loadGroups() {
  loadingGroups.value = true
  try {
    const { data } = await groupsAuthApi.getAll()
    groups.value = Array.isArray(data) ? data.filter(g => !g.isRole) : []
  } catch {
    groups.value = []
  } finally {
    loadingGroups.value = false
  }
}

async function loadAreas() {
  try {
    const { data } = await authorizationsApi.getAreas()
    areas.value = (Array.isArray(data) ? data : []).map(a => ({
      id: a.id,
      label: a.moduleCode ? `${a.description} (${a.moduleCode})` : a.description,
    }))
  } catch {
    areas.value = []
  }
}

async function loadModules() {
  try {
    const { data } = await rolesAuthApi.getModules()
    modules.value = (Array.isArray(data) ? data : []).map(m => ({
      code: m.code,
      label: m.description ? `${m.description} (${m.code})` : m.code,
    }))
  } catch {
    modules.value = []
  }
}

function reloadMaster() {
  if (masterTab.value === 'roles') loadRoles()
  else if (masterTab.value === 'groups') loadGroups()
  else loadResources()
}

// ─── SELECTION ─────────────────────────────────────────────────────────────
function isSelected(it) {
  if (!selected.value) return false
  const t = masterTab.value === 'resources' ? 'auth' : masterTab.value === 'roles' ? 'role' : 'group'
  return selected.value.type === t && selected.value.item?.id === it.id
}

function selectItem(it) {
  if (masterTab.value === 'resources') {
    selected.value = { type: 'auth', item: it }
    editingAuthId.value = it.id
    authForm.code        = it.code
    authForm.description = it.description
    authForm.areaId      = it.areaId
    resetAuthErrors()
  } else {
    const type = masterTab.value === 'roles' ? 'role' : 'group'
    selected.value = { type, item: it }
    resetPermForm()
    loadExistingPermissions(it)
  }
}

// ─── RESOURCE CRUD ─────────────────────────────────────────────────────────
function newResource() {
  masterTab.value = 'resources'
  selected.value      = { type: 'auth', item: null }
  editingAuthId.value = null
  authForm.code        = ''
  authForm.description = ''
  authForm.areaId      = null
  resetAuthErrors()
}

function resetAuthErrors() {
  authErrors.code = null
  authErrors.description = null
  authErrors.areaId = null
}

function validateAuth() {
  resetAuthErrors()
  let ok = true
  if (!authForm.code?.trim())        { authErrors.code = 'Il codice è obbligatorio'; ok = false }
  if (!authForm.description?.trim()) { authErrors.description = 'La descrizione è obbligatoria'; ok = false }
  if (!authForm.areaId)              { authErrors.areaId = "L'area è obbligatoria"; ok = false }
  return ok
}

async function saveResource() {
  if (!validateAuth()) return
  savingAuth.value = true
  try {
    if (editingAuthId.value) {
      await authorizationsApi.update(editingAuthId.value, {
        description: authForm.description.trim(),
        areaId: authForm.areaId,
      })
      toast.add({ severity: 'success', summary: 'Risorsa aggiornata', detail: authForm.code, life: 3000 })
    } else {
      await authorizationsApi.create({
        code: authForm.code.trim(),
        description: authForm.description.trim(),
        areaId: authForm.areaId,
      })
      toast.add({ severity: 'success', summary: 'Risorsa creata', detail: authForm.code.trim(), life: 3000 })
    }
    await loadResources()
    // riallinea la selezione sulla risorsa salvata
    const saved = resources.value.find(r => r.code === authForm.code.trim())
    if (saved) selectItem(saved)
  } catch (err) {
    if (!err?.response) toast.add({ severity: 'error', summary: 'Errore di rete', life: 4000 })
  } finally {
    savingAuth.value = false
  }
}

async function deleteResource() {
  if (!editingAuthId.value) return
  if (!confirm(`Eliminare la risorsa "${authForm.code}"? L'operazione fallisce se è assegnata a ruoli, gruppi o utenti.`)) return
  deletingAuth.value = true
  try {
    await authorizationsApi.remove(editingAuthId.value)
    toast.add({ severity: 'success', summary: 'Risorsa eliminata', detail: authForm.code, life: 3000 })
    selected.value = null
    await loadResources()
  } catch (err) {
    if (!err?.response) toast.add({ severity: 'error', summary: 'Errore di rete', life: 4000 })
  } finally {
    deletingAuth.value = false
  }
}

// ─── PERMISSION MANAGEMENT ─────────────────────────────────────────────────
async function loadExistingPermissions(item) {
  loadingExisting.value = true
  try {
    const { data } = await groupsAuthApi.getAuthorizations(item.id)
    existingPermissions.value = Array.isArray(data)
      ? data.map(p => ({
          ...p,
          can: p.can != null ? p.can : {
            canView:   p.canView   ?? false,
            canCreate: p.canCreate ?? false,
            canModify: p.canModify ?? false,
            canDelete: p.canDelete ?? false,
            canAction: p.canAction ?? false,
          },
        }))
      : []
  } catch {
    existingPermissions.value = []
  } finally {
    loadingExisting.value = false
  }
}

async function updateExistingPermission(perm) {
  if (!perm.id || !selected.value) return
  savingPerm[perm.id] = true
  try {
    const api = selected.value.type === 'role' ? rolesAuthApi : groupsAuthApi
    await api.updatePermission(perm.id, selected.value.item.id, perm.authCode, perm.can)
  } catch {
    await loadExistingPermissions(selected.value.item)
  } finally {
    delete savingPerm[perm.id]
  }
}

// Toggle di una singola cella della matrice (salvataggio immediato)
async function togglePerm(perm, key) {
  if (savingPerm[perm.id]) return
  perm.can[key] = !perm.can[key]
  await updateExistingPermission(perm)
}

function resetPermForm() {
  existingPermissions.value = []
  permSearch.value = ''
}

// ─── AGGIUNTA RISORSA A UN RUOLO/GRUPPO ────────────────────────────────────
function openAddRow() {
  addRowCode.value = null
  addRowVisible.value = true
}

// Assegna una risorsa (con sola Visualizza) al ruolo/gruppo selezionato.
async function assignResource(authCode) {
  if (!authCode || !selected.value) return
  const api = selected.value.type === 'role' ? rolesAuthApi : groupsAuthApi
  await api.addPermission(selected.value.item.id, authCode, {
    canView: true, canCreate: false, canModify: false, canDelete: false, canAction: false,
  })
  await loadExistingPermissions(selected.value.item)
  permSearch.value = authCode   // posiziona il filtro sulla risorsa appena aggiunta
  toast.add({
    severity: 'success', summary: 'Risorsa aggiunta',
    detail: `"${authCode}" assegnata a "${selected.value.item.code}".`, life: 3500,
  })
}

async function confirmAddRow() {
  if (!addRowCode.value) return
  adding.value = true
  try {
    await assignResource(addRowCode.value)
    addRowVisible.value = false
  } catch {
    // gestito dall'interceptor
  } finally {
    adding.value = false
  }
}

// Aggiunta inline dal risultato di ricerca ("+" Aggiungi)
async function addResourceByCode(code) {
  if (!code) return
  addingCode[code] = true
  try {
    await assignResource(code)
  } catch {
    // gestito dall'interceptor
  } finally {
    delete addingCode[code]
  }
}

// ─── NUOVO RUOLO / CLONA ───────────────────────────────────────────────────
const defaultModuleCode = () => modules.value.find(m => m.code === 'DomuWeb')?.code ?? modules.value[0]?.code ?? null

function openNewRole() {
  roleDialogMode.value = 'new'
  roleSource.value     = null
  roleForm.code        = ''
  roleForm.description = ''
  roleForm.moduleCode  = defaultModuleCode()
  roleErrors.code      = null
  roleDialogVisible.value = true
}

function openCloneRole() {
  if (!selected.value || selected.value.type !== 'role') return
  roleDialogMode.value = 'clone'
  roleSource.value     = selected.value.item
  roleForm.code        = `${selected.value.item.code}_COPY`
  roleForm.description = selected.value.item.description ? `${selected.value.item.description} (copia)` : ''
  roleForm.moduleCode  = defaultModuleCode()
  roleErrors.code      = null
  roleDialogVisible.value = true
}

function openEditRole() {
  if (!selected.value || selected.value.type !== 'role') return
  roleDialogMode.value = 'edit'
  roleSource.value     = selected.value.item
  roleForm.code        = selected.value.item.code
  roleForm.description = selected.value.item.description ?? ''
  roleForm.moduleCode  = null
  roleErrors.code      = null
  roleDialogVisible.value = true
}

async function confirmRoleDialog() {
  if (!roleForm.code?.trim()) { roleErrors.code = 'Il codice è obbligatorio'; return }
  const mode = roleDialogMode.value
  savingRole.value = true
  try {
    const payload = {
      code: roleForm.code.trim(),
      description: roleForm.description?.trim() ?? '',
      moduleCode: roleForm.moduleCode,
    }
    let result
    if (mode === 'clone') {
      const { data } = await rolesAuthApi.clone(roleSource.value.id, payload)
      result = data
      toast.add({ severity: 'success', summary: 'Ruolo clonato', detail: payload.code, life: 3500 })
    } else if (mode === 'edit') {
      const { data } = await rolesAuthApi.updateDetails(roleSource.value.id, payload)
      result = data ?? { id: roleSource.value.id, code: payload.code }
      toast.add({ severity: 'success', summary: 'Ruolo aggiornato', detail: payload.code, life: 3500 })
    } else {
      const { data } = await rolesAuthApi.create(payload)
      result = data
      toast.add({ severity: 'success', summary: 'Ruolo creato', detail: payload.code, life: 3500 })
    }
    roleDialogVisible.value = false
    await loadRoles()
    // seleziona il ruolo risultante
    const fresh = roles.value.find(r => r.id === result?.id) || roles.value.find(r => r.code === payload.code)
    if (fresh) { masterTab.value = 'roles'; selectItem(fresh) }
    else if (mode !== 'edit')
      toast.add({ severity: 'warn', summary: 'Operazione completata', detail: 'Il ruolo non risulta nella lista del modulo selezionato.', life: 5000 })
  } catch (err) {
    const msg = err?.response?.data?.Errors?.[0]
      ?? err?.response?.data?.message
      ?? err?.response?.data?.title
      ?? err?.response?.data
      ?? (err?.response ? `Errore ${err.response.status}` : 'Impossibile raggiungere il server')
    const summary = mode === 'clone' ? 'Clonazione fallita' : mode === 'edit' ? 'Modifica fallita' : 'Creazione fallita'
    toast.add({ severity: 'error', summary, detail: typeof msg === 'string' ? msg : undefined, life: 6000 })
  } finally {
    savingRole.value = false
  }
}

// ─── COPIA PERMESSI DA UN ALTRO RUOLO ──────────────────────────────────────
function openCopyPerms() {
  copySourceId.value = null
  copyPermsVisible.value = true
}

async function confirmCopyPerms() {
  if (!copySourceId.value || !selected.value) return
  copying.value = true
  try {
    await rolesAuthApi.copyPermissions(selected.value.item.id, copySourceId.value)
    await loadExistingPermissions(selected.value.item)
    copyPermsVisible.value = false
    toast.add({ severity: 'success', summary: 'Permessi copiati', life: 3500 })
  } catch (err) {
    if (!err?.response) toast.add({ severity: 'error', summary: 'Errore di rete', life: 4000 })
  } finally {
    copying.value = false
  }
}

async function deletePermission(perm, list) {
  deleting[perm.id] = true
  try {
    const api = selected.value.type === 'role' ? rolesAuthApi : groupsAuthApi
    await api.deletePermission(perm.id)
    const idx = list.findIndex(a => a.id === perm.id)
    if (idx > -1) list.splice(idx, 1)
    toast.add({ severity: 'success', summary: 'Permesso rimosso', detail: perm.authCode, life: 3000 })
  } catch {
    // gestito dall'interceptor
  } finally {
    delete deleting[perm.id]
  }
}
</script>

<style scoped>
/* ── PAGE ─────────────────────────────────────────────────────────────────── */
.auth-page {
  display: flex;
  flex-direction: column;
  gap: 18px;
  padding: 28px 32px;
  min-height: 100%;
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header { display: flex; align-items: flex-start; }
.page-header__left { display: flex; flex-direction: column; gap: 4px; }
.page-title {
  display: flex; align-items: center; gap: 10px;
  font-size: 22px; font-weight: 700; color: var(--text); margin: 0;
}
.page-title__icon { color: var(--accent); font-size: 20px; }
.page-subtitle { font-size: 13px; color: var(--text-dim); }

/* ── MASTER-DETAIL LAYOUT ─────────────────────────────────────────────────── */
.master-detail {
  display: grid;
  grid-template-columns: 340px 1fr;
  gap: 16px;
  flex: 1;
  min-height: 0;
}

/* ── MASTER ──────────────────────────────────────────────────────────────── */
.master {
  display: flex;
  flex-direction: column;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 12px;
  overflow: hidden;
}
.master__tabs {
  display: flex;
  border-bottom: 1px solid var(--border);
}
.seg-btn {
  flex: 1;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  padding: 11px 8px;
  border: none; background: transparent;
  color: var(--text-dim); font-size: 12px; font-weight: 500;
  cursor: pointer; font-family: inherit;
  border-bottom: 2px solid transparent;
  transition: color .15s, border-color .15s, background .15s;
}
.seg-btn:hover { color: var(--text); background: var(--surface); }
.seg-btn--active { color: var(--accent); border-bottom-color: var(--accent); font-weight: 600; }
.seg-btn .pi { font-size: 12px; }

.master__search {
  display: flex; align-items: center; gap: 8px;
  padding: 12px;
  border-bottom: 1px solid var(--border);
}
.master__search .p-input-icon-left { flex: 1; }
.filter-input {
  width: 100%;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.btn-new { flex: 0 0 auto; width: 36px !important; height: 36px !important; }

.master__list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
  display: flex; flex-direction: column; gap: 4px;
}
.list-item {
  display: grid;
  grid-template-columns: 1fr auto;
  grid-template-areas: "code tag" "desc tag";
  gap: 1px 8px;
  text-align: left;
  padding: 9px 11px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: transparent;
  cursor: pointer; font-family: inherit;
  transition: background .12s, border-color .12s;
}
.list-item:hover { background: var(--surface); }
.list-item--active { background: var(--surface); border-color: var(--accent); }
.list-item__code {
  grid-area: code;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px; font-weight: 600; color: var(--accent);
}
.list-item__desc {
  grid-area: desc;
  font-size: 12px; color: var(--text-dim);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.list-item__tag {
  grid-area: tag; align-self: center;
  font-size: 10px; color: var(--text-faint);
  border: 1px solid var(--border); border-radius: 4px;
  padding: 1px 5px;
}
.list-loading, .list-empty {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 8px; padding: 36px 12px;
  font-size: 12px; color: var(--text-faint);
}
.list-loading { flex-direction: row; }
.list-loading .pi-spin, .list-empty .pi { color: var(--accent); }
.list-empty .pi { font-size: 28px; opacity: .4; }

/* ── DETAIL ──────────────────────────────────────────────────────────────── */
.detail {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 12px;
  overflow: hidden;
  display: flex; flex-direction: column;
}
.detail__placeholder {
  flex: 1;
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 12px; color: var(--text-faint); font-size: 13px;
}
.detail__placeholder .pi { font-size: 32px; opacity: .4; }
.detail__pane { display: flex; flex-direction: column; min-height: 0; flex: 1; }
.detail__header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--border);
}
.detail__title {
  display: flex; align-items: center; gap: 9px;
  font-size: 15px; font-weight: 600; color: var(--text);
}
.detail__title .pi { color: var(--accent); }
.detail__actions { display: flex; align-items: center; gap: 8px; flex: 0 0 auto; }
.dialog-hint { font-size: 12px; color: var(--text-dim); margin: 0 0 14px; line-height: 1.5; }
.detail__body {
  flex: 1; overflow-y: auto;
  padding: 20px;
  display: flex; flex-direction: column; gap: 18px;
}
.form-pane { max-width: 520px; }

/* ── FIELDS ──────────────────────────────────────────────────────────────── */
.field { display: flex; flex-direction: column; gap: 5px; }
:deep(.p-dialog-content) .field + .field { margin-top: 14px; }
.field__label {
  font-size: 11px; font-weight: 600; color: var(--text-dim);
  text-transform: uppercase; letter-spacing: .5px;
}
.field__input {
  width: 100% !important;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.field__hint { font-size: 11px; color: var(--text-faint); }
.field__error { font-size: 11px; color: #ff6b6b; }
.required { color: #ff6b6b; margin-left: 2px; }
.form-actions { margin-top: 4px; }

.existing-empty {
  display: flex; align-items: center; gap: 7px;
  padding: 10px 12px; font-size: 12px; color: var(--text-faint);
  background: var(--surface); border: 1px solid var(--border); border-radius: 8px;
}

/* ── MATRIX ──────────────────────────────────────────────────────────────── */
.detail__body--matrix { gap: 12px; }

.matrix-search {
  display: flex; align-items: center; gap: 12px;
}
.matrix-search .p-input-icon-left { flex: 1; }
.matrix-count {
  font-size: 11px; color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace; white-space: nowrap;
}

.matrix {
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.matrix__head, .matrix__row {
  display: grid;
  grid-template-columns: minmax(160px, 1fr) repeat(5, 46px) 40px;
  align-items: center;
}
.matrix__head {
  background: var(--surface);
  border-bottom: 1px solid var(--border);
  padding: 9px 12px;
}
.matrix__h-res {
  font-size: 10px; font-weight: 700; color: var(--text-dim);
  text-transform: uppercase; letter-spacing: .6px;
}
.matrix__h-col {
  display: flex; align-items: center; justify-content: center;
  color: var(--text-faint); font-size: 12px;
}
.matrix__row {
  padding: 7px 12px;
  border-bottom: 1px solid var(--border);
  transition: background .12s;
}
.matrix__row:last-child { border-bottom: none; }
.matrix__row:hover { background: var(--surface); }
.matrix__row--saving { opacity: .6; }
.matrix__res { display: flex; flex-direction: column; gap: 1px; min-width: 0; padding-right: 8px; }
.matrix__code {
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px; font-weight: 600; color: var(--text);
}
.matrix__desc {
  font-size: 11px; color: var(--text-faint);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.matrix__act { display: flex; align-items: center; justify-content: center; }
.matrix__saving { font-size: 12px; color: var(--accent); }
.matrix__empty { padding: 24px 12px; text-align: center; font-size: 12px; color: var(--text-faint); }

/* Cella permesso: spenta = contorno grigio neutro; accesa = verde tenue */
.cell {
  justify-self: center;
  width: 26px; height: 26px;
  display: flex; align-items: center; justify-content: center;
  border-radius: 6px; cursor: pointer; padding: 0;
  border: 1px solid var(--border);
  background: transparent;
  color: transparent;
  transition: background .12s, border-color .12s, color .12s;
}
.cell:hover:not(:disabled) { border-color: var(--text-dim); }
.cell:disabled { cursor: not-allowed; }
.cell--off { background: transparent; }
.cell--on {
  background: rgba(52, 211, 153, .14);
  border-color: rgba(52, 211, 153, .45);
  color: var(--accent);
}
.cell--on .pi { font-size: 12px; }

/* ── RISORSE NON ASSEGNATE (aggiunta inline dalla ricerca) ───────────────── */
.unassigned {
  display: flex; flex-direction: column; gap: 6px;
  padding: 12px;
  border: 1px dashed var(--border);
  border-radius: 10px;
  background: var(--surface);
}
.unassigned__label {
  display: flex; align-items: center; gap: 6px;
  font-size: 10px; font-weight: 700; color: var(--text-dim);
  text-transform: uppercase; letter-spacing: .6px;
  margin-bottom: 2px;
}
.unassigned__label .pi { color: var(--accent); }
.unassigned__row {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  padding: 7px 4px;
}
.unassigned__row + .unassigned__row { border-top: 1px solid var(--border); }

/* ── BUTTONS ─────────────────────────────────────────────────────────────── */
.btn-primary {
  background: var(--accent) !important; border-color: var(--accent) !important;
  color: #000 !important; font-weight: 600 !important; font-size: 13px !important;
}
.btn-sm { padding: 6px 11px !important; font-size: 12px !important; }
.btn-ghost {
  background: transparent !important; border-color: var(--border) !important; color: var(--text-dim) !important;
}
.btn-danger {
  background: transparent !important; border-color: rgba(255,80,80,.4) !important;
  color: #ff5555 !important; font-size: 13px !important;
}
.btn-danger:hover { background: rgba(255,80,80,.1) !important; }
.btn-icon-sm {
  background: transparent !important; border: none !important; color: var(--text-dim) !important;
  width: 28px !important; height: 28px !important; padding: 0 !important; border-radius: 5px !important;
}
.btn-icon-sm:hover { background: var(--surface2) !important; }
.btn-icon-sm--danger:hover { background: rgba(255,80,80,.1) !important; color: #ff5555 !important; }

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 900px) {
  .master-detail { grid-template-columns: 1fr; }
  .master { max-height: 320px; }
  .auth-page { padding: 16px; }
}
</style>
