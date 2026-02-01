

<template>
  <div class="container-fluid">
    <div class="text-center">
      <div class="error mx-auto">404</div>
      <p class="lead text-gray-800 mb-5">Page Not Found</p>
      <p class="text-gray-500 mb-0" v-if="messageStore.visibleMessages != null && messageStore.visibleMessages.length > 0">{{messageStore.visibleMessages[0].message}}</p>
      <a :href="backaction" title="Torna indietro" class="btn">← Torna indietro</a>
  </div>
  </div>
   

</template>
<script setup>
  
import { useMessageStore } from '@/stores/messageStore'

const messageStore = useMessageStore()

const props = defineProps({
  backaction: String
})
</script>
