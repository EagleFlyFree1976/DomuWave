





<template>
  <div class="card">
    <div class="flex mb-2 gap-2 justify-end">
      <Button @click="value = '0'" rounded label="1" class="w-8 h-8 p-0" :outlined="value !== '0'" />
      <Button @click="value = '1'" rounded label="2" class="w-8 h-8 p-0" :outlined="value !== '1'" />
      <Button @click="value = '2'" rounded label="3" class="w-8 h-8 p-0" :outlined="value !== '2'" />
    </div>

    <Tabs v-model:value="value">
      <TabList>
        <Tab value="0">Header I</Tab>
        <Tab value="1">Header II</Tab>
        <Tab value="2">Header III</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <p class="m-0">
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo
            consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
          </p>
        </TabPanel>
        <TabPanel value="1">
          <p class="m-0">
            Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim
            ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Consectetur, adipisci velit, sed quia non numquam eius modi.
          </p>
        </TabPanel>
        <TabPanel value="2">
          <p class="m-0">
            At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa
            qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus.
          </p>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>




<script setup>


  import { onMounted, watch, reactive, ref } from 'vue'

  import { useAccountStore } from '@/stores/accountStore'
  import { useMessageStore } from '@/stores/messageStore'
  import { useDomuWaveStore } from '@/stores/DomuWaveStore'
  import Toolbar from 'primevue/toolbar';
  import Button from 'primevue/button';

  import Error404 from '../../components/Error404.vue'
  import InputText from 'primevue/inputtext';
  import DatePicker from 'primevue/datepicker';

  import Tabs from 'primevue/tabs';
  import TabList from 'primevue/tablist';
  import Tab from 'primevue/tab';
  import TabPanels from 'primevue/tabpanels';
  import TabPanel from 'primevue/tabpanel';

  const value = ref('0');
  const accountStore = useAccountStore()
  const messageStore = useMessageStore()
  const DomuWaveStore = useDomuWaveStore()
  const props = defineProps({
    accountid: Number
  });




  async function refresh() {
    DomuWaveStore.startLoading();

    DomuWaveStore.stopLoading();
  }


  onMounted(async () => {

    await refresh();
  })


  watch(
    () => props.accountid,
    (newId) => {
      if (newId != null) {
        if (newId != 0) {
          DomuWaveStore.startLoading();

          DomuWaveStore.stopLoading();
        }
        else {


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
