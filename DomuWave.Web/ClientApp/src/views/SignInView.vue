<template>
  <div class="container mt-5" style="max-width: 400px">
  

    <SignIn></SignIn>
    </div>
</template>

<script setup>
  import { ref } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  import SignIn from './components/SignIn.vue'
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
