


<template>

  <div>


    <div class="card">
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">Tassi di cambio</h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea Tasso " text plain @click="newExchangeRateHistory()">
              <i class="fa fa-plus-square"></i>

            </Button>


          </div>
        </template>
      </Toolbar>
    </div>
    <div class="card shadow mb-4">
      <div class="card-header d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center gap-2">

          <AutoComplete class="d-inline-block w-auto me-2"
                        name="currency" optionLabel="description" v-model="toCurrencyId"
                        :suggestions="currencies" forceSelection fluid
                        minLength="2"
                        @item-select="filter"
                        @complete="findCurrencies" placeholder="Filtra per valuta" />

          <DatePicker name="targetDate" fluid
                  
                      @update:modelValue="onDateChange"
                      :dateFormat="DomuWaveStore.options.dateFormat"
                      placeholder="Cerca per data" />


        </div>
        <div class="d-flex justify-content-end align-items-center gap-3">
          <span>Totale: {{ exchangeRateHistoryStore.totalRecords }}</span>

          <Paginator :rows="exchangeRateHistoryStore.pageSize"
                     :totalRecords="exchangeRateHistoryStore.totalRecords"
                     :first="(exchangeRateHistoryStore.page - 1) * exchangeRateHistoryStore.pageSize"
                     @page="onPageChange" />
        </div>
      </div>
      <div class="card-body">
        <div class="table-responsive">
          <ConfirmPopup></ConfirmPopup>

        

          <table class="table table-bordered table-striped " id="dataTable" width="100%" cellspacing="0">
            <thead>
              <tr>
                <th class="text-muted">
                  <SortField v-model="sortState" field="ToCurrency" @update:modelValue="onSortChanged">
                    Name
                  </SortField>
                </th>
                <th class="text-muted">
                  <SortField v-model="sortState" field="ValidFrom" @update:modelValue="onSortChanged">
                    Data
                  </SortField>
                </th>
                <th class="text-muted">
                  <SortField v-model="sortState" field="Rate" @update:modelValue="onSortChanged">
                    Valuta
                  </SortField>
                </th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in exchangeRateHistoryStore.exchangeRateHistory" :key="row.id" >

                <td class="text-start align-middle">{{row.toCurrency.code}} - {{row.toCurrency.description}}</td>
                <td class="text-start align-middle" v-date:date="row.validFrom"></td>
                <td class="text-start align-middle">{{row.rate}}</td>

                <td class="text-start align-middle">
                  <Button @click="editExchangeRateHistory(row.id)" label="Modifica" severity="default" outlined>
                    <i class="far fa-edit"></i>
                  </Button>

                  <Button @click="confirmRemove($event, row.id)" label="Delete" severity="danger" outlined>
                    <i class="far fa-minus-square"></i>
                  </Button>


                </td>
              </tr>
              <tr v-if="exchangeRateHistoryStore.exchangeRateHistory != null && exchangeRateHistoryStore.exchangeRateHistory.length === 0">
                <td colspan="4" class="text-center text-muted fst-italic py-3">
                  Nessun tasso di cambio trovato.
                </td>
              </tr>
            </tbody>
            
            <tfoot>
              <tr>
                <td colspan="4">
                  <div class="d-flex justify-content-end align-items-center gap-3">
                    <span>Totale: {{ exchangeRateHistoryStore.totalRecords }}</span>

                    <Paginator :rows="exchangeRateHistoryStore.pageSize"
                               :totalRecords="exchangeRateHistoryStore.totalRecords"
                               :first="(exchangeRateHistoryStore.page - 1) * exchangeRateHistoryStore.pageSize"
                               @page="onPageChange" />
                  </div>
                </td>
              </tr>
            </tfoot>

          </table>
        </div>
      </div>
    </div>

    <Dialog header="Tasso di cambio" v-model:visible="modalEditVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
      <ExchangeRateHistoryEditForm ref="editForm"></ExchangeRateHistoryEditForm>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <Button type="button" label="Cancel" severity="secondary" @click="modalEditVisible = false"></Button>
        <Button type="button" label="Save" class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="savExchangeRateHistoryForm()"></Button>
      </div>
    </Dialog>
  </div>

</template>
<script setup>
  import { onMounted, ref, watch } from 'vue'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { useExchangeRateHistoryStore } from '@/stores/exchangeRateHistoryStore'
  import { useConfirm } from "primevue/useconfirm";
  import Toolbar from 'primevue/toolbar';
  import ConfirmPopup from 'primevue/confirmPopup';
  import Button from 'primevue/button';
  import ExchangeRateHistoryEditForm from './ExchangeRateHistoryEditForm.vue'
  import Dialog from 'primevue/dialog';
  import AutoComplete from 'primevue/autocomplete';
  import Paginator from 'primevue/paginator';
  import DatePicker from 'primevue/datepicker';
  import SortField from "../../../../components/SortField.vue";
  const exchangeRateHistoryStore = useExchangeRateHistoryStore();
  var modalEditVisible = ref(false);
  const currencies = ref([]);
  const targetDate = ref(null);
  const toCurrencyId = ref(null);
  const currencyAuto = ref(null);
  const sortState = ref({ field: "ValidFrom", direction: 'asc' });

  const editForm = ref(null)
  const DomuWaveStore = useDomuWaveStore();
  const confirm = useConfirm();
  const props = defineProps({
    ExchangeRateHistoryid:Number
  })

  async function deleteExchangeRateHistory(id) {
    DomuWaveStore.startLoading();
    console.log("e",exchangeRateHistoryStore);
    await exchangeRateHistoryStore.deleteExchangeRateHistory(id);
    DomuWaveStore.stopLoading();
  }
  const findCurrencies = async (event) => {
    console.log("Carico le valute", event.query);
    currencies.value = await DomuWaveStore.loadCurrencies(event.query);
    console.log("Ho caricato le valute:", currencies);
  };
  const confirmRemove = (event, id) => {
    confirm.require({
        target: event.currentTarget,
        message: 'Sei sicuro di voler procedere con la cancellazione del tasso di cambio selezionato?',
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
        accept: async () => {

          await deleteExchangeRateHistory(id);
            //toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 });
        },
        reject: () => {

        }
    });
};
  const onDateChange = async (newDate) => {
    targetDate.value = newDate;
    console.log('Data aggiornata:', newDate);
    await filter()
    // Qui puoi eseguire qualsiasi azione al momento della selezione
  };
  async function filter() {
    console.log("FILTER", targetDate);
    DomuWaveStore.startLoading();
    var filterCurrencyId = toCurrencyId != null && toCurrencyId.value != null ? toCurrencyId.value.id : null;
    var filterDateValue = targetDate != null && targetDate.value != null ? targetDate.value : null;

    await exchangeRateHistoryStore.filterAllexchangeRateHistory(filterDateValue, filterCurrencyId,sortState.value.field, sortState.value.direction);
    DomuWaveStore.stopLoading();
  }

  async function onSortChanged(newSort) {
  sortState.value = newSort;


  // qui richiami la funzione che ricarica i dati
  // ad es. exchangeRateHistoryStore.load(newSort.field, newSort.direction);

  await refresh();;
}

  async function refresh(){
    DomuWaveStore.startLoading();
    var filterCurrencyId = toCurrencyId != null && toCurrencyId.value != null ? toCurrencyId.value.id : null;
    var filterDateValue = targetDate != null && targetDate.value != null ? targetDate.value : null;
      console.log("nuovo sort rerf", sortState, sortState.value);
    await exchangeRateHistoryStore.filterAllexchangeRateHistory(filterDateValue, filterCurrencyId, sortState.value.field, sortState.value.direction);
    DomuWaveStore.stopLoading();
  }
  onMounted(async () => {

    await refresh();

    

  })
  

  watch(
    () => props.ExchangeRateHistoryid,
    (newId) => {
      if (newId != null) {
        if (newId != 0) {
          
          editExchangeRateHistory(newId)
    

          
        }
        else {
          newExchangeRateHistory();

        }


        ;
      }
    },
    { immediate: true }
  )
  async function editExchangeRateHistory(_exchangeRateHistoryId) {
    await exchangeRateHistoryStore.edit(_exchangeRateHistoryId);
    modalEditVisible.value = true;
  }
  function newExchangeRateHistory() {
    exchangeRateHistoryStore.newEntity();
    modalEditVisible.value = true;
  }
  async function onPageChange(event) {
    exchangeRateHistoryStore.onPageChange(event.page + 1);
  }
  async function savExchangeRateHistoryForm() {
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
