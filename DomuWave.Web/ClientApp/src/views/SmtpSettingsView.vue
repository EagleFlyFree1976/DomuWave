<template>
  <div>
    <div class="page-header">
      <h1>Configurazione Email (SMTP)</h1>
    </div>

    <div v-if="loading" class="loading-state">Caricamento…</div>

    <div v-else class="card" style="max-width:600px">
      <div class="form-grid">
        <div class="form-group" :class="{ 'has-error': errors.host }">
          <label class="form-label">Server SMTP *</label>
          <input class="form-input" v-model="form.host" @input="clearError('host')" placeholder="smtp.gmail.com" />
          <span v-if="errors.host" class="field-error">{{ errors.host }}</span>
        </div>
        <div class="form-group" :class="{ 'has-error': errors.port }">
          <label class="form-label">Porta *</label>
          <input class="form-input" v-model.number="form.port" @input="clearError('port')" type="number" placeholder="587" />
          <span v-if="errors.port" class="field-error">{{ errors.port }}</span>
        </div>
        <div class="form-group form-group--full">
          <label class="form-label" style="display:flex;align-items:center;gap:8px;cursor:pointer">
            <input type="checkbox" v-model="form.useSsl" />
            <span>Usa SSL/TLS</span>
          </label>
        </div>
        <div class="form-group" :class="{ 'has-error': errors.username }">
          <label class="form-label">Username *</label>
          <input class="form-input" v-model="form.username" @input="clearError('username')" placeholder="user@example.com" autocomplete="new-password" />
          <span v-if="errors.username" class="field-error">{{ errors.username }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">Password {{ isEditing ? '(lascia vuoto per non modificare)' : '*' }}</label>
          <input class="form-input" v-model="form.password" type="password" placeholder="••••••••" autocomplete="new-password" />
        </div>
        <div class="form-group" :class="{ 'has-error': errors.fromEmail }">
          <label class="form-label">Indirizzo mittente *</label>
          <input class="form-input" v-model="form.fromEmail" @input="clearError('fromEmail')" placeholder="noreply@condominio.it" />
          <span v-if="errors.fromEmail" class="field-error">{{ errors.fromEmail }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">Nome mittente</label>
          <input class="form-input" v-model="form.fromName" placeholder="Amministrazione Condominio" />
        </div>
        <div class="form-group form-group--full">
          <label class="form-label" style="display:flex;align-items:center;gap:8px;cursor:pointer">
            <input type="checkbox" v-model="form.isEnabled" />
            <span>Configurazione attiva</span>
          </label>
        </div>
      </div>

      <div class="modal-footer" style="padding-top:16px;border-top:1px solid var(--border)">
        <!-- Test email -->
        <div style="display:flex;align-items:center;gap:8px;margin-right:auto">
          <input class="form-input" v-model="testEmail" placeholder="Email di test" style="width:220px" />
          <button class="btn btn-ghost btn-sm" @click="sendTest" :disabled="testing || !testEmail">
            <span v-if="testing" class="spinner" style="width:12px;height:12px"></span>
            <span v-else>Invia test</span>
          </button>
        </div>
        <button class="btn btn-primary" @click="save" :disabled="saving">
          <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
          Salva configurazione
        </button>
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
  if (!form.value.username?.trim())  e.username  = "Lo username è obbligatorio"
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
