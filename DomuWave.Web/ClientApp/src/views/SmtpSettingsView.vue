<template>
  <div>
    <div class="page-header">
      <div>
        <h1>Configurazione Email</h1>
        <p class="text-muted" style="font-size:0.85rem;margin-top:2px">Impostazioni SMTP per l'invio delle comunicazioni ai condomini</p>
      </div>
      <div v-if="isEditing" class="status-badge" :class="form.isEnabled ? 'status-active' : 'status-inactive'">
        <span class="status-dot"></span>
        {{ form.isEnabled ? 'Attiva' : 'Disattivata' }}
      </div>
    </div>

    <div v-if="loading" class="loading-state">Caricamento…</div>

    <div v-else class="smtp-layout">

      <!-- Colonna principale -->
      <div class="smtp-main">

        <!-- Sezione server -->
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-server"></i>
            <span>Server</span>
          </div>
          <div class="form-grid-3">
            <div class="form-group col-span-2" :class="{ 'has-error': errors.host }">
              <label class="form-label">Server SMTP *</label>
              <input class="form-input" v-model="form.host" @input="clearError('host')"
                     placeholder="smtp.gmail.com" />
              <span v-if="errors.host" class="field-error">{{ errors.host }}</span>
            </div>
            <div class="form-group" :class="{ 'has-error': errors.port }">
              <label class="form-label">Porta *</label>
              <input class="form-input" v-model.number="form.port" @input="clearError('port')"
                     type="number" placeholder="587" />
              <span v-if="errors.port" class="field-error">{{ errors.port }}</span>
            </div>
          </div>
          <div class="toggle-row">
            <label class="toggle-label">
              <input type="checkbox" v-model="form.useSsl" class="toggle-checkbox" />
              <span class="toggle-track"><span class="toggle-thumb"></span></span>
              <span>Usa SSL/TLS</span>
            </label>
          </div>
        </div>

        <!-- Sezione autenticazione -->
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-lock"></i>
            <span>Autenticazione</span>
          </div>
          <div class="form-grid-2">
            <div class="form-group" :class="{ 'has-error': errors.username }">
              <label class="form-label">Username *</label>
              <input class="form-input" v-model="form.username" @input="clearError('username')"
                     placeholder="user@example.com" autocomplete="new-password" />
              <span v-if="errors.username" class="field-error">{{ errors.username }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">
                Password
                <span class="label-hint" v-if="isEditing">lascia vuoto per non modificare</span>
              </label>
              <input class="form-input" v-model="form.password" type="password"
                     placeholder="••••••••" autocomplete="new-password" />
            </div>
          </div>
        </div>

        <!-- Sezione mittente -->
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-envelope"></i>
            <span>Mittente</span>
          </div>
          <div class="form-grid-2">
            <div class="form-group" :class="{ 'has-error': errors.fromEmail }">
              <label class="form-label">Indirizzo mittente *</label>
              <input class="form-input" v-model="form.fromEmail" @input="clearError('fromEmail')"
                     placeholder="noreply@condominio.it" />
              <span v-if="errors.fromEmail" class="field-error">{{ errors.fromEmail }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Nome mittente</label>
              <input class="form-input" v-model="form.fromName"
                     placeholder="Amministrazione Condominio" />
            </div>
          </div>
        </div>

      </div>

      <!-- Colonna laterale -->
      <div class="smtp-sidebar">

        <!-- Stato & salva -->
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-cog"></i>
            <span>Stato</span>
          </div>
          <div class="toggle-row" style="margin-bottom:16px">
            <label class="toggle-label">
              <input type="checkbox" v-model="form.isEnabled" class="toggle-checkbox" />
              <span class="toggle-track"><span class="toggle-thumb"></span></span>
              <span>Configurazione attiva</span>
            </label>
          </div>
          <button class="btn btn-primary btn-block" @click="save" :disabled="saving">
            <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
            <span v-else><i class="pi pi-save"></i></span>
            Salva configurazione
          </button>
        </div>

        <!-- Test invio -->
        <div class="settings-card">
          <div class="settings-card-header">
            <i class="pi pi-send"></i>
            <span>Invia email di test</span>
          </div>
          <p class="settings-hint">Verifica che le impostazioni siano corrette inviando una email di prova.</p>
          <div class="form-group" style="margin-bottom:8px">
            <label class="form-label">Destinatario</label>
            <input class="form-input" v-model="testEmail" placeholder="tua@email.com" type="email" />
          </div>
          <button class="btn btn-ghost btn-block" @click="sendTest" :disabled="testing || !testEmail">
            <span v-if="testing" class="spinner" style="width:12px;height:12px"></span>
            <span v-else><i class="pi pi-send"></i></span>
            Invia test
          </button>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { tenantSmtpApi } from '@/services/api'

const store   = useAppStore()
const loading = ref(false)
const saving  = ref(false)
const testing = ref(false)
const errors  = ref({})
const testEmail = ref('')
const isEditing = ref(false)

const defaultForm = () => ({
  host: '', port: 587, useSsl: true, username: '', password: '',
  fromEmail: '', fromName: '', isEnabled: true,
})
const form = ref(defaultForm())

async function load() {
  loading.value = true
  try {
    const { data } = await tenantSmtpApi.get()
    if (data) {
      isEditing.value = true
      form.value = {
        host: data.host, port: data.port, useSsl: data.useSsl,
        username: data.username, password: '',
        fromEmail: data.fromEmail, fromName: data.fromName, isEnabled: data.isEnabled,
      }
    }
  } catch (err) {
    if (err?.response?.status !== 404) store.toast('Errore caricamento configurazione', 'error')
  } finally {
    loading.value = false
  }
}

function clearError(f) { delete errors.value[f] }

function validate() {
  const e = {}
  if (!form.value.host?.trim())      e.host      = 'Il server SMTP è obbligatorio'
  if (!form.value.port)              e.port      = 'La porta è obbligatoria'
  if (!form.value.username?.trim())  e.username  = 'Lo username è obbligatorio'
  if (!form.value.fromEmail?.trim()) e.fromEmail = "L'indirizzo mittente è obbligatorio"
  errors.value = e
  return Object.keys(e).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    await tenantSmtpApi.upsert(form.value)
    store.toast('Configurazione SMTP salvata', 'success')
    isEditing.value = true
    form.value.password = ''
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

async function sendTest() {
  if (!testEmail.value) return
  testing.value = true
  try {
    await tenantSmtpApi.test(testEmail.value)
    store.toast(`Email di test inviata a ${testEmail.value}`, 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    testing.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.page-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 12px; }

.status-badge {
  display: flex; align-items: center; gap: 6px;
  padding: 5px 12px; border-radius: 20px; font-size: 0.8rem; font-weight: 500;
}
.status-active   { background: rgba(34,197,94,0.1);  color: #22c55e; border: 1px solid rgba(34,197,94,0.2); }
.status-inactive { background: rgba(107,114,128,0.1); color: var(--text-muted); border: 1px solid var(--border); }
.status-dot {
  width: 7px; height: 7px; border-radius: 50%;
  background: currentColor;
}

.smtp-layout {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 16px;
  align-items: start;
}
@media (max-width: 860px) {
  .smtp-layout { grid-template-columns: 1fr; }
}

.smtp-main    { display: flex; flex-direction: column; gap: 16px; }
.smtp-sidebar { display: flex; flex-direction: column; gap: 16px; }

.settings-card {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md, 8px);
  padding: 18px 20px;
}

.settings-card-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 0.8rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.5px; color: var(--text-muted);
  margin-bottom: 16px;
  padding-bottom: 10px;
  border-bottom: 1px solid var(--border);
}
.settings-card-header .pi { font-size: 0.85rem; }

.form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 12px 16px; }
.form-grid-3 { display: grid; grid-template-columns: 1fr 1fr 120px; gap: 12px 16px; }
.col-span-2  { grid-column: span 2; }

@media (max-width: 600px) {
  .form-grid-2, .form-grid-3 { grid-template-columns: 1fr; }
  .col-span-2 { grid-column: span 1; }
}

/* Toggle switch */
.toggle-row { display: flex; align-items: center; }
.toggle-label { display: flex; align-items: center; gap: 10px; cursor: pointer; font-size: 0.875rem; user-select: none; }
.toggle-checkbox { display: none; }
.toggle-track {
  width: 36px; height: 20px; border-radius: 10px;
  background: var(--border); position: relative;
  transition: background 0.2s;
  flex-shrink: 0;
}
.toggle-checkbox:checked + .toggle-track { background: var(--accent); }
.toggle-thumb {
  position: absolute; top: 3px; left: 3px;
  width: 14px; height: 14px; border-radius: 50%;
  background: #fff; transition: left 0.2s;
}
.toggle-checkbox:checked + .toggle-track .toggle-thumb { left: 19px; }

.label-hint {
  font-size: 0.75rem; color: var(--text-muted);
  font-weight: 400; margin-left: 4px;
}

.btn-block { width: 100%; justify-content: center; gap: 6px; }

.settings-hint {
  font-size: 0.82rem; color: var(--text-muted);
  margin-bottom: 12px; line-height: 1.5;
}
</style>
