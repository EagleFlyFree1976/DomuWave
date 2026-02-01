


<template>
  <div class="tabcard">
     <Tabs :value="currentRoute">
      <TabList>
        <Tab v-for="tab in items.filter(t => t.visible)" :key="tab.label" :value="tab.route">


          <router-link v-if="tab.route" v-slot="{ href, navigate }" :to="tab.route" custom>
            <a v-ripple :href="href" @click="navigate" class="p-button p-component p-button-text p-button-plain">
              <i :class="tab.icon" />
              <span>{{ tab.label }}</span>
            </a>
          </router-link>
        </Tab>
      </TabList>
    </Tabs>
  </div>

</template>
<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'
  import { useRoute } from 'vue-router'
  import Tabs from 'primevue/tabs';
  import TabList from 'primevue/tablist';
  import Tab from 'primevue/tab';
  import TabPanels from 'primevue/tabpanels';
  import TabPanel from 'primevue/tabpanel';

  let currentRoute = ref("");
  const props = defineProps({
    accountid: Number
  });

  const route = useRoute();
  const items = ref([
    
    { route: '/accounts/' + props.accountid + '/dashboard', label: 'Dashboard', icon: 'fas fa-tachometer-alt', visible:props.accountid != 0 },
    { route: '/accounts/' + props.accountid + '/transactions', label: 'Transactions', icon: 'fas fa-euro-sign', visible:props.accountid != 0 },
    { route: '/accounts/' + props.accountid, label: 'Proprietà', icon: 'far fa-edit', visible:true }

  ]);


  onMounted(() => {
    console.log("route", route);
    console.log("route path", route.path);
    currentRoute.value = route.path;
  })
</script>

<style scoped>
  .tabcard {
    background: var(--card-bg);
    border: var(--card-border);
    padding: 2rem;
    border-radius: 10px;
    margin-bottom: 1rem;
  }

  /* Personalizza il bordo inferiore del tab attivo */
  .p-tab .p-tab-active {
    border-bottom: 2px solid black !important; /* <-- imposta nero */
  }

</style>
