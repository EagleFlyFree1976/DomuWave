<template>
  <div class="col-xl-3 col-md-6 mb-4">
    <div class="card border-left-primary shadow h-100 py-2">
      <div class="card-body">
        <div class="row no-gutters align-items-center">
          <div class="col mr-2">
            <div class="text-xs font-weight-bold text-primary text-uppercase mb-1">
              <slot name="header"></slot>
              <div v-if="inItem != null">
                <span class="small text-muted" v-date:date="inItem.period.firstDay"></span> - <span class="small text-muted" v-date:date="inItem.period.lastDay"></span>

              </div>
            </div>
            <div class="h5 mb-0 font-weight-bold text-gray-800">
              <i class="fas fa-long-arrow-alt-down text-success"></i><span v-currency="{ amount: inAmount}"></span> / <i class="fas fa-long-arrow-alt-up text-danger"></i><span v-currency="{ amount: outAmount}"></span>
            </div>
          </div>
          <div class="col-auto">
            <i class="fas fa-euro-sign fa-2x text-gray-300"></i>
            
            
          </div>
        </div>
      </div>
      <div class="card-footer">
        <div class="row no-gutters align-items-center">
          <div class="col mr-2">
            <div class="text-xs font-weight-bold text-primary text-uppercase mb-1 float-md-right">
              <div v-if="inItem != null">
                <i class="fas fa-long-arrow-alt-down text-success" v-if="difference > 0"></i>
                <i class="fas fa-long-arrow-alt-up text-danger" v-else></i>
                <span v-currency="{ amount: difference}"></span>

              </div>
            </div>
            
          </div>
          
        </div>
        
      </div>
    </div>
  </div>


  

</template>

<script setup>
  
  import { computed } from 'vue'
  
  const props = defineProps({
    inItem: Object,
    outItem: Object,
    inAmount: Number,
    outAmount: Number
  })

  const difference = computed(() => Math.abs(props.inAmount) - Math.abs(props.outAmount));

   


</script>
