<template>
  <div class="app-layout">
    <AppSidebar />
    <div class="app-main">
      <div class="app-content">
        <RouterView />
      </div>
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
    background: var(--bg-base);
  }

  .app-main {
    flex: 1;
    min-width: 0;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
  }

  .app-content {
    flex: 1;
    padding: 1.75rem 2rem;
    max-width: 1400px;
  }
</style>
