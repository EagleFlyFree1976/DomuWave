


<template>

  <div v-if="exchangeRateHistoryStore.edititem != null">
    <div class="card shadow mb-4">
      <div class="card-body">
        <div class="container-fluid mt-4">
          <div class="w-50">
            <div class="">


              <Form v-slot="$form" ref="formRef" :resolver @submit="onFormSubmit" class="flex flex-col gap-4 w-full sm:w-56 mt-2 ml-2">

                <div class="row mb-3">
                  <label for="name" class="col-sm-3 col-form-label">Valuta</label>
                  <div class="col-sm-9">
                    
                    <AutoComplete   :disabled="!exchangeRateHistoryStore.createmode"
                                  name="currency" optionLabel="description" v-model="editEntity.toCurrency"
                                  :suggestions="currencies" forceSelection fluid
                                  minLength="2"
                                  @complete="findCurrencies" placeholder="Seleziona la valuta" />

                    <Message v-if="$form.currency?.invalid" severity="error" size="small" variant="simple">{{ $form.currency.error?.message }}</Message>
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="targetdate" class="col-sm-3 col-form-label">
                    Data  
                  </label>
                  <div class="col-sm-9">
                    <DatePicker name="validFrom" fluid
                                v-model="editEntity.validFrom"
                                :disabled="!exchangeRateHistoryStore.createmode"
                                :dateFormat="DomuWaveStore.options.dateFormat"
                                placeholder="Seleziona la data" />
                    <Message v-if="$form.validFrom?.invalid" severity="error" size="small" variant="simple">{{ $form.validFrom.error?.message }}</Message>
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="rate" class="col-sm-3 col-form-label">
                    Tasso
                  </label>
                  <div class="col-sm-9">
                    <InputText name="rate" type="number" placeholder="Specifica il tasso" fluid v-model="editEntity.rate" class="form-control" />
                    <Message v-if="$form.rate?.invalid" severity="error" size="small" variant="simple">{{ $form.rate.error?.message }}</Message>
                  </div>
                </div>

              </Form>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
  <div v-if="exchangeRateHistoryStore.edititem == null && !DomuWaveStore.loading">
    <Error404 :backaction="/PaymentMethods/"></Error404>
  </div>
</template>
<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'


  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { useExchangeRateHistoryStore } from '@/stores/exchangeRateHistoryStore'
  import AutoComplete from 'primevue/autocomplete';
  import Button from 'primevue/button';
  import Error404 from '@/components/Error404.vue'
  import InputText from 'primevue/inputtext';
  import DatePicker from 'primevue/datepicker';
  import { Form } from '@primevue/forms';

  var saving = ref(false);
  const currencies = ref([]);
  const findCurrencies = async (event) => {
    console.log("Carico le valute", event.query);
    currencies.value = await DomuWaveStore.loadCurrencies(event.query);
    console.log("Ho caricato le valute:", currencies);
  };
  const exchangeRateHistoryStore = useExchangeRateHistoryStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()
  var editEntity = ref({ ...exchangeRateHistoryStore.edititem });
  const formSubmitResolve = ref(null);

  const formRef = ref(null);
  console.log("editEntity ", editEntity);
  const resolver = ({ values }) => {
    const errors = {};
    if (editEntity.value.toCurrency == null) {
      errors.currency = [{ message: 'Selezionare la valuta di riferimento.' }];
    }
    console.log("editEntity.value.validFrom ", editEntity.value.validFrom);
    if (editEntity.value.validFrom == null || editEntity.value.validFrom == "") {
      errors.validFrom = [{ message: 'Selezionare la data di riferimento.' }];
    }
    if (editEntity.value.rate == null) {
      errors.rate = [{ message: 'Specificare il tasso di cambio.' }];
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
        //resolve(true);
      } else {
        resolve(false);
      }
    });



  };


  const onFormSubmit = async ({ valid }) => {
    let success = false;
    if (valid) {
      saving.value = true;
      try {
        if (exchangeRateHistoryStore.createmode) {
          success = await exchangeRateHistoryStore.createEntity(editEntity.value);
        } else {
          success = await exchangeRateHistoryStore.updateEntity(editEntity.value);
        }
      } catch (err) {
        console.error("Errore nel salvataggio:", err);
        messageStore.showError("Errore durante il salvataggio del cambio valuta.");
        success = false;
      } finally {
        saving.value = false;
      }

    }

    if (formSubmitResolve.value) {
      console.log("onFormSubmit resolve", success);
      formSubmitResolve.value(success);
      console.log("onFormSubmit resolve 2", success);
      formSubmitResolve.value = null;
    }
  };
  console.log("onFormSubmit after");

  async function refresh() {

  }

  function undoNew() {

    exchangeRateHistoryStore.undoNewEntity();


  }

  onMounted(async () => {
    await refresh();
    // exchangeRateHistoryStore.newEntity();
  })


  defineExpose({
    submitForm
  })
  watch(
    () => exchangeRateHistoryStore.edititem,
    (newId) => {
      if (newId != null) {

        editEntity.value = { ...exchangeRateHistoryStore.edititem };
      }
    },
    { immediate: true }
  )
</script>
