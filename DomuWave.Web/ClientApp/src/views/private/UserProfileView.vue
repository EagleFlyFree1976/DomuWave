


<template>
  <div v-if="auth != null && auth.user != null">
    <div>ciao {{auth.user.fullName}}</div>
    <a class="dropdown-item" href="#" @click="logout" data-toggle="modal" data-target="#logoutModal">
      <i class="fas fa-sign-out-alt fa-sm fa-fw mr-2 text-gray-400"></i>
      Logout
    </a>
  </div>
</template>
<script setup>

  import { useAuthStore } from '@/stores/authStore'


  const auth = useAuthStore();

  const logout = async () => {
    console.log("eseguo il logout");
    await auth.logout();
    console.log("AUTH.Token", auth.token);
    if (auth.token) {
     // router.push('/dashboard') // rotta protetta dopo il login
    }
  }


  </script>
