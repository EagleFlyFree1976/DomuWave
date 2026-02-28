<template>
  <div v-if="condominio">
    <!-- Header -->
    <div class="page-header">
      <div class="header-left">
        <router-link to="/condomini" class="btn btn-ghost btn-sm">← Indietro</router-link>
        <h1>{{ condominio.name }}</h1>
        <span class="badge" :class="condominio.isActive ? 'badge-green' : 'badge-muted'">
          {{ condominio.isActive ? 'Attivo' : 'Inattivo' }}
        </span>
      </div>
      <button class="btn btn-primary" @click="openEdit">✎ Modifica</button>
    </div>

    <!-- Detail grid: 2 columns -->
    <div class="detail-grid">

      <!-- Col 1 -->
      <div class="detail-col">
        <!-- Informazioni generali -->
        <div class="card">
          <div class="card-header"><h2>Informazioni generali</h2></div>
          <dl class="info-list">
            <div class="info-row"><dt>Codice</dt><dd class="mono">{{ condominio.code || '—' }}</dd></div>
            <div class="info-row"><dt>Cod. fiscale</dt><dd class="mono">{{ condominio.taxCode || '—' }}</dd></div>
            <div class="info-row"><dt>P.IVA</dt><dd class="mono">{{ condominio.vatNumber || '—' }}</dd></div>
            <div class="info-row"><dt>Email</dt><dd>{{ condominio.email || '—' }}</dd></div>
            <div class="info-row"><dt>Telefono</dt><dd>{{ condominio.phone || '—' }}</dd></div>
            <div class="info-row"><dt>PEC</dt><dd>{{ condominio.pec || '—' }}</dd></div>
          </dl>
        </div>

        <!-- Dati tecnici -->
        <div class="card">
          <div class="card-header"><h2>Dati tecnici</h2></div>
          <dl class="info-list">
            <div class="info-row"><dt>N° unità</dt><dd>{{ condominio.numberOfUnits }}</dd></div>
            <div class="info-row"><dt>N° scale</dt><dd>{{ condominio.numberOfStaircases }}</dd></div>
            <div class="info-row"><dt>N° piani</dt><dd>{{ condominio.numberOfFloors ?? '—' }}</dd></div>
            <div class="info-row"><dt>Anno costruzione</dt><dd>{{ condominio.yearOfConstruction ?? '—' }}</dd></div>
            <div class="info-row"><dt>Millesimi totali</dt><dd class="mono">{{ condominio.totalMillesimal }}</dd></div>
            <div class="info-row" v-if="condominio.commonAreasSqm != null">
              <dt>Mq aree comuni</dt><dd>{{ condominio.commonAreasSqm }} m²</dd>
            </div>
            <div class="info-row">
              <dt>Ascensore</dt>
              <dd>
                <span v-if="condominio.hasElevator">✓ Sì
                  <span v-if="condominio.numberOfElevators" class="text-muted">({{ condominio.numberOfElevators }})</span>
                </span>
                <span v-else class="text-muted">✕ No</span>
              </dd>
            </div>
            <div class="info-row">
              <dt>Riscald. centralizzato</dt>
              <dd><span :class="condominio.hasCentralHeating ? '' : 'text-muted'">{{ condominio.hasCentralHeating ? '✓ Sì' : '✕ No' }}</span></dd>
            </div>
            <div class="info-row">
              <dt>Portineria</dt>
              <dd><span :class="condominio.hasConcierge ? '' : 'text-muted'">{{ condominio.hasConcierge ? '✓ Sì' : '✕ No' }}</span></dd>
            </div>
          </dl>
        </div>

        <!-- Note -->
        <div class="card" v-if="condominio.notes">
          <div class="card-header"><h2>Note</h2></div>
          <p class="notes-text">{{ condominio.notes }}</p>
        </div>
      </div>

      <!-- Col 2 -->
      <div class="detail-col">
        <!-- Configurazione rate -->
        <div class="card">
          <div class="card-header"><h2>Gestione amministrativa</h2></div>
          <dl class="info-list">
            <div class="info-row"><dt>Frequenza rate</dt><dd>{{ freqLabel }}</dd></div>
            <div class="info-row"><dt>Giorno scadenza</dt><dd>{{ condominio.installmentDueDay }}° del mese</dd></div>
            <div class="info-row"><dt>Inizio mandato</dt><dd>{{ fmtDate(condominio.mandateStartDate) }}</dd></div>
            <div class="info-row"><dt>Fine mandato</dt><dd>{{ fmtDate(condominio.mandateEndDate) }}</dd></div>
            <div class="info-row"><dt>Ultima assemblea</dt><dd>{{ fmtDate(condominio.lastAssemblyDate) }}</dd></div>
          </dl>
        </div>

        <!-- Indirizzo -->
        <div class="card">
          <div class="card-header"><h2>Indirizzo</h2></div>
          <template v-if="condominio.address && (condominio.address.street || condominio.address.city)">
            <dl class="info-list">
              <div class="info-row" v-if="condominio.address.street">
                <dt>Via / Piazza</dt>
                <dd>{{ condominio.address.street }} {{ condominio.address.streetNumber }}</dd>
              </div>
              <div class="info-row" v-if="condominio.address.postalCode || condominio.address.city">
                <dt>CAP e città</dt>
                <dd>{{ [condominio.address.postalCode, condominio.address.city].filter(Boolean).join(' ') }}</dd>
              </div>
              <div class="info-row" v-if="condominio.address.province">
                <dt>Provincia</dt>
                <dd>{{ condominio.address.province }}</dd>
              </div>
              <div class="info-row" v-if="condominio.address.country">
                <dt>Nazione</dt>
                <dd>{{ condominio.address.country }}</dd>
              </div>
            </dl>
          </template>
          <p v-else class="text-muted" style="font-size:0.875rem;padding:0.5rem 1rem 1rem">Nessun indirizzo registrato</p>
        </div>
      </div>
    </div>

    <!-- Quick nav -->
    <div class="quick-nav">
      <router-link v-for="q in quickNav" :key="q.label" :to="`/condomini/${condominio.id}${q.path}`" class="qnav-chip">
        {{ q.icon }} {{ q.label }}
      </router-link>
    </div>

    <!-- Edit Modal -->
    <div class="modal-overlay" v-if="showEdit" @click.self="showEdit=false">
      <div class="modal modal--wide">
        <div class="modal-header">
          <h2>Modifica condominio</h2>
          <button class="btn-icon" @click="showEdit=false">✕</button>
        </div>
        <div class="modal-body">

          <fieldset class="form-fieldset">
            <legend class="form-fieldset-legend">Dati anagrafici</legend>
            <div class="form-grid">
              <div class="form-group" :class="{ 'has-error': errors.name }">
                <label class="form-label">Nome *</label>
                <input class="form-input" v-model="form.name" @input="clearError('name')" />
                <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
              </div>
              <div class="form-group">
                <label class="form-label">Codice</label>
                <input class="form-input" v-model="form.code" />
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

          <div class="form-group" style="margin-top:1rem">
            <label class="form-label">Note</label>
            <textarea class="form-textarea" v-model="form.notes" rows="3"></textarea>
          </div>
          <div class="form-group" style="flex-direction:row;align-items:center;gap:0.5rem;margin-top:0.5rem">
            <input type="checkbox" id="editIsActive" v-model="form.isActive" />
            <label for="editIsActive" style="font-size:0.875rem;cursor:pointer">Condominio attivo</label>
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
          <button class="btn btn-ghost" @click="showEdit=false">Annulla</button>
          <button class="btn btn-primary" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            Salva
          </button>
        </div>
      </div>
    </div>
  </div>

  <div v-else class="loading-state">
    <div class="spinner"></div> Caricamento…
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { condominiumApi } from '@/services/api'

const route    = useRoute()
const condominio = ref(null)
const showEdit = ref(false)
const saving   = ref(false)
const errors   = ref({})
const form     = ref({})
const currentYear = new Date().getFullYear()

const freqMap = { Monthly: 'Mensile', Quarterly: 'Trimestrale', Biannual: 'Semestrale', Annual: 'Annuale' }
const freqLabel = computed(() => freqMap[condominio.value?.installmentFrequency] || '—')

function fmtDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('it-IT')
}

function toDateInput(val) {
  if (!val) return null
  return new Date(val).toISOString().substring(0, 10)
}

function clearError(field) {
  delete errors.value[field]
}

function openEdit() {
  const c = condominio.value
  form.value = {
    ...c,
    mandateStartDate: toDateInput(c.mandateStartDate),
    mandateEndDate:   toDateInput(c.mandateEndDate),
    lastAssemblyDate: toDateInput(c.lastAssemblyDate),
    address: c.address ? { ...c.address } : { street: '', streetNumber: '', postalCode: '', city: '', province: '', country: '' },
  }
  errors.value = {}
  showEdit.value = true
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
    const { data } = await condominiumApi.update(condominio.value.id, form.value)
    condominio.value = data
    showEdit.value = false
  } catch {
    // error handled by global interceptor
  } finally {
    saving.value = false
  }
}

const quickNav = [
  { path: '/unita', icon: '⊞', label: 'Unità' },
  { path: '/budget',        icon: '◎', label: 'Budget' },
  { path: '/rate',          icon: '◷', label: 'Rate' },
  { path: '/fornitori',     icon: '◈', label: 'Fornitori' },
  { path: '/documenti',     icon: '▤', label: 'Documenti' },
  { path: '/comunicazioni', icon: '◉', label: 'Comunicazioni' },
]

onMounted(async () => {
  const { data } = await condominiumApi.getById(route.params.id)
  condominio.value = data
})
</script>

<style scoped>
.header-left { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}
@media (max-width: 768px) {
  .detail-grid { grid-template-columns: 1fr; }
}

.detail-col { display: flex; flex-direction: column; gap: 1rem; }

.info-list { display: flex; flex-direction: column; padding: 0 1rem; }
.info-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--border);
  gap: 1rem;
}
.info-row:last-child { border-bottom: none; }
dt { font-size: 0.8rem; color: var(--text-muted); flex-shrink: 0; }
dd { font-size: 0.875rem; color: var(--text-primary); text-align: right; }
.text-muted { color: var(--text-muted); }
.notes-text { font-size: 0.875rem; line-height: 1.7; white-space: pre-wrap; color: var(--text-secondary); padding: 0 1rem 1rem; }

.quick-nav { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.qnav-chip {
  padding: 0.4rem 0.85rem;
  border-radius: 99px;
  border: 1px solid var(--border);
  background: var(--bg-surface);
  color: var(--text-secondary);
  font-size: 0.82rem;
  text-decoration: none;
  transition: all 0.15s;
}
.qnav-chip:hover { background: var(--bg-hover); color: var(--text-primary); text-decoration: none; }

/* Edit modal */
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
