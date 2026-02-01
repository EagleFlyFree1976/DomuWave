


<template>

  <div>


    <div class="card">
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">Accounts</h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea account" text plain @click="newAccount()">
              <i class="fa fa-plus-square"></i>
            </Button>
           
          </div>
        </template>
      </Toolbar>
    </div>
    <div class="card shadow mb-4">

      <div class="card-body">
        <div class="table-responsive">
          <ConfirmPopup></ConfirmPopup>
          <table class="table table-bordered" id="dataTable" width="100%" cellspacing="0">
            <thead>
              <tr>
                <th>Tipo</th>
                <th>Valuta</th>
                <th>Name</th>
                <th>Description</th>
                <th>Aperto il </th>
                <th>Chiuso il</th>
                <th>Bilancio ad oggi</th>
                <th>Bilancio a fine mese</th>
                <th></th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="row in accountStore.accounts">
                <td>{{row.accountType.description}}</td>
                <td>{{row.currency.description}}</td>

                <td>{{row.name}}</td>
                <td>{{row.description}}</td>
                <td v-date:date="row.openDate"></td>
                <td v-date:date="row.closedDate"></td>
                <td >{{row.report.balance}}</td>
                <td>{{row.report.balanceEOM}}</td>
                <td>

                  <router-link class="btn" title="Modifica account" :to="`/accounts/${row.id}`">
                    <i class="far fa-edit"></i>
                  </router-link>
                  <router-link class="btn" title="Vai alla dashboard" :to="`/accounts/${row.id}/dashboard`">
                    <i class="fas fa-tachometer-alt"></i>
                  </router-link>
                  <Button @click="confirmRemove($event, row.id)" label="Delete" severity="danger" outlined>
                    <i class="far fa-minus-square"></i>
                  </Button>
                  <!--<a href="#" class="btn text-danger" id="btnRemoveAccount" @click="deleteAccount(row)" @click="confirm2($event)">

              </a>-->

                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <Dialog header="Crea account" v-model:visible="modalCreateAccountVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
        <AccountCreateForm ref="createForm"></AccountCreateForm>
        <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
          <Button type="button" label="Cancel" severity="secondary" @click="modalCreateAccountVisible = false"></Button>
          <Button type="button" label="Save" class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="saveNewAccount()"></Button>
        </div>
      </Dialog>
    </div>
  </div>

</template> 
<script setup>
  import { onMounted, ref, watch } from 'vue'
  
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { useAccountStore } from '@/stores/accountStore'
  import { useConfirm } from "primevue/useconfirm";
  import AccountCreateForm from './AccountCreateForm.vue'
  import Dialog from 'primevue/dialog';
  import Toolbar from 'primevue/toolbar';
  import ConfirmPopup from 'primevue/confirmPopup';
  import Button from 'primevue/button';
  const createForm = ref(null)
  var modalCreateAccountVisible = ref(false);
  const accountStore = useAccountStore();
  const DomuWaveStore = useDomuWaveStore();
  const confirm = useConfirm();
  const props = defineProps({
  
  })

  async function deleteAccount(id) {
    DomuWaveStore.startLoading();
    await accountStore.deleteAccount(id);
    await refresh();
    DomuWaveStore.stopLoading();
  }

  const confirmRemove = (event, id) => {
    confirm.require({
        target: event.currentTarget,
        message: 'Sei sicuro di voler procedere con la cancellazione dell\'account selezionato?',
        icon: 'fas fa-exclamation-triangle yellow',
        rejectProps: {
            label: 'Cancel',
            severity: 'secondary',
            outlined: true
        },
        acceptProps: {
          label: 'Cancella',
          class: "bg-danger"


        },
        accept: () => {

          deleteAccount(id);
            //toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 });
        },
        reject: () => {
           
        }
    });
};
   
  async function refresh(){
    DomuWaveStore.startLoading();
    await accountStore.loadAllAccounts();
    DomuWaveStore.stopLoading();
  }
  onMounted(async () => {
    
    await refresh();
    
    
  })
  function newAccount() {
    modalCreateAccountVisible.value = true;
  }
  async function saveNewAccount() {
    const result = await createForm.value.submitForm();
    if (result) {
      modalCreateAccountVisible.value = false;
      await refresh();
    }
  }

</script>
