<template>
  <!-- Nav Item - Dashboard -->
  <li class="nav-item active">
    <router-link class="nav-link" :to="item.action">{{ item.description }}</router-link>
    {{item.shortDescription}}
    <ul v-if="item.children && item.children.length > 0">
      <MenuItem v-for="child in item.children"
                :key="child.id"
                :item="child" />
    </ul>
  </li>
</template>

<script setup>
  const props = defineProps({
    item: {
      type: Object,
      required: true
    }
  })
</script>
