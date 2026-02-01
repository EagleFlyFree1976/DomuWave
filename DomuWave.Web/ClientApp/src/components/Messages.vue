<template>
 
    <div class="toast-container position-fixed bottom-0 end-0 p-3 _toastContainer ">
      <div id="divToastDefaultMessage" class="toast align-items-center border-0 "
           data-bs-autohide="true" :class="alertClass"
           role="alert" aria-live="assertive" aria-atomic="true">
        <div class="toast-header">
          <i class="fa fa-atlassian"></i>
          <strong class="me-auto">DomuWave</strong>
          <small></small>

          <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">
          <span v-if="_messages != null">
            <i :class="messageIcon"></i> {{_messages.message}}
          </span>
        </div>
      </div>
    </div>
  
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue'
import { useMessageStore } from '@/stores/messageStore'
import { MESSAGES } from '@/code/messages'
  import { nextTick } from 'vue';
  let thisVue = this;
  let toastDefault = null;

  const _messages = ref(null);
  const messageStore = useMessageStore();

 const props = defineProps({
   type: {
    type: String,
    default: 'danger' // danger, warning, info, success
  },
  autoDismiss: {
    type: Boolean,
    default: true
  },
  dismissTime: {
    type: Number,
    default: 5000
  }
});

  
  const alertClass = computed(() => {
    if (_messages != null && _messages.value != null)
      return `text-bg-${_messages.value.type}`;
    else
      return "text-bg-primary";

  })
  const visibleMessages = computed(() => messageStore.visibleMessages);
  const messageIcon = computed(() => {
    if (_messages != null && _messages.value != null)
    {

      switch (_messages.value.type) {
        case "success":
          return "fas fa-check";
          break;
        case "danger":
          return "fas fa-exclamation-triangle";
          break;
        case "warning":
          return "fas fa-exclamation-circle";
          break;
      }
    }

    return "fas fa-exclamation";
  }
  );


 

  
 
onMounted(() => {
    const toastElDefaultMessage = document.getElementById("divToastDefaultMessage");
    var self = this;
    const option = {
      autohide: props.autoDismiss,
      delay: props.dismissTime
    };
    toastDefault = bootstrap.Toast.getOrCreateInstance(toastElDefaultMessage, option);
   
    var onHide = function(){
      // do something...
      console.log("Cancello i messaggi");
      messageStore.clear();
    };

  toastElDefaultMessage.addEventListener('hide.bs.toast', () => onHide);
  
  });


// Rendi visibile l'alert ogni volta che arrivano nuovi messaggi
watch(
  () => messageStore.visibleMessages,
  async (newMessages) => {
    console.log("[Message]", newMessages, newMessages.length);
    if (newMessages.length > 0) {
      _messages.value = newMessages[0];
      console.log("chiamo show");
      await nextTick(); // assicurati che il DOM sia pronto
      toastDefault.show();
    }
    else {
      console.log("chiamo hide");
      toastDefault.hide();
    }  


  },
  { deep: true }
)
</script>
