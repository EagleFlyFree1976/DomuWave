<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <div class="modal-header">
        <h2>{{ props.user ? 'Modifica condomino' : 'Nuovo condomino' }}</h2>
        <button class="btn-icon" @click="$emit('close')">✕</button>
      </div>
      <div class="modal-body">

        <div class="form-grid">
          <div class="form-group" :class="{ 'has-error': errors.firstName }">
            <label class="form-label">Nome *</label>
            <input class="form-input" v-model="form.firstName" @input="clearError('firstName')" />
            <span v-if="errors.firstName" class="field-error">{{ errors.firstName }}</span>
          </div>
          <div class="form-group" :class="{ 'has-error': errors.lastName }">
            <label class="form-label">Cognome *</label>
            <input class="form-input" v-model="form.lastName" @input="clearError('lastName')" />
            <span v-if="errors.lastName" class="field-error">{{ errors.lastName }}</span>
          </div>
          <div class="form-group" :class="{ 'has-error': errors.email }" style="grid-column: span 2">
            <label class="form-label">Email *</label>
            <input class="form-input" type="email" v-model="form.email" @input="clearError('email')" />
            <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
          </div>
          <div v-if="!props.user" class="form-group" :class="{ 'has-error': errors.password }" style="grid-column: span 2">
            <label class="form-label">Password *</label>
            <input class="form-input" type="password" v-model="form.password" @input="clearError('password')" autocomplete="new-password" />
            <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
          </div>
          <div class="form-group" style="grid-column: span 2">
            <label class="form-label">Telefono</label>
            <input class="form-input" type="tel" v-model="form.phone" />
          </div>
          <div v-if="props.user" class="form-group" style="grid-column: span 2; flex-direction:row; align-items:center; gap:0.5rem">
            <input type="checkbox" id="condominoIsActive" v-model="form.isActive" />
            <label for="condominoIsActive" style="font-size:0.875rem; cursor:pointer">Utente attivo</label>
          </div>
        </div>

      </div>
      <div class="modal-footer">
        <button class="btn btn-ghost" @click="$emit('close')">Annulla</button>
        <button class="btn btn-primary" @click="save" :disabled="saving">
          <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
          {{ props.user ? 'Salva' : 'Crea' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useAppStore } from '@/stores/app'
import { userApi, userTenantApi } from '@/services/userService'

const props = defineProps({ user: Object, prefill: Object })
const emit  = defineEmits(['saved', 'close'])

const store  = useAppStore()
const saving = ref(false)
const errors = ref({})

const form = reactive({
  firstName: props.user?.firstName ?? props.user?.name ?? props.prefill?.firstName ?? '',
  lastName:  props.user?.lastName  ?? props.prefill?.lastName  ?? '',
  email:     props.user?.email     ?? props.prefill?.email     ?? '',
  phone:     props.user?.phone     ?? props.prefill?.phone     ?? '',
  password:  '',
  isActive:  props.user?.isActive  ?? true,
})

function clearError(field) {
  delete errors.value[field]
}

function validate() {
  const e = {}
  if (!form.firstName?.trim()) e.firstName = 'Il nome è obbligatorio'
  if (!form.lastName?.trim())  e.lastName  = 'Il cognome è obbligatorio'
  if (!form.email?.trim())     e.email     = "L'email è obbligatoria"
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
    e.email = 'Formato email non valido'
  if (!props.user && !form.password?.trim()) e.password = 'La password è obbligatoria'
  errors.value = e
  return Object.keys(e).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (props.user) {
      const payload = {
        email:    form.email,
        name:     form.firstName,
        surName:  form.lastName,
        roleCode: 'Condomino',
        isActive: form.isActive,
      }
      await userApi.update(props.user.id, payload)
      emit('saved', { ...props.user, firstName: form.firstName, lastName: form.lastName, email: form.email, isActive: form.isActive })
    } else {
      const payload = {
        email:      form.email,
        name:       form.firstName,
        surName:    form.lastName,
        roleCode:   'Condomino',
        password:   form.password,
        moduleCode: 'DomuWeb',
        isActive:   true,
      }
      const { data } = await userApi.create(payload)
      const created = {
        id:        data.id,
        firstName: form.firstName,
        lastName:  form.lastName,
        email:     form.email,
        phone:     form.phone,
        isActive:  true,
      }
      // Associate user with current tenant
      const tenantId = localStorage.getItem('tenantId')
      if (tenantId && created.id) {
        try {
          await userTenantApi.create({ userId: created.id, tenantId, isDefault: false, isActive: true })
        } catch { /* ignore association errors */ }
      }
      emit('saved', created)
    }
    store.toast(props.user ? 'Condomino aggiornato' : 'Condomino creato', 'success')
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.has-error .form-input { border-color: var(--accent-red, #e53e3e); }
.field-error { font-size: 0.78rem; color: var(--accent-red, #e53e3e); margin-top: 0.2rem; }
</style>
