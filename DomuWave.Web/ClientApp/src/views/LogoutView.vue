<template>
  <div class="container mt-5" style="max-width: 400px">

    <h1>Disconnessione avvenuta con successo</h1>
    <SignIn></SignIn>
  </div>
</template>

<script setup>
  import { ref,onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
    import SignIn from './components/SignIn.vue'
  const auth = useAuthStore()
   
  const router = useRouter()

  onMounted(async () => {
   
   await auth.logout();
   
   
 })
  

 
</script>

<style scoped>
  .card {
    border-radius: 1rem;
  }
</style>
