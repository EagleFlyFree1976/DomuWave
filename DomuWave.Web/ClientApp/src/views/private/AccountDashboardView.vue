<template>
  <div>
    <div v-if="accountid != null && accountStore.dashboarddata != null && !DomuWaveStore.loading">


      <!-- Page Heading -->
 

      <div class="card">
        <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
          <template #start>
            <h1 class="h3 mb-0 text-gray-800">
              Dashboard:    {{ accountStore.dashboarddata.accountName }}
            </h1>
          </template>
          <template #center>
          </template>
          <template #end>
            <div class="flex items-center gap-2">
              <Button label="Refresh" text plain @click="refresh">
                <i class="fas fa-sync-alt"></i>
              </Button>
               
            </div>
          </template>
        </Toolbar>

        <AccountTabber :accountid="props.accountid"></AccountTabber>
      </div>
      <!-- Content Row -->
      <!-- Content Row -->


       
      <div class="row">
        <!-- Earnings (Monthly) Card Example -->
        <box :inAmount="accountStore.dashboarddata.accountBalance"
             :outAmount="accountStore.dashboarddata.availableBalance">
          <template #header>
            Bilanzio Contabile / Disponibile
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>


      </div>
      <div class="row">

        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 0)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 0)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 0).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 0).amount">
          <template #header>
            Entrate / Uscite (Oggi)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>


        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 1)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 1)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 1).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 1).amount">
          <template #header>
            Entrate / Uscite (Settimana corrente)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>

        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 2)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 2)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 2).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 2).amount">
          <template #header>
            Entrate / Uscite (Mese corrente)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>

        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 3)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 3)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 3).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 3).amount">
          <template #header>
            Entrate / Uscite (Quarter corrente)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>
      </div>
      <div class="row">

        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 4)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 4)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 4).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 4).amount">
          <template #header>
            Entrate / Uscite (Ieri)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>


        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 5)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 5)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 5).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 5).amount">
          <template #header>
            Entrate / Uscite (Settimana precedente)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>

        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 6)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 6)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 6).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 6).amount">
          <template #header>
            Entrate / Uscite (Mese precedente)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>

        <box :inItem="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 7)"
             :outItem="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 7)"
             :inAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 0, 7).amount"
             :outAmount="getFirstTotalByMovementType(accountStore.dashboarddata, 1, 7).amount">
          <template #header>
            Entrate / Uscite (Quarter precedente)
          </template>
          <icon>                <i class="fas fa-calendar fa-2x text-gray-300"></i></icon>
        </box>
      </div>
    </div>

       
       
    <div v-if="accountStore.dashboarddata == null && !DomuWaveStore.loading">
        <Error404 :backaction="/accounts/"></Error404>
    </div>
  </div>
</template>

<script setup>
  
  import { onMounted,watch, ref } from 'vue'
  import { useAccountStore } from '@/stores/accountStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import PageHeader from './../../components/PageHeader.vue'
  import AccountTabber from './components/AccountTabber.vue'
  import Box from './../../components/dashboard/Box.vue'
  import Error404 from '../../components/Error404.vue'
  import Toolbar from 'primevue/toolbar';
  import Button from 'primevue/button';
  const accountStore = useAccountStore()
  const DomuWaveStore = useDomuWaveStore()
  function getFirstTotalByMovementType(data, movementType, period) {
    if (!data || !Array.isArray(data.totals)) return {
      "amount": 0, period: { "firstDay": 0, "lastDay": 0 } };
    return data.totals.find(item => item.movementType === movementType && item.period.period === period) || {
      "amount": 0, period: { "firstDay": 0, "lastDay": 0 }
    };
  }

  const props = defineProps({
    accountid: Number
  })

  async function refresh() {
    DomuWaveStore.startLoading();
    await accountStore.loadDashboard(props.accountid);
    DomuWaveStore.stopLoading();
  }


  watch(
    () => props.accountid,
    async (newId) => {
      if (newId != null) {
        DomuWaveStore.startLoading();
        await accountStore.loadDashboard(newId);
        DomuWaveStore.stopLoading();
        
      }
    },
    { immediate: true }
  )


</script>
