<template>
  <div class="tenant-detail-page">

    <!-- ── BREADCRUMB ─────────────────────────────────────────────────── -->
    <div class="breadcrumb">
      <span class="breadcrumb__link" @click="goBack">
        <i class="pi pi-arrow-left" /> Tenant
      </span>
      <span class="breadcrumb__sep">›</span>
      <span class="breadcrumb__current">{{ pageTitle }}</span>
    </div>

    <!-- ── HEADER ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div>
        <h1 class="page-title">{{ pageTitle }}</h1>
        <span v-if="!isNew" class="page-id">ID: {{ store.currentTenant?.id }}</span>
      </div>
      <div class="page-header__actions">
        <Button
          label="Annulla"
          icon="pi pi-times"
          class="btn-ghost"
          :disabled="store.saving"
          @click="goBack"
        />
        <Button
          :label="store.saving ? 'Salvataggio...' : 'Salva'"
          icon="pi pi-check"
          class="btn-primary"
          :loading="store.saving"
          :disabled="store.saving || store.loading"
          @click="handleSave"
        />
      </div>
    </div>

    <!-- ── SPINNER CARICAMENTO ─────────────────────────────────────────── -->
    <div v-if="store.loading" class="loading-state">
      <i class="pi pi-spinner pi-spin loading-state__icon" />
      <span>Caricamento in corso...</span>
    </div>

    <!-- ── FORM ──────────────────────────────────────────────────────────── -->
    <template v-else-if="store.currentTenant">
      <div class="form-grid">

        <!-- ── CARD: Informazioni principali ─────────────────────────── -->
        <div class="form-card form-card--main">
          <div class="form-card__header">
            <i class="pi pi-info-circle" />
            <span>Informazioni principali</span>
          </div>
          <div class="form-card__body">

            <!-- Nome -->
            <div class="field" :class="{ 'field--error': v$.name.$error }">
              <label class="field__label" for="name">
                Nome <span class="required">*</span>
              </label>
              <InputText
                id="name"
                v-model="store.currentTenant.name"
                class="field__input"
                placeholder="Nome del tenant"
                maxlength="200"
                @blur="v$.name.$touch()"
              />
              <small v-if="v$.name.$error" class="field__error">
                {{ v$.name.$errors[0].$message }}
              </small>
            </div>

            <!-- Codice -->
            <div class="field" :class="{ 'field--error': v$.code.$error }">
              <label class="field__label" for="code">
                Codice <span class="required">*</span>
              </label>
              <InputText
                id="code"
                v-model="store.currentTenant.code"
                class="field__input field__input--mono"
                placeholder="Codice univoco (es. COND_ROMA_01)"
                maxlength="50"
                @blur="v$.code.$touch()"
                @input="store.currentTenant.code = store.currentTenant.code?.toUpperCase()"
              />
              <small class="field__hint">
                Identificativo univoco. Usato internamente per riferimento rapido.
              </small>
              <small v-if="v$.code.$error" class="field__error">
                {{ v$.code.$errors[0].$message }}
              </small>
            </div>

          </div>
        </div>

        <!-- ── CARD: Configurazione ───────────────────────────────────── -->
        <div class="form-card form-card--settings">
          <div class="form-card__header">
            <i class="pi pi-cog" />
            <span>Configurazione</span>
          </div>
          <div class="form-card__body">

            <!-- Stato -->
            <div class="field field--inline">
              <label class="field__label" for="isActive">Stato attivo</label>
              <div class="toggle-wrapper">
                <ToggleSwitch
                  id="isActive"
                  v-model="store.currentTenant.isActive"
                  class="tenant-toggle"
                />
                <span class="toggle-label" :class="store.currentTenant.isActive ? 'toggle-label--on' : 'toggle-label--off'">
                  {{ store.currentTenant.isActive ? 'Attivo' : 'Inattivo' }}
                </span>
              </div>
              <small class="field__hint">
                I tenant inattivi non possono accedere alla piattaforma.
              </small>
            </div>

          </div>
        </div>

        <!-- ── CARD: Audit trail (solo in modifica) ───────────────────── -->
        <div v-if="!isNew" class="form-card form-card--audit">
          <div class="form-card__header">
            <i class="pi pi-history" />
            <span>Audit</span>
          </div>
          <div class="form-card__body">
            <div class="audit-grid">
              <div class="audit-item">
                <span class="audit-item__label">Creato da</span>
                <span class="audit-item__value">{{ store.currentTenant.createdByFullName || '—' }}</span>
              </div>
              <div class="audit-item">
                <span class="audit-item__label">Ultima modifica</span>
                <span class="audit-item__value">{{ store.currentTenant.lastUpdatedByFullName || '—' }}</span>
              </div>
            </div>
          </div>
        </div>

      </div>

    </template>

    <!-- ── ERRORE CARICAMENTO ─────────────────────────────────────────── -->
    <div v-else class="error-state">
      <i class="pi pi-exclamation-circle error-state__icon" />
      <span>Impossibile caricare il tenant</span>
      <Button label="Riprova" icon="pi pi-refresh" class="btn-ghost" size="small" @click="loadData" />
    </div>

  </div>
</template>

<script setup>
import { computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useVuelidate } from '@vuelidate/core'
import { required, minLength, maxLength, helpers } from '@vuelidate/validators'
import { useToast } from 'primevue/usetoast'
import { useTenantStore } from '@/stores/tenantStore'

import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'

// ─── PROPS (route props: true) ────────────────────────────────────────────────
// Con props: true il router inietta i params come props del componente.
// Dichiariamo 'id' come prop opzionale (undefined = nuovo tenant).
const props = defineProps({
  id: { type: String, default: null }
})

// ─── COMPOSABLES ────────────────────────────────────────────────────────────
const router = useRouter()
const route  = useRoute()
const toast  = useToast()
const store  = useTenantStore()

// ─── COMPUTED ────────────────────────────────────────────────────────────────
// Legge prima dalla prop (props: true), poi da route.params come fallback
const tenantId  = computed(() => props.id || route.params.id || null)
const isNew     = computed(() => !tenantId.value)
const pageTitle = computed(() => (isNew.value ? 'Nuovo Tenant' : 'Modifica Tenant'))

// ─── VALIDATION ──────────────────────────────────────────────────────────────
const rules = computed(() => ({
  name: {
    required: helpers.withMessage('Il nome è obbligatorio', required),
    minLength: helpers.withMessage('Minimo 2 caratteri', minLength(2)),
    maxLength: helpers.withMessage('Massimo 200 caratteri', maxLength(200)),
  },
  code: {
    required: helpers.withMessage('Il codice è obbligatorio', required),
    minLength: helpers.withMessage('Minimo 2 caratteri', minLength(2)),
    maxLength: helpers.withMessage('Massimo 50 caratteri', maxLength(50)),
    alphaNumUnderscore: helpers.withMessage(
      'Solo lettere, numeri e underscore',
      helpers.regex(/^[A-Z0-9_]+$/)
    ),
  },
}))

const formState = computed(() => ({
  name: store.currentTenant?.name ?? '',
  code: store.currentTenant?.code ?? '',
}))

const v$ = useVuelidate(rules, formState)

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
onMounted(() => loadData())

// Osserva cambi del param id: gestisce navigazione tra dettagli
// senza smontare/rimontare il componente (es. da /tenants/1 a /tenants/2)
watch(tenantId, (newId, oldId) => {
  if (newId !== oldId) loadData()
})

// ─── METHODS ────────────────────────────────────────────────────────────────

async function loadData() {
  if (isNew.value) {
    store.initNew()
  } else {
    await store.fetchById(tenantId.value)
  }
}

async function handleSave() {
  // Valida
  const valid = await v$.value.$validate()
  if (!valid) return

  try {
    const saved = await store.save()
    toast.add({
      severity: 'success',
      summary: 'Salvato',
      detail: `Tenant "${saved.name}" salvato con successo.`,
      life: 3000,
    })
    // Dopo la creazione, naviga al dettaglio (aggiorna URL con il nuovo ID)
    // Usa lo stesso name della route corrente per essere coerente con il progetto
    if (isNew.value) {
      router.replace({ name: 'tenant-detail', params: { id: saved.id } })
    }
  } catch {
    // Il toast di errore è già mostrato dall'interceptor API
  }
}

function goBack() {
  router.push({ name: 'tenants' })
}
</script>

<style scoped>
/* ── PAGE LAYOUT ─────────────────────────────────────────────────────────── */
.tenant-detail-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  max-width: 960px;
}

/* ── BREADCRUMB ──────────────────────────────────────────────────────────── */
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: var(--text-faint);
}
.breadcrumb__link {
  cursor: pointer;
  color: var(--accent);
  display: flex;
  align-items: center;
  gap: 5px;
  transition: opacity 0.15s;
}
.breadcrumb__link:hover {
  opacity: 0.75;
}
.breadcrumb__sep {
  color: var(--border);
}
.breadcrumb__current {
  color: var(--text-dim);
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}
.page-id {
  font-size: 11px;
  color: var(--text-faint);
  font-family: 'JetBrains Mono', monospace;
}
.page-header__actions {
  display: flex;
  gap: 8px;
}

/* ── BUTTONS ─────────────────────────────────────────────────────────────── */
.btn-primary {
  background: var(--accent) !important;
  border-color: var(--accent) !important;
  color: #000 !important;
  font-weight: 600 !important;
  font-size: 13px !important;
}
.btn-ghost {
  background: transparent !important;
  border-color: var(--border) !important;
  color: var(--text-dim) !important;
}

/* ── FORM GRID ───────────────────────────────────────────────────────────── */
.form-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  grid-template-rows: auto auto;
  gap: 16px;
}
.form-card--main {
  grid-column: 1;
  grid-row: 1;
}
.form-card--settings {
  grid-column: 2;
  grid-row: 1;
}
.form-card--audit {
  grid-column: 1 / -1;
  grid-row: 2;
}

/* ── FORM CARD ───────────────────────────────────────────────────────────── */
.form-card {
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
}
.form-card__header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  border-bottom: 1px solid var(--border);
  font-size: 13px;
  font-weight: 600;
  color: var(--text-dim);
  background: var(--surface);
}
.form-card__header i {
  color: var(--accent);
  font-size: 14px;
}
.form-card__body {
  padding: 20px 16px;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

/* ── FIELDS ──────────────────────────────────────────────────────────────── */
.field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}
.field--inline {
  gap: 8px;
}
.field__label {
  font-size: 12px;
  font-weight: 600;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.required {
  color: #ff6b6b;
  margin-left: 2px;
}
.field__input {
  width: 100% !important;
  background: var(--surface) !important;
  border-color: var(--border) !important;
  color: var(--text) !important;
  font-size: 13px !important;
}
.field__input--mono {
  font-family: 'JetBrains Mono', monospace !important;
  letter-spacing: 0.5px !important;
}
.field--error .field__input {
  border-color: #ff6b6b !important;
}
.field__error {
  color: #ff6b6b;
  font-size: 11px;
}
.field__hint {
  color: var(--text-faint);
  font-size: 11px;
  line-height: 1.4;
}

/* ── TOGGLE ──────────────────────────────────────────────────────────────── */
.toggle-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
}
.toggle-label {
  font-size: 13px;
  font-weight: 500;
}
.toggle-label--on  { color: #5dca7e; }
.toggle-label--off { color: var(--text-faint); }

/* ── AUDIT ───────────────────────────────────────────────────────────────── */
.audit-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}
.audit-item {
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.audit-item__label {
  font-size: 11px;
  color: var(--text-faint);
  text-transform: uppercase;
  letter-spacing: 0.6px;
}
.audit-item__value {
  font-size: 13px;
  color: var(--text-dim);
}

/* ── LOADING / ERROR STATES ──────────────────────────────────────────────── */
.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 64px 0;
  color: var(--text-faint);
}
.loading-state__icon {
  font-size: 36px;
  color: var(--accent);
}
.error-state__icon {
  font-size: 36px;
  color: #ff6b6b;
}

/* ── API ERROR ───────────────────────────────────────────────────────────── */

/* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .tenant-detail-page {
    padding: 16px;
  }
  .form-grid {
    grid-template-columns: 1fr;
  }
  .form-card--main,
  .form-card--settings,
  .form-card--audit {
    grid-column: 1;
    grid-row: auto;
  }
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }
  .audit-grid {
    grid-template-columns: 1fr;
  }
}
</style>
