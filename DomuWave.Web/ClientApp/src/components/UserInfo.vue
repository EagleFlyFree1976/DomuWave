<template>
 
  <li class="nav-item  ">
    <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button"
       data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"
       v-if="auth.user != null">
      <span class="mr-2 d-none d-lg-inline text-gray-600 small" v-if="auth.user != null">
      

      </span>
      <img class="img-profile rounded-circle" src="@/assets/undraw_profile.svg">
    </a>

    <router-link to="/signin" v-else>Login</router-link>


    <!-- Dropdown - User Information -->
    <div class="dropdown-menu dropdown-menu-right shadow animated--grow-in"
         aria-labelledby="userDropdown" v-if="auth.isAuthenticated">
 
        
        
    
      <a class="dropdown-item" href="#">
        <i class="fas fa-cogs fa-sm fa-fw mr-2 text-gray-400"></i>
        Settings
      </a>
      <a class="dropdown-item" href="#">
        <i class="fas fa-list fa-sm fa-fw mr-2 text-gray-400"></i>
        Activity Log
      </a>
      <div class="dropdown-divider"></div>
      <a class="dropdown-item" href="#" data-toggle="modal" data-target="#logoutModal">
        <i class="fas fa-sign-out-alt fa-sm fa-fw mr-2 text-gray-400"></i>
        Logout
      </a>
    </div>
  </li>
</template>
<style>
  .router-link-active {
    font-weight: bold;
    color: blue;
  }
</style>
<script setup>
 
  import { useAuthStore } from '@/stores/authStore'

   
  const auth = useAuthStore();
</script>
