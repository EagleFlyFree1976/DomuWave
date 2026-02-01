<template>

  <ul class="navbar-nav bg-gradient-primary sidebar sidebar-dark accordion" id="accordionSidebar">
  
    <!-- Sidebar - Brand -->
    <a class="sidebar-brand d-flex align-items-center justify-content-center" href="/">
      <div class="sidebar-brand-icon rotate-n-15">
        <i class="fas fa-laugh-wink"></i>
      </div>
      <div class="sidebar-brand-text mx-3">DomuWave <sup>X</sup></div>
    </a>
    <!-- Divider -->
    <hr class="sidebar-divider my-0">
   
    <MenuItem v-for="item in tree"
              :key="item.id"
              :item="item" />


  </ul>

</template>

<script setup>
  import { computed } from 'vue'
  import { useDomuWaveStore } from '../../stores/DomuWaveStore'
  import MenuItem from './MenuItem.vue'
  const DomuWave = useDomuWaveStore()
  const props = defineProps({
    menuItems: {
      type: Array,
      required: true
    }
  })

  // Funzione per costruire il menu ad albero
  function buildTree(items) {


    const map = new Map()
    const roots = []

    items.forEach(item => {
      map.set(item.id, { ...item, children: [] })
    })

    map.forEach(item => {
      if (item.parentMenuId) {
        const parent = map.get(item.parentMenuId)
        if (parent) parent.children.push(item)
      } else {
        roots.push(item)
      }
    })

    return roots
  }

  const tree = computed(() => buildTree(DomuWave.itemMenues))
</script>
