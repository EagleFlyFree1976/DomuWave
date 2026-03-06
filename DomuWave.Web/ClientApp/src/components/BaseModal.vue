<template>
  <Teleport to="body">
    <div v-if="show" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal" :class="sizeClass">
        <div class="modal-header">
          <div>
            <h2><slot name="title">{{ title }}</slot></h2>
            <p v-if="hasSubtitle" class="modal-subtitle">
              <slot name="subtitle">{{ subtitle }}</slot>
            </p>
          </div>
          <button class="btn-icon" @click="$emit('close')">✕</button>
        </div>
        <div class="modal-body">
          <slot />
        </div>
        <div v-if="$slots.footer" class="modal-footer">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { computed, useSlots } from 'vue'

const props = defineProps({
  show:     { type: Boolean, required: true },
  title:    { type: String,  default: '' },
  subtitle: { type: String,  default: '' },
  size:     { type: String,  default: '' }, // '' | 'sm' | 'md' | 'lg' | 'xl' | 'wide'
})

defineEmits(['close'])

const slots = useSlots()

const sizeClass = computed(() => props.size ? `modal-${props.size}` : '')
const hasSubtitle = computed(() => !!slots.subtitle || !!props.subtitle)
</script>
