<template>
  <span class="sortable d-inline-flex align-items-center" @click="toggleSort">
    <slot /> <!-- il nome della colonna -->
    <i v-if="isActive && direction === 'asc'" class="ms-1 fas fa-sort-up"></i>
    <i v-else-if="isActive && direction === 'desc'" class="ms-1 fas fa-sort-down"></i>
    <i v-else class="ms-1 fas fa-sort text-muted"></i>
  </span>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  field: { type: String, required: true },
  modelValue: { type: Object, default: () => ({ field: null, direction: null }) }
});

const emit = defineEmits(["update:modelValue"]);

const isActive = computed(() => props.modelValue.field === props.field);
const direction = computed(() => props.modelValue.direction);

function toggleSort() {
  let newDirection = "asc";
  if (isActive.value) {
    newDirection = direction.value === "asc" ? "desc" : "asc";
  }
  emit("update:modelValue", { field: props.field, direction: newDirection });
}
</script>

<style scoped>
  .sortable {
    cursor: pointer;
    user-select: none;
  }
</style>
