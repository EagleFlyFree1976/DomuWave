<template>
  <div>

    <!-- Toolbar -->
    <div class="toolbar">
      <input
        class="form-input search-input"
        v-model="search"
        placeholder="Cerca per unità, proprietario o inquilino…"
      />
      <label class="toggle-label">
        <input type="checkbox" v-model="showInactive" />
        Mostra inattivi
      </label>
    </div>

    <!-- Loading scheletro unità -->
    <div v-if="loadingUnits" class="loading-state">
      <div class="spinner"></div> Caricamento…
    </div>

    <div v-else-if="!filteredUnits.length" class="empty-state">
      <div class="empty-icon">⊞</div>
      <div>Nessuna unità trovata</div>
    </div>

    <!-- Grid delle card unità -->
    <div v-else class="units-grid">
      <div v-for="u in filteredUnits" :key="u.id" class="unit-card">

        <!-- Header unità -->
        <div class="unit-header">
          <div class="unit-header-left">
            <span class="unit-number">Int. {{ u.internalNumber }}</span>
            <span v-if="u.displayName" class="unit-displayname">{{ u.displayName }}</span>
            <span class="unit-meta">
              Piano {{ u.floor }}
              <template v-if="u.staircase"> · Sc. {{ u.staircase }}</template>
              <template v-if="u.unitType"> · {{ u.unitType }}</template>
            </span>
          </div>
          <div class="unit-header-right">
            <button class="btn-icon-sm" @click="openOccupanti(u)" title="Gestisci occupanti">👤</button>
            <span class="badge" :class="u.isActive ? 'badge-green' : 'badge-muted'">
              {{ u.isActive ? 'Attiva' : 'Inattiva' }}
            </span>
          </div>
        </div>

        <!-- Corpo: proprietari | inquilini -->
        <div class="unit-body">

          <!-- Proprietari -->
          <div class="occupants-col">
            <div class="col-label">Proprietari</div>
            <div v-if="unitData[u.id]?.loadingOwners" class="mini-loading">
              <div class="spinner-sm"></div>
            </div>
            <template v-else>
              <div
                v-for="o in ownersFor(u.id)"
                :key="o.id"
                class="occupant-row"
                :class="{ 'occupant-inactive': !o.isActive }"
              >
                <span class="occupant-name">{{ fullName(o) || '—' }}</span>
                <span class="occupant-meta">{{ o.ownershipQuota }}%</span>
                <span v-if="!o.isActive" class="tag-inactive">inattivo</span>
              </div>
              <div v-if="!ownersFor(u.id).length" class="occupant-empty">—</div>
            </template>
          </div>

          <div class="col-divider"></div>

          <!-- Inquilini -->
          <div class="occupants-col">
            <div class="col-label">Inquilini</div>
            <div v-if="unitData[u.id]?.loadingTenants" class="mini-loading">
              <div class="spinner-sm"></div>
            </div>
            <template v-else>
              <div
                v-for="t in tenantsFor(u.id)"
                :key="t.id"
                class="occupant-row"
                :class="{ 'occupant-inactive': !t.isActive }"
              >
                <span class="occupant-name">{{ fullName(t) || '—' }}</span>
                <span v-if="t.leaseEndDate" class="occupant-meta">fino {{ fmtDate(t.leaseEndDate) }}</span>
                <span v-if="!t.isActive" class="tag-inactive">inattivo</span>
              </div>
              <div v-if="!tenantsFor(u.id).length" class="occupant-empty">—</div>
            </template>
          </div>

        </div>
      </div>
    </div>

  </div>

  <!-- Occupanti modal -->
  <OccupantiModal
    v-if="occupantiUnit"
    :unit-id="occupantiUnit.id"
    :unit-label="occupantiUnit.internalNumber || `#${occupantiUnit.id}`"
    @close="onOccupantiClose"
  />
</template>

<script setup>
import { ref, reactive, computed, inject, onMounted, watch } from 'vue'
import { unitApi, unitOwnerApi, unitTenantApi } from '@/services/api'
import OccupantiModal from '@/views/condomini/OccupantiModal.vue'

const condominiumId = inject('condominiumId')

const units        = ref([])
const unitData     = reactive({})   // { [unitId]: { owners, tenants, loadingOwners, loadingTenants } }
const loadingUnits = ref(false)
const search       = ref('')
const showInactive = ref(false)
const occupantiUnit = ref(null)

// ─── Helpers ──────────────────────────────────────────────────
function fullName(p) {
  return [p.firstName, p.lastName].filter(Boolean).join(' ')
}

function fmtDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('it-IT', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

// ─── Computed ─────────────────────────────────────────────────
function ownersFor(unitId) {
  const all = unitData[unitId]?.owners ?? []
  return showInactive.value ? all : all.filter(o => o.isActive)
}

function tenantsFor(unitId) {
  const all = unitData[unitId]?.tenants ?? []
  return showInactive.value ? all : all.filter(t => t.isActive)
}

const filteredUnits = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return units.value

  return units.value.filter(u => {
    if ((u.internalNumber ?? '').toLowerCase().includes(q)) return true
    if ((u.staircase ?? '').toLowerCase().includes(q)) return true
    if ((u.unitType ?? '').toLowerCase().includes(q)) return true

    const owners  = unitData[u.id]?.owners  ?? []
    const tenants = unitData[u.id]?.tenants ?? []

    if (owners.some(o  => fullName(o).toLowerCase().includes(q)))  return true
    if (tenants.some(t => fullName(t).toLowerCase().includes(q))) return true

    return false
  })
})

// ─── Data loading ─────────────────────────────────────────────
async function loadUnit(u) {
  unitData[u.id] = { owners: [], tenants: [], loadingOwners: true, loadingTenants: true }

  unitOwnerApi.getByUnit(u.id)
    .then(r => { unitData[u.id].owners = r.data ?? [] })
    .catch(() => {})
    .finally(() => { unitData[u.id].loadingOwners = false })

  unitTenantApi.getByUnit(u.id)
    .then(r => { unitData[u.id].tenants = r.data ?? [] })
    .catch(() => {})
    .finally(() => { unitData[u.id].loadingTenants = false })
}

async function load(id) {
  loadingUnits.value = true
  try {
    const { data } = await unitApi.getByCondominium(id)
    units.value = data ?? []
    units.value.forEach(loadUnit)
  } catch { /* handled by interceptor */ }
  finally { loadingUnits.value = false }
}

function openOccupanti(unit) {
  occupantiUnit.value = unit
}

function onOccupantiClose() {
  const unit = occupantiUnit.value
  occupantiUnit.value = null
  if (unit) loadUnit(unit)
}

onMounted(() => load(condominiumId.value))
watch(condominiumId, load)
</script>

<style scoped>
.toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
  flex-wrap: wrap;
}
.search-input { flex: 1; min-width: 220px; max-width: 360px; }
.toggle-label {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.875rem;
  color: var(--text-secondary);
  cursor: pointer;
  user-select: none;
}

/* ── Grid ───────────────────────────────────────────────────── */
.units-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(420px, 1fr));
  gap: 1rem;
}

/* ── Card ───────────────────────────────────────────────────── */
.unit-card {
  background: var(--bg-surface, #fff);
  border: 1px solid var(--border);
  border-radius: 8px;
  overflow: hidden;
}

.unit-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.65rem 1rem;
  background: var(--bg-subtle, #f8f9fa);
  border-bottom: 1px solid var(--border);
  gap: 0.5rem;
}
.unit-header-left { display: flex; align-items: center; gap: 0.6rem; }
.unit-header-right { display: flex; align-items: center; gap: 0.4rem; }
.btn-icon-sm {
  background: none;
  border: none;
  cursor: pointer;
  padding: 2px 4px;
  font-size: 0.9rem;
  border-radius: 4px;
  line-height: 1;
  opacity: 0.6;
  transition: opacity 0.15s;
}
.btn-icon-sm:hover { opacity: 1; background: var(--bg-muted, #f3f4f6); }
.unit-number { font-weight: 600; font-size: 0.9rem; }
.unit-displayname { font-size: 0.85rem; color: var(--text-secondary); font-style: italic; max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.unit-meta { font-size: 0.8rem; color: var(--text-secondary); }

/* ── Body con 2 colonne ─────────────────────────────────────── */
.unit-body {
  display: flex;
  padding: 0.75rem 1rem;
  gap: 0;
  min-height: 56px;
}
.occupants-col {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}
.col-divider {
  width: 1px;
  background: var(--border);
  margin: 0 0.875rem;
  flex-shrink: 0;
}
.col-label {
  font-size: 0.72rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--text-muted, #9ca3af);
  margin-bottom: 0.15rem;
}

/* ── Occupante ──────────────────────────────────────────────── */
.occupant-row {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.84rem;
  line-height: 1.4;
}
.occupant-name { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.occupant-meta { font-size: 0.75rem; color: var(--text-secondary); white-space: nowrap; }
.occupant-empty { font-size: 0.84rem; color: var(--text-muted, #9ca3af); }

.occupant-inactive { opacity: 0.45; }
.occupant-inactive .occupant-name { text-decoration: line-through; }

.tag-inactive {
  font-size: 0.68rem;
  background: var(--bg-muted, #f3f4f6);
  color: var(--text-muted, #9ca3af);
  border-radius: 3px;
  padding: 1px 4px;
  white-space: nowrap;
}

/* ── Mini loading ───────────────────────────────────────────── */
.mini-loading { display: flex; align-items: center; height: 20px; }
.spinner-sm {
  width: 14px;
  height: 14px;
  border: 2px solid var(--border);
  border-top-color: var(--accent, #4f6ef7);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
