import { defineStore } from 'pinia'
import { ref } from 'vue'

/**
 * Stato UI condiviso della shell.
 * mobileSidebarOpen: apertura del drawer di navigazione su schermi stretti (<768px).
 */
export const useUiStore = defineStore('ui', () => {
  const mobileSidebarOpen = ref(false)

  function toggleMobileSidebar() {
    mobileSidebarOpen.value = !mobileSidebarOpen.value
  }

  function openMobileSidebar() {
    mobileSidebarOpen.value = true
  }

  function closeMobileSidebar() {
    mobileSidebarOpen.value = false
  }

  return {
    mobileSidebarOpen,
    toggleMobileSidebar,
    openMobileSidebar,
    closeMobileSidebar,
  }
})
