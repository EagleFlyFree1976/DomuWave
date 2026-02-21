<template>
  <div class="app-layout">
    <AppSidebar />
    <div class="app-main">
      <RouterView />
    </div>
  </div>
</template>

<script setup>
  import AppSidebar from '@/components/layout/AppSidebar.vue'
</script>

<style scoped>
  .app-layout {
    display: flex;
    min-height: 100vh;
    background: var(--p-surface-ground);
  }

  .app-main {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    display: flex;
    flex-direction: column;
  }
</style>
