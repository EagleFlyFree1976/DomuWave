<template>
  <div v-if="totalCount > 0" class="paginator">
    <div class="paginator-nav">
      <button class="paginator-btn" :disabled="modelValue === 1" @click="emit('update:modelValue', 1)">«</button>
      <button class="paginator-btn" :disabled="modelValue === 1" @click="emit('update:modelValue', modelValue - 1)">‹</button>

      <button
        v-for="p in pages"
        :key="p"
        class="paginator-btn"
        :class="{ active: p === modelValue, ellipsis: p === '…' }"
        :disabled="p === '…'"
        @click="p !== '…' && emit('update:modelValue', p)"
      >{{ p }}</button>

      <button class="paginator-btn" :disabled="modelValue === totalPages" @click="emit('update:modelValue', modelValue + 1)">›</button>
      <button class="paginator-btn" :disabled="modelValue === totalPages" @click="emit('update:modelValue', totalPages)">»</button>

      <span class="paginator-info">
        {{ (modelValue - 1) * pageSize + 1 }}–{{ Math.min(modelValue * pageSize, totalCount) }} di {{ totalCount }}
      </span>
    </div>

    <div class="paginator-size">
      <label class="paginator-size-label">Righe:</label>
      <select class="paginator-size-select" :value="pageSize" @change="onSizeChange">
        <option v-for="s in pageSizeOptions" :key="s" :value="s">{{ s }}</option>
      </select>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue:      { type: Number, required: true },
  totalCount:      { type: Number, required: true },
  pageSize:        { type: Number, required: true },
  pageSizeOptions: { type: Array,  default: () => [10, 20, 50, 100] },
})
const emit = defineEmits(['update:modelValue', 'update:pageSize'])

const totalPages = computed(() => Math.ceil(props.totalCount / props.pageSize))

const pages = computed(() => {
  const total = totalPages.value
  const cur   = props.modelValue
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)

  const result = []
  result.push(1)
  if (cur > 3)          result.push('…')
  for (let p = Math.max(2, cur - 1); p <= Math.min(total - 1, cur + 1); p++) result.push(p)
  if (cur < total - 2)  result.push('…')
  result.push(total)
  return result
})

function onSizeChange(e) {
  emit('update:pageSize', Number(e.target.value))
  emit('update:modelValue', 1)
}
</script>

<style scoped>
.paginator {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 0.6rem 0.5rem;
  flex-wrap: wrap;
}
.paginator-nav {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  flex-wrap: wrap;
}
.paginator-btn {
  min-width: 2rem;
  height: 2rem;
  padding: 0 0.4rem;
  border-radius: 6px;
  border: 1px solid var(--border);
  background: transparent;
  color: var(--text);
  font-size: 0.85rem;
  cursor: pointer;
  transition: background 0.1s, border-color 0.1s;
}
.paginator-btn:hover:not(:disabled):not(.ellipsis) {
  background: var(--accent-glow);
  border-color: var(--accent);
}
.paginator-btn.active {
  background: var(--accent);
  border-color: var(--accent);
  color: #fff;
  font-weight: 600;
}
.paginator-btn:disabled { opacity: 0.35; cursor: default; }
.paginator-btn.ellipsis { border-color: transparent; cursor: default; }
.paginator-info {
  margin-left: 0.5rem;
  font-size: 0.8rem;
  color: var(--text-muted);
  white-space: nowrap;
}
.paginator-size {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.paginator-size-label {
  font-size: 0.8rem;
  color: var(--text-muted);
  white-space: nowrap;
}
.paginator-size-select {
  height: 2rem;
  padding: 0 0.4rem;
  border-radius: 6px;
  border: 1px solid var(--border);
  background: var(--bg-surface);
  color: var(--text);
  font-size: 0.85rem;
  cursor: pointer;
}
</style>
