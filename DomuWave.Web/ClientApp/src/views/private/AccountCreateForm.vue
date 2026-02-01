


<template>

  <div v-if="accountStore.createitem != null">
     <div class="card shadow mb-4">
      <div class="card-body">
        <div class="container-fluid mt-4">
          <div class="w-50">
            <div class="flex justify-center mt-4">
              <Form v-slot="$form" ref="formRef" :resolver @submit="onFormSubmit" class="flex flex-col gap-4 w-full sm:w-56 mt-2 ml-2">

                <div class="row mb-3">
                  <label for="accountType" class="col-sm-3 col-form-label">Tipo</label>
                  <div class="col-sm-9">
                    <Select name="accountType"
                            v-model="editEntity.accountType"
                            :options="accounttypes"
                            optionLabel="description"
                            placeholder="Seleziona la tipologia di account" />

                    <Message v-if="$form.accountType?.invalid" severity="error" size="small" variant="simple">{{ $form.accountType.error?.message }}</Message>
                  </div>
                </div>

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
                <div class="row mb-3">
                  <label for="currency" class="col-sm-3 col-form-label">Valuta</label>
                  <div class="col-sm-9">


                    <AutoComplete name="currency" v-model="editEntity.currency" optionLabel="description"
                                  :suggestions="currencies" forceSelection fluid
                                  minLength="2"
                                  @complete="findCurrencies" placeholder="Seleziona la valuta" />


                    <Message v-if="$form.currency?.invalid" severity="error" size="small" variant="simple">{{ $form.currency.error?.message }}</Message>
                  </div>
                </div>

                <div class="row mb-3">
                  <label for="openDate" class="col-sm-3 col-form-label">Data apertura</label>
                  <div class="col-sm-9">
                    <DatePicker name="openDate" fluid v-model="editEntity.openDate" :dateFormat="DomuWaveStore.options.dateFormat" id="opendate"
                                placeholder="Inserisci la data di apertura" />
                    <Message v-if="$form.openDate?.invalid" severity="error" size="small" variant="simple">{{ $form.openDate.error?.message }}</Message>
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="closedDate" class="col-sm-3 col-form-label">Data chiusura</label>
                  <div class="col-sm-9">
                    <DatePicker name="closedDate" fluid v-model="editEntity.closedDate" :dateFormat="DomuWaveStore.options.dateFormat" id="closedDate"
                                placeholder="Inserisci la data di chiusura" />
                    <Message v-if="$form.closedDate?.invalid" severity="error" size="small" variant="simple">{{ $form.closedDate.error?.message }}</Message>
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="initialbalance" class="col-sm-3 col-form-label">Saldo iniziale</label>
                  <div class="col-sm-9">

                    <InputNumber v-model="editEntity.initialBalance"
                                 :min="0"
                                 :step="0.01"
                                 :useGrouping="true"
                                 :minFractionDigits="2"
                                 :maxFractionDigits="2"
                                 mode="currency"
                                 :currency="editEntity.currency != null && editEntity.currency != '' ? editEntity.currency.code : 'EUR'"
                                 locale="it-IT"
                                 showButtons class="w-100" />

                    <Message v-if="$form.initialBalance?.invalid" severity="error" size="small" variant="simple">{{ $form.type.initialBalance?.message }}</Message>

                  </div>
                </div>

              </Form>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
  <div v-if="accountStore.createitem == null && !DomuWaveStore.loading">
    <Error404 :backaction="/accounts/"></Error404>
  </div>
</template>
<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'
  
  import { useAccountStore } from '@/stores/accountStore'
  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import InputNumber from 'primevue/inputnumber';
  import Button from 'primevue/button';
  import Error404 from '../../components/Error404.vue'
  import InputText from 'primevue/inputtext';
  import DatePicker from 'primevue/datepicker';
  import Select from 'primevue/select';
  import { Form } from '@primevue/forms';
  import AutoComplete from 'primevue/autocomplete';
    var saving = ref(false);
  const accountStore = useAccountStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()
  var editEntity = ref({ ...accountStore.createitem });
  const formSubmitResolve = ref(null);
  const currencies = ref([]);
  const accounttypes = ref([]);
  const formRef = ref(null);
  console.log("editEntity ", editEntity);
  const resolver = ({ values }) => {
    const errors = {};
    if (!editEntity.value.name) {
      errors.name = [{ message: 'Il nome è obbligatorio.' }];
    }
    if (editEntity.value.accountType == null) {
      errors.accountType = [{ message: 'Specificare la tipologia di account' }];
    }
    if (editEntity.value.openDate == null || isNaN(new Date(editEntity.value.openDate))) {
      errors.openDate = [{ message: 'Il campo data apertura è obbligatorio.' }];
    }
    if (editEntity.value.closedDate != null && isNaN(new Date(editEntity.value.closedDate))) {
      errors.closedDate = [{ message: 'Il campo data chiusura non è corretto!.' }];
    }
    if (editEntity.value.currency == null) {
      errors.currency = [{ message: 'Specificare la valuta del conto' }];
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
       await accountStore.createEntity(editEntity.value);
      
       success = true;
      saving.value = false;

    }

    if (formSubmitResolve.value) {
    formSubmitResolve.value(success);
    formSubmitResolve.value = null;
  }
  };
  console.log("onFormSubmit after");

  async function refresh() {
    DomuWaveStore.startLoading();
    accounttypes.value = await DomuWaveStore.loadAccountTypes();
    DomuWaveStore.stopLoading();
  }

  function undoNew() {
 
    accountStore.undoNewEntity();
 
    
  }

  onMounted(async () => {
     await refresh();
    accountStore.newEntity();
  })
 
  const findCurrencies = async  (event) => {
    console.log("Carico le valute", event.query);
    currencies.value = await DomuWaveStore.loadCurrencies(event.query);
    console.log("Ho caricato le valute:", currencies);
  };
   
  defineExpose({
  submitForm
})
  watch(
    () => accountStore.createitem,
    (newId) => {
      if (newId != null) {

        editEntity.value = { ...accountStore.createitem };
      }
    },
    { immediate: true }
  )
</script>
