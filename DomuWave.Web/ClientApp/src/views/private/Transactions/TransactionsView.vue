


<template>

  <div>
    <Drawer v-model:visible="showFilterVisible" position="right" :modal="true" style="width: 30rem">
      <h2 class="mb-4">Specificare i filtri</h2>
      <p>
        <div class="container-fluid mt-4">
          <div class="row mb-3">
            <label for="account" class="col-sm-3 col-form-label">Account</label>
            <div class="col-sm-9">

              <Select name="account"
                      v-model="transactionStore.currentFilter.accountId"
                      :options="transactionStore.filterLookups.accounts"
                      optionLabel="name"
                      optionValue="id"
                      placeholder="Seleziona l''account" class="w-full" />

            </div>
          </div>
          <div class="row mb-3">
            <label for="category" class="col-sm-3 col-form-label">Categoria</label>
            <div class="col-sm-9">

              <TreeSelect v-model="storeSelectionKeys" :options="treeCategorySource" placeholder="Seleziona categoria" class="w-full treeselect-fixed" showClear="true" />




            </div>
          </div>
          <div class="row mb-3">
            <label for="filterFromDate" class="col-sm-3 col-form-label">Data Da</label>
            <div class="col-sm-9">
              <DatePicker id="filterFromDate"
                          v-model="transactionStore.currentFilter.fromDate"
                          :maxDate="transactionStore.currentFilter.toDate"
                          dateFormat="dd/mm/yy"
                          placeholder="Seleziona un intervallo"
                          showIcon class="w-full"/>

            </div>
          </div>
          <div class="row mb-3">
            <label for="filterToDate" class="col-sm-3 col-form-label">Data A</label>
            <div class="col-sm-9">
              <DatePicker id="filterToDate" v-model="transactionStore.currentFilter.toDate"
                          :minDate="transactionStore.currentFilter.fromDate"
                          dateFormat="dd/mm/yy"
                          placeholder="Seleziona un intervallo"
                          showIcon class="w-full"/>

            </div>
          </div>
          <div class="row mb-3">
            <label for="filterTransactionType" class="col-sm-3 col-form-label">Tipo</label>
            <div class="col-sm-9">
              <Dropdown v-model="transactionStore.currentFilter.transactionType"
                        :options="transactionTypeOptions"
                        optionLabel="label"
                        optionValue="value"
                        placeholder="Seleziona un'opzione"
                        showClear class="w-full"/>

            </div>
          </div>
          <div class="row mb-3" v-if="transactionStore.currentFilter.transactionType != null && transactionStore.currentFilter.transactionType == 't'">
            <label for="filterFlowDirection" class="col-sm-3 col-form-label">Flusso</label>
            <div class="col-sm-9">
              <Dropdown v-model="transactionStore.currentFilter.flowDirection"
                        :options="flowDirectionOptions"
                        optionLabel="label"
                        optionValue="value"
                        placeholder="Seleziona un'opzione"
                        showClear class="w-full"/>

            </div>
          </div>

          <div class="row mb-3">
            <label for="status" class="col-sm-3 col-form-label">Stato</label>
            <div class="col-sm-9">

              <Select name="status"
                      v-model="transactionStore.currentFilter.status"
                      :options="transactionStore.filterLookups.statuses"
                      optionLabel="description"
                      optionValue="id"
                      placeholder="Seleziona lo stato" class="w-full"/>

            </div>
          </div>
          <div class="row mb-3">
            <label for="note" class="col-sm-3 col-form-label">Note</label>
            <div class="col-sm-9">

              <InputText name="note"
                         v-model="transactionStore.currentFilter.note"
                         placeholder="Cerca nel campo note" class="w-full"/>

            </div>

          </div>

          <div class="row mb-3">
            <label for="account" class="col-sm-3 col-form-label"></label>
            <div class="col-sm-9">

              <Button as="a" label="Filtra" text plain @click="filter()">
                <i class="fa fa-filter"></i>

              </Button>

            </div>
          </div>


        </div>


      </p>


    </Drawer>

    <div class="card">
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">Transazioni</h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea" text plain @click="newTransaction()">
              <i class="fa fa-plus-square"></i>

            </Button>

            <Button as="a" label="Filtri" text plain @click="showFilter()">
              <i class="fa fa-filter"></i>

            </Button>


          </div>
        </template>
      </Toolbar>
    </div>
    <div class="card shadow mb-4">
      <div class="card-header d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center gap-2">





        </div>
        <div class="d-flex justify-content-end align-items-center gap-3">
          <span>Totale: {{ transactionStore.totalRecords }}</span>

          <Paginator :rows="transactionStore.pageSize"
                     :totalRecords="transactionStore.totalRecords"
                     :first="(transactionStore.page - 1) * transactionStore.pageSize"
                     @page="onPageChange" />
        </div>
      </div>
      <div class="card-body">
        <div class="table-responsive">
          <ConfirmPopup></ConfirmPopup>



          <table class="table table-bordered table-striped" id="dataTable" width="100%" cellspacing="0">
            <thead>
              <tr>
                <th class="wp-5">
                  <SortField v-model="sortState" field="TransactionType" @update:modelValue="onSortChanged">

                  </SortField>

                </th>
                <th class="text-muted wp-25">

                  <SortField v-model="sortState" field="TransactionDate" @update:modelValue="onSortChanged">
                    Data
                  </SortField>
                </th>
                <th class="text-muted wp-25">

                  <SortField v-model="sortState" field="status" @update:modelValue="onSortChanged">
                    Stato
                  </SortField>
                </th>
                <th class="text-muted wp-200">
                  <SortField v-model="sortState" field="Account" @update:modelValue="onSortChanged">
                    Account
                  </SortField>

                </th>
                <th class="text-muted wp-200">
                  <SortField v-model="sortState" field="Beneficiary" @update:modelValue="onSortChanged">
                    Beneficiario
                  </SortField>

                </th>
                <th class="text-muted wp-200">
                  <SortField v-model="sortState" field="Category" @update:modelValue="onSortChanged">
                    Categoria
                  </SortField>

                </th>
                <th class="text-muted wp-35">
                  <SortField v-model="sortState" field="AmountInAccountCurrency" @update:modelValue="onSortChanged">
                    Importo
                  </SortField>
                </th>
                <th class="text-muted wp-35">
                  
                    Saldo
                  
                </th>
                <th class="text-muted wp-100">
                  <SortField v-model="sortState" field="Note" @update:modelValue="onSortChanged">
                    Note
                  </SortField>

                </th>

                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in transactionStore.transactions" :key="row.id">
                <td class="text-start align-middle" :class="transactionTypeClasses[row.transactionType]" :title="transactionTypeDescription[row.transactionType * (row.flowDirection+15) ]">
                  <i :class="transactionTypeIcon[row.transactionType * (row.flowDirection+15)]"></i>  
                </td>
               
                <td class="text-start align-middle" v-date:date="row.transactionDate" :class="row.status.cssClass"></td>
                <td class="text-start align-middle" :class="row.status.cssClass">{{row.status.description}}</td>
                <td class="text-start align-middle" :class="row.status.cssClass">
                  <a :href="`/accounts/${row.account.id}/transactions`" target="_blank">
                    {{ row.account.code }}
                  </a>

                  <div v-if="row.destinationAccount != null && row.transactionType == 2">
                    <i :class="{'fa fa-arrow-right':row.flowDirection == 1,'fa fa-arrow-left':row.flowDirection == 0 }"></i>
                    <a :href="`/accounts/${row.destinationAccount.id}/transactions`" target="_blank">
                      {{ row.destinationAccount.code }}
                    </a>

                    
                  </div>
                </td>
                <td class="text-start align-middle " :class="row.status.cssClass">
                  {{row.beneficiary.description}}
                </td>
                <td class="text-start align-middle " :class="row.status.cssClass">
                  {{row.category.description}}
                </td>
                <td class="text-start align-middle" v-currency="{ amount: row.amountInAccountCurrency, locale: 'it-IT', currency: row.accountCurrency .code }" :class="row.status.cssClass"></td>
                <td class="text-start align-middle" v-currency="{ amount: row.accountBalance, locale: 'it-IT', currency: row.accountCurrency.code }" :class="row.status.cssClass"></td>
                <td class="text-start align-middle" :class="row.status.cssClass">{{row.description}}</td>
                <td class="text-start align-middle" :class="row.status.cssClass">
                  <Button @click="editTransaction(row.id)" label="Modifica" severity="default" outlined>
                    <i class="far fa-edit"></i>
                  </Button>
                  <Button @click="confirmRemove($event, row.id)" label="Delete" severity="danger" outlined>
                    <i class="far fa-minus-square"></i>
                  </Button>
                </td>
              </tr>
              <tr v-if="transactionStore.Transaction != null && transactionStore.Transaction.length === 0">
                <td colspan="4" class="text-center text-muted fst-italic py-3">
                  Nessuna transazione trovata
                </td>
              </tr>
            </tbody>
         
          </table>
        </div>
      </div>
 
        <div class="card-header d-flex justify-content-between align-items-center">
          <div class="d-flex align-items-center gap-2">





          </div>
          <div class="d-flex justify-content-end align-items-center gap-3">
            <span>Totale: {{ transactionStore.totalRecords }}</span>

            <Paginator :rows="transactionStore.pageSize"
                       :totalRecords="transactionStore.totalRecords"
                       :first="(transactionStore.page - 1) * transactionStore.pageSize"
                       @page="onPageChange" />
          </div>
        </div>
 
      </div>

      <Dialog header="Transazione" v-model:visible="modalEditVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
        <TransactionEditForm ref="editForm"></TransactionEditForm>
        <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
          <Button type="button" label="Cancel" severity="secondary" @click="modalEditVisible = false"></Button>
          <Button type="button" label="Save" class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="saveTransactionForm()"></Button>
        </div>
      </Dialog>
    </div>

</template>
<script setup>
  import { useRoute, useRouter } from 'vue-router'

  import { onMounted, ref, watch, computed } from 'vue'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { useAccountStore } from '@/stores/accountStore'
  import { useTransactionStore } from '@/stores/transactionStore'
  import { useConfirm } from "primevue/useconfirm";
  import Toolbar from 'primevue/toolbar';
  import ConfirmPopup from 'primevue/confirmPopup';
  import Button from 'primevue/button';
  import Select from 'primevue/select';
  import TransactionEditForm from './TransactionEditForm.vue'
  import Dialog from 'primevue/dialog';
  import AutoComplete from 'primevue/autocomplete';
  import Paginator from 'primevue/paginator';
  import DatePicker from 'primevue/datepicker';
  import SortField from "../../../components/SortField.vue";
  import TreeSelect from "primevue/treeselect";
  import Drawer from 'primevue/drawer'
  import Dropdown from "primevue/dropdown";
  import InputText from 'primevue/inputtext';

  const route = useRoute();
  const router = useRouter();


  const transactionStore = useTransactionStore();
  const accountStore = useAccountStore();
  var modalEditVisible = ref(false);
  const currencies = ref([]);
  const showFilterVisible = ref(false);

  const dateRange = ref(null);
  const currencyAuto = ref(null);
  const sortState = ref({ field: "ValidFrom", direction: 'asc' });

  const editForm = ref(null)
  const DomuWaveStore = useDomuWaveStore();
  const confirm = useConfirm();
  const props = defineProps({
    Transactionid: Number
  });

  const nodes = ref(null);
  const selectedValue = ref(null);
  const transactionTypeOptions = [
    { label: "Deposito", value: "e" },
    { label: "Uscita", value: "u" },
    { label: "Trasferimento", value: "t" }
  ];
  const flowDirectionOptions = [
    { label: "Entrata", value: "i" },
    { label: "Uscita", value: "o" },
  ];
  function showFilter() {
    showFilterVisible.value = !showFilterVisible.value;
  }

  const transactionTypeClasses = {
    1: 'bg-danger text-white',
    0: 'bg-success text-white',
    2: 'bg-dark text-white'
  }
  const transactionTypeIcon = {
    0: 'fa fa-arrow-down',
    16: 'fa fa-arrow-up',
    30: 'fas fa-exchange-alt text-success',
    32: 'fas fa-exchange-alt text-danger'

  }
  const transactionTypeDescription = {
    0: 'Entrata',
    16: 'Uscita',
    30: 'Trasferimento in entrata',
    32: 'Trasferimento in uscita'
  }

  function toTreeNodes(items) {
    const map = new Map()
    for (const c of items) {
      map.set(c.id, {
        key: c.id,
        label: c.name,
        data: c,
        children: []
      })
    }
    const roots = []
    for (const c of items) {
      const node = map.get(c.id)
      const parentId = c.parent?.id ?? null
      if (parentId && map.has(parentId)) {
        map.get(parentId).children.push(node)
      } else {
        roots.push(node)
      }
    }
    // opzionale: rendi i nodi con figli non selezionabili (se vuoi forzare la scelta sulle foglie)
    // roots.forEach(r => { if (r.children?.length) r.selectable = false })
    return roots
  }


  // Computed che funge da ponte diretto tra TreeSelect e store
  const storeSelectionKeys = computed({
    get() {
      const id = transactionStore.currentFilter.categoryId;
      return id != null ? { [id]: true } : null;
    },
    set(newVal) {
      console.log("NEWVVV", newVal);
      transactionStore.currentFilter.categoryId = newVal
        ? parseInt(Object.keys(newVal)[0])
        : null;
    }
  });

  watch(() => transactionStore.currentFilter.transactionType, (newV) => {
    if (newV != null) {
      if (newV != 't') {
        transactionStore.currentFilter.flowDirection = null;
      }
    }
  });
  const treeCategorySource = computed(() => {
    console.log("TreeCS");
    let sourceData = transactionStore.filterLookups.categories;
    var treeSource = toTreeNodes(sourceData);



    return treeSource;
  })

  async function deleteTransaction(id) {
    DomuWaveStore.startLoading();
    console.log("e", transactionStore);
    await transactionStore.deleteTransaction(id);
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

        await deleteTransaction(id);
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

  async function  setFilterOnRouting(){
    var filterAccountId = transactionStore.currentFilter.accountId != null ? transactionStore.currentFilter.accountId : null;
    var filterCategoryId = transactionStore.currentFilter.categoryId != null ? transactionStore.currentFilter.categoryId : null;
    var filterFromDateValue = transactionStore.currentFilter.fromDate != null ? transactionStore.currentFilter.fromDate : null;
    var filterToDateValue = transactionStore.currentFilter.toDate != null ? transactionStore.currentFilter.toDate : null;
    var filterTransactionTypeId = transactionStore.currentFilter.transactionType != null ? transactionStore.currentFilter.transactionType : null;
    var filterFlowDirectionId = transactionStore.currentFilter.flowDirection != null ? transactionStore.currentFilter.flowDirection : null;
    var filterStatusId = transactionStore.currentFilter.status != null ? transactionStore.currentFilter.status : null;
    var filterNote = transactionStore.currentFilter.note != null ? transactionStore.currentFilter.note : null;


    const params = {
      accountId: transactionStore.currentFilter.accountId || undefined,
      categoryId: transactionStore.currentFilter.categoryId || undefined,
      fromDate: transactionStore.currentFilter.fromDate
        ? transactionStore.currentFilter.fromDate.toISOString().split('T')[0]
        : undefined,
      toDate: transactionStore.currentFilter.toDate
        ? transactionStore.currentFilter.toDate.toISOString().split('T')[0]
        : undefined,
      transactionType: transactionStore.currentFilter.transactionType || undefined,
      flowDirection: transactionStore.currentFilter.flowDirection || undefined,
      status: transactionStore.currentFilter.status || undefined,
      note: transactionStore.currentFilter.note || undefined,
      sortBy: sortState.value.field || undefined,
      sortOrder: sortState.value.direction || undefined,
      currentPage: transactionStore.page || undefined

    };
    console.log("Aggiorna la query string senza ricaricare la pagina", params);
    // Aggiorna la query string senza ricaricare la pagina
    router.replace({ query: params });
  };
  async function filter() {

    DomuWaveStore.startLoading();

    await  setFilterOnRouting();
    var filterAccountId = transactionStore.currentFilter.accountId != null ? transactionStore.currentFilter.accountId : null;
    var filterCategoryId = transactionStore.currentFilter.categoryId != null ? transactionStore.currentFilter.categoryId : null;
    var filterFromDateValue = transactionStore.currentFilter.fromDate != null ? transactionStore.currentFilter.fromDate : null;
    var filterToDateValue = transactionStore.currentFilter.toDate != null ? transactionStore.currentFilter.toDate : null;
    var filterTransactionTypeId = transactionStore.currentFilter.transactionType != null ? transactionStore.currentFilter.transactionType : null;
    var filterFlowDirectionId = transactionStore.currentFilter.flowDirection != null ? transactionStore.currentFilter.flowDirection : null;
    var filterStatusId = transactionStore.currentFilter.status != null ? transactionStore.currentFilter.status : null;
    var filterNote = transactionStore.currentFilter.note != null ? transactionStore.currentFilter.note : null;

    await transactionStore.filterAlltransaction(null, filterAccountId, filterCategoryId, filterFromDateValue, filterToDateValue, filterTransactionTypeId, filterFlowDirectionId, filterStatusId, filterNote,
      sortState.value.field, sortState.value.direction, transactionStore.page)

    DomuWaveStore.stopLoading();
  }

  async function onSortChanged(newSort) {
    sortState.value = newSort;


    // qui richiami la funzione che ricarica i dati
    // ad es. transactionStore.load(newSort.field, newSort.direction);

    await refresh();;
  }

  async function refresh() {
    DomuWaveStore.startLoading();
    await transactionStore.loadFilterLookups();
    await accountStore.loadLookups();

    await filter();
  }
  onMounted(async () => {
    console.log("CategoryId iniziale:", transactionStore.currentFilter.categoryId);
    transactionStore.currentFilter.accountId = null;


    const q = route.query;

    transactionStore.currentFilter.accountId = q.accountId ? parseInt(q.accountId) : null;
    transactionStore.currentFilter.categoryId = q.categoryId ? parseInt(q.categoryId) : null;
    transactionStore.currentFilter.fromDate = q.fromDate ? new Date(q.fromDate) : null;
    transactionStore.currentFilter.toDate = q.toDate ? new Date(q.toDate) : null;
    transactionStore.currentFilter.transactionType = q.transactionType || null;
    transactionStore.currentFilter.flowDirection = q.flowDirection || null;
    transactionStore.currentFilter.status = q.status ? parseInt(q.status) : null;
    transactionStore.currentFilter.note = q.note || null;
    transactionStore.page = q.currentPage ? parseInt(q.currentPage) : 1;
    sortState.value.field = q.sortBy || "TransactionDate";
    sortState.value.direction = q.sortOrder || "desc";

    await refresh();
    console.log("CategoryId iniziale 22:", transactionStore.currentFilter.categoryId);


  })


  watch(
    () => props.Transactionid,
    (newId) => {
      if (newId != null) {
        if (newId != 0) {

          editTransaction(newId)



        }
        else {
          newTransaction();

        }


        ;
      }
    },
    { immediate: true }
  )
  async function editTransaction(_TransactionId) {
    await transactionStore.edit(_TransactionId);
    modalEditVisible.value = true;
  }
  function newTransaction() {
    transactionStore.newEntity();
    modalEditVisible.value = true;
  }
  async function onPageChange(event) {


    transactionStore.onPageChange(event.page + 1);
    await setFilterOnRouting();
  }
  async function saveTransactionForm() {
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

  .treeselect-fixed .p-treeselect-label {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

</style>
