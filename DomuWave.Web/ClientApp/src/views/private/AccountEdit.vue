


<template>

  <div v-if="accountStore.edititem != null">
    <div class="card">
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">
            [{{accountStore.edititem.id}}] {{accountStore.edititem.name}}
          </h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea account" text plain @click="newEntity()">
              <i class="fa fa-plus-square"></i>
            </Button>

            <Button severity="secondary" label="Annulla" class="btn btn-warning align-content-end" :loading="saving" @click="undoNew" v-if="accountStore.createmode" />
            <Button severity="secondary" label="Salva" class="btn btn-primary align-content-end" :loading="saving" @click="submitForm" />
          </div>
        </template>
      </Toolbar>

      <AccountTabber :accountid="props.accountid"></AccountTabber>
    </div>
    <div class="card shadow mb-4">
      <div class="card-body">
        <div class="container-fluid mt-4">
          <div class="w-50">
            <div class="flex justify-center">
              <Form v-slot="$form" ref="formRef" :resolver @submit="onFormSubmit" class="flex flex-col gap-4 w-full sm:w-56 mt-2 ml-2" >

                <div class="row mb-3">
                  <label for="accountType" class="col-sm-3 col-form-label">Tipo</label>
                  <div class="col-sm-9">
                    <Select name="accountType"
                            v-if="accountStore.createmode" v-model="editEntity.accountType"
                            :options="accounttypes"
                            optionLabel="description"
                            placeholder="Seleziona la tipologia di account" />
                    <span v-else>
                      {{editEntity.accountType.description}}
                    </span>
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
    <Dialog header="Crea account" v-model:visible="modalCreateVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
      <AccountCreateForm ref="createForm"></AccountCreateForm>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;" >
        <Button type="button" label="Cancel" severity="secondary" @click="modalCreateVisible = false"></Button>
        <Button type="button" label="Save"  class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="saveNewEntity()"></Button>
      </div>
    </Dialog>
  </div>
  <div v-if="accountStore.edititem == null && !DomuWaveStore.loading">
    <Error404 :backaction="/accounts/"></Error404>
  </div>

</template>
<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'
  
  import { useAccountStore } from '@/stores/accountStore'
  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import Toolbar from 'primevue/toolbar';
  import Button from 'primevue/button';
  import InputNumber from 'primevue/inputnumber';
  import Error404 from '../../components/Error404.vue'
  import AccountCreateForm from './AccountCreateForm.vue'
  import InputText from 'primevue/inputtext';
  import DatePicker from 'primevue/datepicker';
  import Select from 'primevue/select';
  import { Form } from '@primevue/forms';
  import AutoComplete from 'primevue/autocomplete';
  import AccountTabber from './components/AccountTabber.vue'
  import Dialog from 'primevue/dialog';
  const accountStore = useAccountStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()
  const props = defineProps({
    accountid: Number
  });

  var saving = ref(false);
  var editEntity = ref({ ...accountStore.edititem });
  const createForm = ref(null)
  var modalCreateVisible = ref(false);


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
      if (accountStore.createmode) {
        await accountStore.createEntity(editEntity.value);
      }
      else {
        await accountStore.updateEntity(editEntity.value);
      }
      saving.value = false;

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
  function newEntity() {
    modalCreateVisible.value = true;
  }
  async function saveNewEntity() {
    const result = await createForm.value.submitForm();
    if (result) {
      modalCreateVisible.value = false;
    }
  }
  onMounted(async () => {
    console.log("onmounted");
    //accountStore.newEntity();
    console.log("accountStore.newEntity(); called");
    await refresh();
  })
  console.log("setup");
  accountStore.newEntity();
  console.log("accountStore.newEntity(); called on setup");
  const findCurrencies = async  (event) => {
    console.log("Carico le valute", event.query);
    currencies.value = await DomuWaveStore.loadCurrencies(event.query);
    console.log("Ho caricato le valute:", currencies);
  };
  watch(
    () => props.accountid,
    (newId) => {
      if (newId != null) {
        if (newId != 0) {
          DomuWaveStore.startLoading();
          accountStore.edit(newId);
          DomuWaveStore.stopLoading();
        }
        else {
          accountStore.newEntity();

        }


        ;
      }
    },
    { immediate: true }
  )

  watch(
    () => accountStore.edititem,
    (newId) => {
      if (newId != null) {

        editEntity.value = { ...accountStore.edititem };
      }
    },
    { immediate: true }
  )
</script>
