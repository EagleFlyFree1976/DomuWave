


<template>

  <div>


    <div class="card">
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">Metodi di pagamento</h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea Metodo di pagamento" text plain @click="newPaymentMethod()">
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
          <table class="table table-bordered table-striped" id="dataTable" width="100%" cellspacing="0">
            <thead>
              <tr>
                <th class="text-muted">Name</th>
                <th class="text-muted">Description</th>

                <th></th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="row in paymentMethodStore.paymentmethods" :key="row.id"
                  :class="{'row-disabled': !row.enabled}">

                <td>{{row.name}}</td>
                <td>{{row.description}}</td>

                <td>

                  
                  <Button @click="editPaymentMethod(row.id)" label="Modifica" severity="default" outlined>
                    <i class="far fa-edit"></i>
                  </Button>

                  <Button @click="confirmRemove($event, row.id)" label="Delete" severity="danger" outlined>
                    <i class="far fa-minus-square"></i>
                  </Button>
                  <Button @click="disable(row.id)" label="Disabilita" severity="success" outlined v-if="row.enabled" title="Disabilita">
                    <i class="fas fa-toggle-off"></i>
                  </Button>
                  <Button @click="enable(row.id)" label="Abilita" severity="danger" outlined v-else title="Abilita">
                    <i class="fas fa-toggle-on"></i>
                  </Button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <Dialog header="Metodo di pagamento" v-model:visible="modalEditVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
      <PaymentEditForm ref="editForm"></PaymentEditForm>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <Button type="button" label="Cancel" severity="secondary" @click="modalEditVisible = false"></Button>
        <Button type="button" label="Save" class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="savePaymentForm()"></Button>
      </div>
    </Dialog>
  </div>

</template>
<script setup>
  import { onMounted, ref, watch } from 'vue'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { usePaymentMethodStore } from '@/stores/paymentMethodStore'
  import { useConfirm } from "primevue/useconfirm";
  import Toolbar from 'primevue/toolbar';
  import ConfirmPopup from 'primevue/confirmPopup';
  import Button from 'primevue/button';
  import PaymentEditForm from './PaymentEditForm.vue'
  import Dialog from 'primevue/dialog';
  const paymentMethodStore = usePaymentMethodStore();
  var modalEditVisible = ref(false);
  const editForm = ref(null)
  const DomuWaveStore = useDomuWaveStore();
  const confirm = useConfirm();
  const props = defineProps({
    paymentmethodid:Number
  })

  async function deletePaymentMethod(id) {
    DomuWaveStore.startLoading();
    await paymentMethodStore.deletePaymentMethod(id);
    DomuWaveStore.stopLoading();
  }
  const disable = async (id) => {
    DomuWaveStore.startLoading();
    await paymentMethodStore.disablePaymentMethod(id);
    DomuWaveStore.stopLoading();
  }
  const enable = async (id) => {
    DomuWaveStore.startLoading();
    await paymentMethodStore.enablePaymentMethod(id);
    DomuWaveStore.stopLoading();
  }
  const confirmRemove = (event, id) => {
    confirm.require({
        target: event.currentTarget,
        message: 'Sei sicuro di voler procedere con la cancellazione del metodo di pagamento selezionato?',
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

          deletePaymentMethod(id);
            //toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 });
        },
        reject: () => {

        }
    });
};

  async function refresh(){
    DomuWaveStore.startLoading();
    await paymentMethodStore.loadAllPaymentMethods();
    DomuWaveStore.stopLoading();
  }
  onMounted(async () => {

    await refresh();

    

  })
  watch(
    () => props.paymentmethodid,
    (newId) => {
      if (newId != null) {
        if (newId != 0) {
          
          editPaymentMethod(newId)
    

          
        }
        else {
          newPaymentMethod();

        }


        ;
      }
    },
    { immediate: true }
  )
  async function editPaymentMethod(_paymentMethodId) {
    await paymentMethodStore.edit(_paymentMethodId);
    modalEditVisible.value = true;
  }
  function newPaymentMethod() {
    paymentMethodStore.newEntity();
    modalEditVisible.value = true;
  }
  async function savePaymentForm() {
    const result = await editForm.value.submitForm();
    if (result) {
      setTimeout(async function () {
        await refresh();
        modalEditVisible.value = false;
      }, 500);
      
      
    }
  }

</script>


<style scoped>
  .row-disabled {
    opacity: 0.5;
    pointer-events: all;
  }


</style>
