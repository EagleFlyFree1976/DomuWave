


<template>

  <div v-if="paymentMethodStore.edititem != null">
    <div class="card">
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">
            [{{paymentMethodStore.edititem.id}}] {{paymentMethodStore.edititem.name}}
          </h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea metodo di pagamento" text plain @click="newEntity()">
              <i class="fa fa-plus-square"></i>
            </Button>

            <Button severity="secondary" label="Annulla" class="btn btn-warning align-content-end" :loading="saving" @click="undoNew" v-if="paymentMethodStore.createmode" />
            <Button severity="secondary" label="Salva" class="btn btn-primary align-content-end" :loading="saving" @click="submitForm" />
          </div>
        </template>
      </Toolbar>
    </div>
    <div class="card shadow mb-4">
      <div class="card-body">
        <div class="container-fluid mt-4">
          <div class="w-50">
            <div class="card flex justify-center">
              <Form v-slot="$form" ref="formRef" :resolver @submit="onFormSubmit" class="flex flex-col gap-4 w-full sm:w-56">

                <div class="row mb-3">
                  <label for="name" class="col-sm-3 col-form-label">Nome</label>
                  <div class="col-sm-9">
                    <InputText name="name" type="text" placeholder="Inserisci il nome" fluid v-model="editEntity.name" class="form-control" />
                   
                    <Message v-if="$form.name?.invalid" severity="error" size="small" variant="simple">{{ $form.name.error?.message }}</Message>
                  </div>
                </div>

                
                <div class="row mb-3">
                  <label for="description" class="col-sm-3 col-form-label">Descrizione</label>
                  <div class="col-sm-9">
                    <InputText name="description" type="text" placeholder="Inserisci la descrizione" fluid v-model="editEntity.description" class="form-control" />
                    <Message v-if="$form.description?.invalid" severity="error" size="small" variant="simple">{{ $form.description.error?.message }}</Message>
                  </div>
                </div>
                


              </Form>
            </div>
          </div>
        </div>

      </div>
    </div>
    <Dialog header="Crea metodo di pagamento" v-model:visible="modalCreateVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
      <PaymentEditForm ref="createForm"></PaymentEditForm>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;" >
        <Button type="button" label="Cancel" severity="secondary" @click="modalCreateVisible = false"></Button>
        <Button type="button" label="Save"  class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="saveNewEntity()"></Button>
      </div>
    </Dialog>
  </div>
  <div v-if="paymentMethodStore.edititem == null && !DomuWaveStore.loading">
    <Error404 :backaction="/PaymentMethods/"></Error404>
  </div>

</template>
<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'
  
  import { usePaymentMethodStore } from '@/stores/paymentMethodStore'
  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import Toolbar from 'primevue/toolbar';
  import Button from 'primevue/button';
 
  import Error404 from '@/components/Error404.vue'
  import PaymentEditForm from './PaymentEditForm.vue'
  import InputText from 'primevue/inputtext';
  import DatePicker from 'primevue/datepicker';
  import Select from 'primevue/select';
  import { Form } from '@primevue/forms';
  import AutoComplete from 'primevue/autocomplete';
  import AccountTabber from './../components/AccountTabber.vue'
  import Dialog from 'primevue/dialog';
  const paymentMethodStore = usePaymentMethodStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()
  const props = defineProps({
    paymentmethodid: Number
  });

  var saving = ref(false);
  var editEntity = ref({ ...paymentMethodStore.edititem });
  const createForm = ref(null)
  var modalCreateVisible = ref(false);


 
  const formRef = ref(null);
  console.log("editEntity ", editEntity);
  const resolver = ({ values }) => {
    const errors = {};
    if (!editEntity.value.name) {
      errors.name = [{ message: 'Il nome è obbligatorio.' }];
    }
     
    return {
      // (Optional) Used to pass current form values to submit event.
      errors
    };
  };
  console.log("onFormSubmit pre");

  const submitForm = async () => {
    if (formRef.value)
    {
      await formRef.value.submit(); // questo attiva la validazione e chiama onFormSubmit
    }
  };
  const onFormSubmit = async ({ valid }) => {
    if (valid) {
      saving.value = true;
      console.log("editEntity", editEntity.value);
      if (paymentMethodStore.createmode) {
        await paymentMethodStore.createEntity(editEntity.value);
      }
      else {
        await paymentMethodStore.updateEntity(editEntity.value);
      }
      saving.value = false;

    }
  };
  console.log("onFormSubmit after");

  async function refresh() {
     
  }

  function undoNew() {
 
    paymentMethodStore.undoNewEntity();
 
    
  }
  function newEntity() {
    console.log("newEntity");
    modalCreateVisible.value = true;
  }
  async function saveNewEntity() {
    const result = await createForm.value.submitForm();
    if (result) {
      modalCreateVisible.value = false;
    }
  }
  onMounted(async () => {
     
    await refresh();
  })
 
   
  watch(
    () => props.paymentmethodid,
    (newId) => {
      if (newId != null) {
        if (newId != 0) {
          DomuWaveStore.startLoading();
          console.log("Call paymentMethodStore.edit");
          paymentMethodStore.edit(newId);
           console.log("paymentMethodStore.edit called");
          DomuWaveStore.stopLoading();
        }
        else {
          paymentMethodStore.newEntity();

        }
      }
    },
    { immediate: true }
  )

  watch(
    () => paymentMethodStore.edititem,
    (newId) => {
      if (newId != null) {
         console.log("watch newid", newId);
        editEntity.value = { ...paymentMethodStore.edititem };
        console.log("END watch newid", newId);
      }
    },
    { immediate: true }
  )
</script>
