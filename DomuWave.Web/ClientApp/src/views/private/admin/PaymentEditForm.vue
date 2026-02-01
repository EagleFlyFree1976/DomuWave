


<template>

  <div v-if="paymentMethodStore.edititem != null">
    <div class="card shadow mb-4">
      <div class="card-body">
        <div class="container-fluid mt-4">
          <div class="w-50">
            <div class="card flex justify-center mt-4">

             
              <Form v-slot="$form" ref="formRef" :resolver @submit="onFormSubmit" class="flex flex-col gap-4 w-full sm:w-56 mt-2 ml-2">

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
  </div>
  <div v-if="paymentMethodStore.edititem == null && !DomuWaveStore.loading">
    <Error404 :backaction="/PaymentMethods/"></Error404>
  </div>
</template>
<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'

  
  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
    import { usePaymentMethodStore } from '@/stores/paymentMethodStore'
  import Button from 'primevue/button';
  import Error404 from '@/components/Error404.vue'
  import InputText from 'primevue/inputtext';
  import DatePicker from 'primevue/datepicker';
  import { Form } from '@primevue/forms';

  var saving = ref(false);
  const paymentMethodStore = usePaymentMethodStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()
  var editEntity = ref({ ...paymentMethodStore.edititem });
  const formSubmitResolve = ref(null);

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

  async function submitForm() {
    return new Promise(async (resolve) => {
      if (formRef.value) {
        // Salviamo il resolver per usarlo in onFormSubmit
        formSubmitResolve.value = resolve;
        await formRef.value.submit(); // questo chiamerà onFormSubmit
        resolve(true);
      } else {
        resolve(false);
      }
    });



  };


  const onFormSubmit = async ({ valid }) => {
    let success = false;
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

    if (formSubmitResolve.value) {
      formSubmitResolve.value(success);
      formSubmitResolve.value = null;
    }
  };
  console.log("onFormSubmit after");

  async function refresh() {
    
  }

  function undoNew() {

    paymentMethodStore.undoNewEntity();


  }

  onMounted(async () => {
    await refresh();
   // paymentMethodStore.newEntity();
  })

 
  defineExpose({
    submitForm
  })
  watch(
    () => paymentMethodStore.edititem,
    (newId) => {
      if (newId != null) {

        editEntity.value = { ...paymentMethodStore.edititem };
      }
    },
    { immediate: true }
  )
</script>
