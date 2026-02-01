<template>
  <form @submit.prevent="handleSubmit" novalidate class="mx-auto" style="max-width: 400px;">

    
    <div class="mb-3">
      <label for="nome" class="form-label">Nome</label>
      <input type="text"
             id="nome"
             v-model="form.nome"
             :class="['form-control', errors.nome ? 'is-invalid' : form.nome ? 'is-valid' : '']"
             required />
      <div class="invalid-feedback">{{ errors.nome }}</div>
    </div>

    <div class="mb-3">
      <label for="cognome" class="form-label">Cognome</label>
      <input type="text"
             id="cognome"
             v-model="form.cognome"
             :class="['form-control', errors.cognome ? 'is-invalid' : form.cognome ? 'is-valid' : '']"
             required />
      <div class="invalid-feedback">{{ errors.cognome }}</div>
    </div>

    <div class="mb-3">
      <label for="email" class="form-label">Email</label>
      <input type="email"
             id="email"
             v-model="form.email"
             :class="['form-control', errors.email ? 'is-invalid' : form.email ? 'is-valid' : '']"
             required />
      <div class="invalid-feedback">{{ errors.email }}</div>
    </div>

    <div class="mb-3">
      <label for="password" class="form-label">Password</label>
      <input type="password"
             id="password"
             v-model="form.password"
             :class="['form-control', errors.password ? 'is-invalid' : form.password ? 'is-valid' : '']"
             minlength="6"
             required />
      <div class="invalid-feedback">{{ errors.password }}</div>
    </div>

    <button type="submit" class="btn btn-primary w-100">Registrati</button>
  </form>
</template>

<script setup>
  import { reactive } from 'vue';
  import { useAuthStore } from '@/stores/authStore'
 
  const auth = useAuthStore();
 
  const form = reactive({
    nome: '',
    cognome: '',
    email: '',
    password: ''
  });

  const errors = reactive({
    nome: '',
    cognome: '',
    email: '',
    password: ''
  });

  function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  async function handleSubmit() {

   

    errors.nome = form.nome ? '' : 'Il nome è obbligatorio';
    errors.cognome = form.cognome ? '' : 'Il cognome è obbligatorio';
    errors.email = form.email ? (validateEmail(form.email) ? '' : 'Email non valida') : 'L\'email è obbligatoria';
    errors.password = form.password
      ? form.password.length >= 6
        ? ''
        : 'La password deve avere almeno 6 caratteri'
      : 'La password è obbligatoria';

    if (!errors.nome && !errors.cognome && !errors.email && !errors.password) {
     
      await auth.register({
        "name": form.nome,
        "surname": form.cognome,
        "email": form.email,
        "password": form.password
      });
    }
  }
</script>
