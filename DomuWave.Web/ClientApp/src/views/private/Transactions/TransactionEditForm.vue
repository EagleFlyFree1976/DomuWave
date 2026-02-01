


<template>

  <div v-if="transactionStore.edititem != null">
    <div class="card shadow mb-4">
      <div class="card-body">
        <div class="container-fluid mt-4">
          <div>
            <Form v-slot="$form" ref="formRef" :resolver @submit="onFormSubmit" class="flex flex-col gap-4 w-full sm:w-56 mt-2 ml-2">
              <div class="row ">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="transactionDate" class="col-form-label">
                      Data  
                    </label>
                    <div>
                     
                      <DatePicker name="transactionDate" fluid
                                  v-model="editEntity.transactionDate"
                                  :dateFormat="DomuWaveStore.options.dateFormat"
                                  @update:modelValue="onTransactionDateChange"
                                  placeholder="Seleziona la data" class="w-100" />
                      <Message v-if="$form.transactionDate?.invalid" severity="error" size="small" variant="simple">{{ $form.transactionDate.error?.message }}</Message>
                    </div>
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="status" class="col-form-label">Stato</label>
                    <div>

                      <Select name="status"
                              v-model="editEntity.status"
                              :options="transactionStore.filterLookups.statuses"
                              optionLabel="description"
                              placeholder="Seleziona lo stato" class="w-100" />

                      <Message v-if="$form.status?.invalid" severity="error" size="small" variant="simple">{{ $form.status.error?.message }}</Message>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="transactionType" class="col-form-label">Tipo</label>
                    <div>

                      <Select name="transactionType"
                              v-model="editEntity.transactionType"
                              :options="transactionStore.filterLookups.transactionTypes"
                              optionLabel="description"
                              optionValue="id"
                              placeholder="Seleziona il tipo"
                              @change="onTransactionTypeChange" class="w-100" />

                      <Message v-if="$form.transactionType?.invalid" severity="error" size="small" variant="simple">{{ $form.transactionType.error?.message }}</Message>
                    </div>
                  </div>
                </div>

              </div>
              <div class="row mb-3">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="account" class="col-form-label">Account</label>
                    <div>

                      <Select name="account"
                              v-model="editEntity.account"
                              :options="accountlookups"
                              optionLabel="code"
                              placeholder="Seleziona l'account"
                              @change="onAccountChange" class="w-100" />

                      <Message v-if="$form.account?.invalid" severity="error" size="small" variant="simple">{{ $form.account.error?.message }}</Message>
                    </div>
                  </div>
                </div>

                <div class="col-md-6">
                  <div class="mb-3" v-if="isTrasfer">
                    <label for="accountTo" class="col-form-label">Account a</label>
                    <div>

                      <Select name="accountTo"
                              v-model="editEntity.destinationAccount"
                              :options="accountlookups"
                              optionLabel="code"
                              placeholder="Seleziona l'account di destinazione"
                              @change="onAccountDestinationChange" class="w-100" />

                      <Message v-if="$form.accountTo?.invalid" severity="error" size="small" variant="simple">{{ $form.accountTo.error?.message }}</Message>
                    </div>
                  </div>
                </div>

              </div>

              <div class="row mb-3">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="paymentmethod" class="col-form-label">Metodo di pagamento</label>
                    <div>



                      <Select name="paymentmethod"
                              v-model="editEntity.paymentMethod"
                              :options="transactionStore.filterLookups.paymentMethods"
                              optionLabel="description"
                              placeholder="Seleziona il metodo di pagamento" class="w-100" />


                      <Message v-if="$form.paymentmethod?.invalid" severity="error" size="small" variant="simple">{{ $form.paymentmethod.error?.message }}</Message>
                    </div>

                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">

                    <label for="amount" class="col-form-label">Importo</label>
                    <div>
                       
                      <InputNumber v-model="editEntity.amount"
                                   :min="0"
                                
                                   :step="0.01"
                                   :useGrouping="true"
                                   :minFractionDigits="2"
                                   :maxFractionDigits="2"
                                   mode="currency"
                                   :currency="editEntity.currency != null && editEntity.currency != '' ? editEntity.currency.code : 'EUR'"
                                   locale="it-IT"
                                   showButtons class="w-100" />

                      <Message v-if="$form.amount?.invalid" severity="error" size="small" variant="simple">{{ $form.type.amount?.message }}</Message>
                    </div>


                  </div>
                </div>
              </div>
              <div class="row mb-3">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="beneficiary" class="col-form-label">Beneficiario</label>
                    <div>
                      <AutoComplete name="beneficiary" optionLabel="description" v-model="editEntity.beneficiary"
                                    :suggestions="beneficiaries" forceSelection="false" fluid
                                    minLength="2"
                                    @item-select="onBeneficiaryChange"
                                    @update:model-value="onBeneficiaryChange"
                                    @complete="findBeneficiaries" placeholder="Seleziona il beneficiario" class="w-100">
                        <template #option="slotProps">
                          <div class="flex items-center">
                            <div>{{ slotProps.option.description }}</div>
                          </div>
                        </template>

                      </AutoComplete>

                      <Message v-if="$form.beneficiary?.invalid" severity="error" size="small" variant="simple">{{ $form.beneficiary.error?.message }}</Message>
                    </div>

                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">

                    <label for="category" class="col-form-label">Categoria</label>
                    <div>
                      <AutoComplete name="category" optionLabel="description" v-model="editEntity.category"
                                    :suggestions="categories" forceSelection="false" fluid
                                    minLength="2"
                                    @item-select="onCategoryChange"
                                    @update:model-value="onCategoryChange"
                                    @complete="findCategories" placeholder="Seleziona la categoria" class="w-100">
                        <template #option="slotProps">
                          <div class="flex items-center">
                            <div>{{ slotProps.option.description }}</div>
                          </div>
                        </template>

                      </AutoComplete>

                      <Message v-if="$form.category?.invalid" severity="error" size="small" variant="simple">{{ $form.category.error?.message }}</Message>
                    </div>


                  </div>
                </div>
              </div>
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="name" class="col-form-label">Valuta</label>
                    <div>
                    
                      <AutoComplete name="currency" optionLabel="description" v-model="editEntity.currency"
                                    :suggestions="currencies" fluid
                                    minLength="2"
                                  :disabled="!enableCurrency"
                                    @item-select="onCurrencyChange"
                                    @update:model-value="onCurrencyChange"
                                    @complete="findCurrencies" placeholder="Seleziona la valuta" class="w-100" />
                      <Message v-if="$form.currency?.invalid" severity="error" size="small" variant="simple">{{ $form.currency.error?.message
                      }}</Message>
                    </div>
                  </div>
                </div>

                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="rate" class="col-form-label">
                      Tasso
                    </label>
                    <div>
                      <InputText name="rate" type="number"    :disabled="!enableRate" placeholder="Specifica il tasso" fluid v-model="editEntity.rate" class="w-100" />
                      <Message v-if="$form.rate?.invalid" severity="error" size="small" variant="simple">
                        {{
 $form.rate.error?.message
                        }}
                      </Message>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row">
                <div class="col-md-12">
                  <div class="mb-3">
                    <label for="name" class="col-form-label">Note</label>
                    <div>
                      <Textarea v-model="editEntity.description" rows="5" cols="30" class="w-100" />

                    </div>
                  </div>
                </div>
              </div>
            </Form>



          </div>
        </div>
      </div>

    </div>
  </div>

  <div v-if="transactionStore.edititem == null && !DomuWaveStore.loading">
    <Error404 :backaction="/PaymentMethods/"></Error404>
  </div>
</template>
<script setup>

  import { onMounted, watch, computed, reactive, ref } from 'vue'
  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { useAccountStore } from '@/stores/accountStore'
  import { useTransactionStore } from '@/stores/transactionStore'
  import AutoComplete from 'primevue/autocomplete';
  import Button from 'primevue/button';
  import Error404 from '@/components/Error404.vue'
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Textarea from 'primevue/textarea';
  import Select from 'primevue/select';
  import DatePicker from 'primevue/datepicker';
  import { Form } from '@primevue/forms';
  import { toLookupEntity } from '@/code/utils/utils'
  var saving = ref(false);
  const currencies = ref([]);
  const beneficiaries = ref([]);
  const categories = ref([]);
  const findCurrencies = async (event) => {
    console.log("Carico le valute", event.query);
    currencies.value = await DomuWaveStore.loadCurrencies(event.query);
    console.log("Ho caricato le valute:", currencies);
  };

  const findBeneficiaries = async (event) => {
    var itemFound = await transactionStore.findBeneficiaries(event.query);
    console.log("find", event.query, itemFound);

    if (itemFound == null || itemFound.length == 0) {
      itemFound = [{
        "name": (" " + event.query),
        "description": (" " + event.query),
        "id": 22
      }];

    };
    console.log("find 2", itemFound);
    //beneficiaries.value = itemFound.map(item => ({
    //  id: item.id,
    //  description: item.name
    //}));;

    beneficiaries.value = itemFound;

    console.log(beneficiaries.value);
  };

  const findCategories = async (event) => {
    var itemFound = await transactionStore.findCategories(event.query);
    console.log("find category", event.query, itemFound);





    categories.value = itemFound;
  };

  const transactionStore = useTransactionStore()
  const accountstore = useAccountStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()

  var editEntity = ref({ ...transactionStore.edititem });
  const formSubmitResolve = ref(null);

  const formRef = ref(null);
  const validatedOnce = ref(false);
  console.log("editEntity ", editEntity);

  const resolver = ({ values }) => {
    console.log("RESOLVER", values);
    const errors = {};
    console.log("editEntity.value.transactionType", editEntity.value.transactionType);
    if (editEntity.value.transactionType == null || editEntity.value.transactionType == "") {
      if (editEntity.value.transactionType != 0) {
        errors.transactionType = [{ message: 'Specificare il tipo' }];
      }
    }

    if (editEntity.value.account == null) {
      errors.currency = [{ message: 'Selezionare l\'account' }];
    }
    if (editEntity.value.transactionType == 2 && editEntity.value.destinationAccount == null) {
      errors.accountTo = [{ message: 'Specificare l\'account di destinazione' }];
    }
    if (editEntity.value.paymentMethod == null) {
      errors.paymentmethod = [{ message: 'Selezionare il metodo di pagamento.' }];
    }

    if (editEntity.value.currency == null) {
      errors.currency = [{ message: 'Selezionare la valuta di riferimento.' }];
    }
    console.log("editEntity.value.validFrom ", editEntity.value.transactionDate);

    if (editEntity.value.transactionDate == null || editEntity.value.transactionDate == "") {
      errors.transactionDate = [{ message: 'Selezionare la data di riferimento.' }];
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
    validatedOnce.value = true;
    if (valid) {
      saving.value = true;
      try {
        if (transactionStore.createmode) {
          success = await transactionStore.createEntity(editEntity.value);
        } else {
          success = await transactionStore.updateEntity(editEntity.value);
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
    if (editEntity != null && editEntity.value != null) {
      var newAccount = editEntity.value.account;
      if (newAccount != null) {
        var paymentMethodsByAccunt = await transactionStore.getPaymentMethods(newAccount.id);
        transactionStore.filterLookups.paymentMethods = toLookupEntity(paymentMethodsByAccunt.map(item => item.paymentMethod));
      }
    }
  }

  function undoNew() {

    transactionStore.undoNewEntity();


  }
  async function onBeneficiaryChange(event) {
    if (event == null)
      return;
    console.log("CCC", event);
    console.log("CCC editEntity.beneficiary", editEntity);
    var beneficiary = event.value;
    console.log("BBBB", beneficiary);
    if (beneficiary != null) {
      editEntity.value.beneficiary = beneficiary;
      editEntity.value.beneficiaryId = beneficiary.id;
      if (beneficiary.category != null) { }
      {
        editEntity.value.category = beneficiary.category;
      }


    } else {
      editEntity.value.beneficiary = null;
      editEntity.value.beneficiaryId = null;
    }

  }
  async function onCategoryChange(event) {
    if (event == null)
      return;
    var category = event.value;
    console.log("BBBB", category.value);
    if (category != null) {
      editEntity.value.category = category;

    } else {
      editEntity.value.category = null;
    }

  }
  async function onAccountDestinationChange(event) {
    const rawAccount = accountstore.accounts.find(item => item.id === event.value.id);
    if (rawAccount != null) {
      var fromAccount = editEntity != null && editEntity.value != null ? editEntity.value.account : null;
      console.log("fromAccount: ", fromAccount);
      const fromRawAccount = fromAccount != null ? accountstore.accounts.find(item => item.id === fromAccount.id) : null;
      console.log("fromRawAccount: ", fromRawAccount);
      editEntity.value.currency = rawAccount.currency;
      if (fromRawAccount != null) {
        var rate = await transactionStore.getRate(fromRawAccount.currency, rawAccount.currency, editEntity.value.transactionDate);

        if (rate != null) {

          editEntity.value.rate = rate.rate;
        } else {
          editEntity.value.rate = null;
        }
      }
    }
  }
  async function onTransactionTypeChange() {

    if (editEntity.value.transactionType != null && editEntity.value.transactionType != 2) {
      editEntity.value.destinationAccount = null;
    } 

    var fromAccount = editEntity != null && editEntity.value != null ? editEntity.account : null;
    var destinationAccount = editEntity != null && editEntity.value != null ? editEntity.value.destinationAccount : null;
    if (destinationAccount != null) {
      const rawAccount = accountstore.accounts.find(item => item.id === destinationAccount.id);

      const fromRawAccount = fromAccount != null ? accountstore.accounts.find(item => item.id === fromAccount.id) : null;
      editEntity.value.currency = rawAccount.currency;
      if (fromRawAccount != null) {
        var rate = await transactionStore.getRate(fromRawAccount.currency, rawAccount.currency, editEntity.value.transactionDate);

        if (rate != null) {

          editEntity.value.rate = rate.rate;
        } else {
          editEntity.value.rate = null;
        }
      }
    }
  }
  async function onAccountChange(event) {


    const newAccount = accountstore.accounts.find(item => item.id === event.value.id);

    if (newAccount != null) {
      var paymentMethodsByAccunt = await transactionStore.getPaymentMethods(newAccount.id);
      transactionStore.filterLookups.paymentMethods = toLookupEntity(paymentMethodsByAccunt.map(item => item.paymentMethod));

      console.log("isTrasfer", isTrasfer.value);
      if (!isTrasfer.value) {
        console.log("editEntity.value.currency", editEntity.value.currency);
        editEntity.value.currency = newAccount.currency;
        console.log("editEntity.value.currency new", editEntity.value.currency);
        editEntity.value.rate = 1;
      }
      else {
        var destinationAccount = editEntity != null && editEntity.value != null ? editEntity.destinationAccount : null;
        if (destinationAccount != null) {
          const rawAccount = accountstore.accounts.find(item => item.id === destinationAccount.id);
          console.log("onaccountchange call getrate", newAccount.currency, rawAccount, editEntity.value.transactionDate);
          var rate = await transactionStore.getRate(newAccount.currency, rawAccount.currency, editEntity.value.transactionDate);

        if (rate != null) {

          editEntity.value.rate = rate.rate;
        } else {
          editEntity.value.rate = null;
        }
        }
      }
    }
  }
  async function onTransactionDateChange(targetDate) {

    var currency = editEntity.value != null ? editEntity.value.currency : null;
    var accountCurrency = editEntity.value != null ? editEntity.value.account.currency : null;
    if (currency != null) {

      var rate = await transactionStore.getRate(accountCurrency, currency, targetDate);

      if (rate != null) {

        editEntity.value.rate = rate.rate;
      } else {
        editEntity.value.rate = null;
      }
    } else {
      editEntity.value.rate = null;
    }
  }
  async function onCurrencyChange(evt) {


    if (evt != null && evt.value != null) {
      var accountCurrency = editEntity.value != null && editEntity.value.account != null ? editEntity.value.account.currency : null;
      if (editEntity.value != null && editEntity.value.account == null)
        return;

      if (accountCurrency == null) {
        console.log("accountCurrency certo rawacc", editEntity.value.account.id);
        var rawAccount = accountstore.accounts.find(item => item.id === editEntity.value.account.id);
        console.log("accountCurrency certo rawacc", rawAccount);
        accountCurrency = rawAccount.currency;
      }

      console.log("accountCurrency ", accountCurrency);
      console.log("accountCurrency evt.value", evt.value);
      var rate = await transactionStore.getRate(accountCurrency, evt.value, editEntity.value.transactionDate);

      if (rate != null) {

        editEntity.value.rate = rate.rate;
      } else {
        editEntity.value.rate = null;
      }

    } else {
      editEntity.value.rate = null;
    }
  }

  onMounted(async () => {
    await refresh();
    // transactionStore.newEntity();
  })


  defineExpose({
    submitForm
  })
  const accountlookups = computed(() =>
    toLookupEntity(accountstore.accounts)

  )
  const isTrasfer = computed(() =>
    editEntity != null && editEntity.value.transactionType != null && editEntity.value.transactionType == 2
  )
 
  const enableCurrency = computed(() => {
    console.log("enableCurrency");
    var _enableCurrency = editEntity != null && editEntity.value != null && editEntity.value.currency != null && editEntity.value.currency != '' && !isTrasfer.value;
    console.log("enableCurrency", _enableCurrency);
    return _enableCurrency;
    }
  )

  const enableRate = computed(() => {
 
    var fromAccount = null;
    var toAccount = null;
    console.log("Enable RATE ", editEntity.rate);
    if (editEntity.rate == null)
      return true;
    if (editEntity != null && editEntity.value != null && editEntity.value.account != null) {
      fromAccount = accountstore.accounts.find(item => item.id === editEntity.value.account.id);
    }
    if (isTrasfer && editEntity != null && editEntity.value != null && editEntity.value.destinationAccount != null) {
      toAccount = accountstore.accounts.find(item => item.id === editEntity.value.destinationAccount.id);
    }

    var enableRate = false;
    console.log("enableRate fromAccount", fromAccount);
    console.log("enableRate toAccount", toAccount);

    fromAccount = fromAccount != null && fromAccount.value != null ? fromAccount.value : fromAccount;
    toAccount = toAccount != null && toAccount.value != null ? toAccount.value : toAccount;
    console.log("enableRate", fromAccount, toAccount);
    console.log("enableRate c", fromAccount != null ? fromAccount.currency : null, toAccount != null ? toAccount.currency : null);

    var currencyFrom = fromAccount != null ? fromAccount.currency : null;
    var currencyTo = toAccount != null ? toAccount.currency : null;

    if (currencyFrom == null || (isTrasfer.value && currencyTo == null)) {
      console.log("enableRate currencyFrom", currencyFrom);
      console.log("enableRate isTransfer", isTrasfer.value);
    return false;
    }
    if (!isTrasfer.value && editEntity.value.currency != null) {
      currencyTo = editEntity.value.currency;
    }
    enableRate = currencyFrom.id != currencyTo.id || editEntity.rate == null;

    //if (!isTrasfer)
    //  enableRate = fromAccount != null && fromAccount != null && fromAccount.currency != null && editEntity.value.currency != null && editEntity.currency.id != fromAccount.currency.id
    //else
    //  enableRate = toAccount != null && toAccount.currency != null && fromAccount != null && fromAccount.currency != null && fromAccount.currency.id != toAccount.currency.id
    
    console.log("enableRate result:", enableRate);
    console.log("enableRate editEntity.rate:", editEntity.rate);
    return enableRate;
  }
  )

  watch(editEntity, async () => {
    if (validatedOnce.value && formRef.value) {
      await formRef.value.validate();
    }
  }, { deep: true });

  watch(
    () => transactionStore.edititem,
    (newId) => {
      if (newId != null) {

        editEntity.value = { ...transactionStore.edititem };
      }
    },
    { immediate: true }
  )

</script>
