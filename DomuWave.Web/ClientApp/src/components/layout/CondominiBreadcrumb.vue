<template>
  <div v-if="show" class="condo-breadcrumb">
    <i class="pi pi-building breadcrumb-icon"></i>
    <span class="breadcrumb-condo">{{ store.selectedCondominio?.name ?? '…' }}</span>
    <span class="breadcrumb-sep">/</span>
    <span class="breadcrumb-page">{{ pageTitle }}</span>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'

const route = useRoute()
const store = useAppStore()

const show = computed(() =>
  route.matched.some(r => r.meta?.requiresTenant) && !!store.selectedCondominio
)

const pageTitle = computed(() => route.meta?.title ?? '')
</script>

<style scoped>
.condo-breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0.55rem 0.75rem;
  margin-bottom: 1.25rem;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 8px;
  font-size: 0.85rem;
  line-height: 1;
}

.breadcrumb-icon {
  font-size: 0.8rem;
  color: var(--accent);
  flex-shrink: 0;
}

.breadcrumb-condo {
  font-weight: 600;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 260px;
}

.breadcrumb-sep {
  color: var(--text-muted);
  flex-shrink: 0;
}

.breadcrumb-page {
  color: var(--text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
