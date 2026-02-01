


<template>

  <div>


    <div class="card">
     
     

      <ConfirmPopup></ConfirmPopup>
      <Toolbar style="padding: 1rem 1rem 1rem 1.5rem">
        <template #start>
          <h1 class="h3 mb-0 text-gray-800">Categorie</h1>
        </template>
        <template #center>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <IconField>
              <InputIcon class="pi pi-search" />
              <InputText v-model="categoryStore.q" @keyup.enter="refresh" class="p-inputtext-sm" placeholder="search" />
              <Button text plain @click="resetSearch" title="Cancella ricerca">
                <i class="fas fa-trash"></i>
              </Button>
            </IconField>
         
            <Button label="Refresh" text plain @click="refresh">
              <i class="fas fa-sync-alt"></i>
            </Button>
            <Button as="a" label="Crea" text plain @click="newCategory(null)">
              <i class="fa fa-plus-square"></i>
            </Button>

          </div>
        </template>
      </Toolbar>
    </div>
    <div class="card shadow mb-4">

      <div class="card-body">


        <TreeTable :value="treeSource" tableStyle="min-width: 50rem">
          <Column field="name" header="label" expander style="width: 34%"></Column>
          <Column field="description" header="Descrizione" style="width: 33%"></Column>
          <Column style="width: 22%m">
            <template #body="slotProp">
              <div class="flex flex-wrap gap-2">
                <Button @click="editCategory(slotProp.node.key)" label="Modifica" severity="default" outlined>
                  <i class="far fa-edit"></i>
                </Button>
                <Button as="a" label="Crea sotto categoria" text plain @click="newCategory(slotProp.node.key)" v-if="slotProp.node.data.parent == null">
                  <i class="fa fa-plus-square"></i>
                </Button>

                <Button @click="confirmRemove($event, slotProp.node)" label="Delete" severity="danger" outlined v-if="slotProp.node.children == null || slotProp.node.children.length == 0">
                  <i class="far fa-minus-square"></i>
                </Button>

                <!--<Button @click="confirmRemove($event, row.id)" label="Delete" severity="danger" outlined>
                  <i class="far fa-minus-square"></i>
                </Button>

                <Button type="button" icon="pi pi-search" rounded />
                <Button type="button" icon="pi pi-pencil" rounded severity="success" />-->
              </div>
            </template>
          </Column>
        </TreeTable>



      </div>

      <Dialog header="Crea categoria" v-model:visible="modalEditVisible" :modal="true" :closable="true" :style="{ width: '50rem' }">
        <EditForm ref="createForm"></EditForm>
        <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
          <Button type="button" label="Cancel" severity="secondary" @click="modalEditVisible = false"></Button>
          <Button type="button" label="Save" class="bg-gray-800 text-white border-none hover:bg-gray-800" @click="savenewCategory()"></Button>
        </div>
      </Dialog>
    </div>
  </div>

</template>
<script setup>
  import { onMounted, ref, watch, computed  } from 'vue'

  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import { useCategoryStore } from '@/stores/categoryStore'
  import { useConfirm } from "primevue/useconfirm";
  import InputText from 'primevue/inputtext';
  import FloatLabel from 'primevue/floatlabel';
  import EditForm from './EditForm.vue'
  import Dialog from 'primevue/dialog';
  import Toolbar from 'primevue/toolbar';
  import ConfirmPopup from 'primevue/confirmPopup';
  import Button from 'primevue/button';

import TreeTable from 'primevue/treetable';
import Column from 'primevue/column';



  const createForm = ref(null)
  var modalEditVisible = ref(false);
  const categoryStore = useCategoryStore();
  const DomuWaveStore = useDomuWaveStore();
  const confirm = useConfirm();
  const props = defineProps({

  })

  async function deleteCategory(id) {
    DomuWaveStore.startLoading();
    await categoryStore.deleteCategory(id);
    await refresh();
    DomuWaveStore.stopLoading();
  }

  const confirmRemove = (event,item) => {
    console.log("confirmremove", event, item);
    confirm.require({
        target: event.currentTarget,
        message: 'Sei sicuro di voler procedere con la cancellazione della categoria selezionata?',
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

          deleteCategory(item.key);
            //toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 });
        },
        reject: () => {

        }
    });
};
  async function resetSearch() {
    categoryStore.q = null;
    await refresh();
  }
  async function refresh(){
    DomuWaveStore.startLoading();
    await categoryStore.loadAllCategories();
    DomuWaveStore.stopLoading();
  }
  onMounted(async () => {

    await refresh();


  })
  async function editCategory(categoryId){

     await categoryStore.edit(categoryId);;
     modalEditVisible.value = true;
  }
  function newCategory(parentCategoryId) {
    categoryStore.newEntity(parentCategoryId);
    modalEditVisible.value = true;
  }
  async function savenewCategory() {
    const result = await createForm.value.submitForm();
    if (result) {

          setTimeout(async function () {
            await refresh();
            modalEditVisible.value = false;


    }, 500);

    }
  }

  function buildTree(items, parentId = null) {
    if (items == null)
      return null;

  return items
    .filter(item => (item.parent ? item.parent.id : null) === parentId)
    .map(item => ({
      key: item.id,
      data: {
        name: item.name,
        description: item.description,
        parent:item.parent
      },
      children: buildTree(items, item.id)
    }))
  }
  const q = computed(() => {
    var _q = categoryStore.q;
    return _q;
  })

 const treeSource = computed(() => {

   let sourceData = categoryStore.categories;
   var treeSource = buildTree(sourceData, null);



   return treeSource ;
})

</script>

<style scoped>
  .p-inputtext-sm {
    height: 2rem; /* regola altezza */
    font-size: 0.875rem; /* testo più piccolo */
  }


</style>
