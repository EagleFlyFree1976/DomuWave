<template>
  <div v-if="show" class="condo-breadcrumb">
    <i class="pi pi-building breadcrumb-icon"></i>
    <span class="breadcrumb-condo">{{ store.selectedCondominio?.name ?? '…' }}</span>
    <span class="breadcrumb-sep">/</span>
    <span class="breadcrumb-page">{{ pageTitle }}</span>
    <button class="breadcrumb-refresh" :class="{ spinning: refreshing }" @click="refresh" title="Aggiorna dati">
      <i class="pi pi-refresh"></i>
    </button>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'

const route = useRoute()
const store = useAppStore()
const refreshing = ref(false)

const show = computed(() =>
  route.matched.some(r => r.meta?.requiresTenant) && !!store.selectedCondominio
)

const pageTitle = computed(() => route.meta?.title ?? '')

async function refresh() {
  if (refreshing.value) return
  refreshing.value = true
  try {
    await store.loadFiscalYears()
    window.dispatchEvent(new CustomEvent('app:refresh'))
  } finally {
    refreshing.value = false
  }
}
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

.breadcrumb-refresh {
  margin-left: auto;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-muted);
  padding: 2px 4px;
  border-radius: 4px;
  line-height: 1;
  transition: color 0.15s;
}
.breadcrumb-refresh:hover { color: var(--accent); }
.breadcrumb-refresh.spinning i { animation: spin 0.6s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
