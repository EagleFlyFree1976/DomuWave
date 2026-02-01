<template>
  
  

    
      <div class="card shadow">
        <div class="card-body">
          <h3 class="text-center mb-4">Accedi</h3>

          <form @submit.prevent="login">
            <div class="mb-3">
              <label for="email" class="form-label">Email</label>
              <input v-model="email"
                     type="email"
                     class="form-control"
                     id="email"
                     required />
            </div>

            <div class="mb-3">
              <label for="password" class="form-label">Password</label>
              <input v-model="password"
                     type="password"
                     class="form-control"
                     id="password"
                     required />
            </div>

            <button type="submit" class="btn btn-primary w-100">Login</button>
          </form>

          <div class="text-center mt-3">
            <small>
              Non hai un account?
              <router-link to="/register">Registrati</router-link>
            </small>
          </div>

          <hr class="my-4" />

          <div class="d-grid gap-2">
            <button @click="loginWithGoogle" class="btn btn-outline-danger">
              <i class="bi bi-google"></i> Accedi con Google
            </button>
            <button @click="loginWithFacebook" class="btn btn-outline-primary">
              <i class="bi bi-facebook"></i> Accedi con Facebook
            </button>
          </div>
        </div>
      </div>
    
</template>

<script setup>
  import { ref } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  
  const auth = useAuthStore()
   
  const router = useRouter()

  const email = ref('oraziogr@gmail.com')
  const password = ref('Pokemon456')

  const login = async () => {
    console.log("eseguo il login");
    await auth.login(email.value, password.value);
    console.log("AUTH.Token", auth.token);
    if (auth.token) {
      router.push('/dashboard') // rotta protetta dopo il login
    }
  }

  const loginWithGoogle = () => {
    console.log('Login con Google')
  }

  const loginWithFacebook = () => {
    console.log('Login con Facebook')
  }
</script>

<style scoped>
  .card {
    border-radius: 1rem;
  }
</style>
