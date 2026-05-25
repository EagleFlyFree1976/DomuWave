<template>
  <div class="tenant-list-page">

    <!-- ── HEADER ─────────────────────────────────────────────────────── -->
    <div class="page-header">
      <div class="page-header__left">
        <h1 class="page-title">
          <i class="pi pi-building page-title__icon" />
          Gestione Tenant
        </h1>
        <span class="page-subtitle">Amministrazione delle organizzazioni della piattaforma</span>
      </div>
      <div class="page-header__actions">
        <Button label="Nuovo Tenant"
                icon="pi pi-plus"
                class="btn-primary"
                @click="goToNew" />
      </div>
    </div>

    <!-- ── FILTRI ──────────────────────────────────────────────────────── -->
    <div class="filter-bar">
      <div class="filter-bar__search">
        <span class="p-input-icon-left w-full">
          <i class="pi pi-search" />
          <InputText v-model="searchText"
                     placeholder="Cerca per nome o codice..."
                     class="filter-input"
                     @input="onSearchInput" />
        </span>
      </div>

      <div class="filter-bar__status">
        <label class="filter-label">Stato</label>
        <Select v-model="selectedStatus"
                :options="statusOptions"
                option-label="label"
                option-value="value"
                placeholder="Tutti"
                class="filter-select"
                @change="onStatusChange" />
      </div>

      <div class="filter-bar__pagesize">
        <label class="filter-label">Per pagina</label>
        <Select v-model="selectedPageSize"
                :options="pageSizeOptions"
                class="filter-select filter-select--sm"
                @change="onPageSizeChange" />
      </div>

      <Button icon="pi pi-filter-slash"
              class="btn-ghost"
              v-tooltip="'Reimposta filtri'"
              :disabled="!hasActiveFilters"
              @click="resetFilters" />
    </div>

    <!-- ── TABELLA ─────────────────────────────────────────────────────── -->
    <div class="table-wrapper">
      <DataTable :value="store.tenants"
                 :loading="store.loading"
                 lazy
                 :rows="store.query.pageSize"
                 :total-records="store.totalCount"
                 :sort-field="store.query.sortField"
                 :sort-order="sortOrderNum"
                 removable-sort
                 data-key="id"
                 class="domu-table"
                 @sort="onSort">
        <!-- Colonna: Codice -->
        <Column field="code" header="Codice" sortable style="width: 120px">
          <template #body="{ data }">
            <span class="code-badge">{{ data.code }}</span>
          </template>
        </Column>

        <!-- Colonna: Nome -->
        <Column field="name" header="Nome" sortable />

        <!-- Colonna: Stato -->
        <Column field="isActive" header="Stato" sortable style="width: 110px">
          <template #body="{ data }">
            <Tag :value="data.isActive ? 'Attivo' : 'Inattivo'"
                 :severity="data.isActive ? 'success' : 'secondary'"
                 class="status-tag" />
          </template>
        </Column>

        <!-- Colonna: Creato da -->
        <Column field="createdByFullName" header="Creato da" style="width: 170px">
          <template #body="{ data }">
            <span class="meta-text">{{ data.createdByFullName || '—' }}</span>
          </template>
        </Column>

        <!-- Colonna: Azioni -->
        <Column header="" style="width: 130px; text-align: right">
          <template #body="{ data }">
            <div class="row-actions">
              <Button icon="pi pi-sync"
                      class="btn-row-action"
                      v-tooltip="'Aggiorna cache licenza'"
                      :loading="refreshingId === data.id"
                      @click="refreshLicense(data)" />
              <Button icon="pi pi-pencil"
                      class="btn-row-action"
                      v-tooltip="'Modifica'"
                      @click="goToDetail(data.id)" />
              <Button icon="pi pi-trash"
                      class="btn-row-action btn-row-action--danger"
                      v-tooltip="'Elimina'"
                      @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>

        <!-- Stato vuoto -->
        <template #empty>
          <div class="empty-state">
            <i class="pi pi-inbox empty-state__icon" />
            <span>Nessun tenant trovato</span>
          </div>
        </template>

        <!-- Loading overlay -->
        <template #loadingicon>
          <i class="pi pi-spinner pi-spin loading-spinner" />
        </template>
      </DataTable>

      <!-- ── PAGINAZIONE ─────────────────────────────────────────────── -->
      <div class="pagination-bar">
        <span class="pagination-info">
          {{ paginationInfo }}
        </span>
        <Paginator :rows="store.query.pageSize"
                   :total-records="store.totalCount"
                   :first="paginatorFirst"
                   :rows-per-page-options="pageSizeOptions"
                   template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink"
                   class="domu-paginator"
                   @page="onPage" />
      </div>
    </div>

    <!-- ── DIALOG CONFERMA ELIMINAZIONE ───────────────────────────────── -->
    <ConfirmDialog class="domu-confirm" />

  </div>
</template>

<script setup>
  import { ref, computed, onMounted, watch } from 'vue'
  import { useRouter } from 'vue-router'
  import { useConfirm } from 'primevue/useconfirm'
  import { useToast } from 'primevue/usetoast'
  import { useTenantStore } from '@/stores/tenantStore'
  import { licenseApi } from '@/services/api'

  import DataTable from 'primevue/datatable'
  import Column from 'primevue/column'
  import Button from 'primevue/button'
  import InputText from 'primevue/inputtext'
  import Select from 'primevue/select'
  import Tag from 'primevue/tag'
  import Paginator from 'primevue/paginator'
  import ConfirmDialog from 'primevue/confirmdialog'

  // ─── COMPOSABLES ────────────────────────────────────────────────────────────
  const router = useRouter()
  const confirm = useConfirm()
  const toast = useToast()
  const store = useTenantStore()

  // ─── LOCAL STATE ────────────────────────────────────────────────────────────
  /** Timeout handle per il debounce della ricerca */
  let searchDebounce = null

  const refreshingId = ref(null)

  const searchText = ref(store.query.search)
  const selectedStatus = ref(store.query.isActive)
  const selectedPageSize = ref(store.query.pageSize)

  const statusOptions = [
    { label: 'Tutti', value: null },
    { label: 'Attivi', value: true },
    { label: 'Inattivi', value: false },
  ]
  const pageSizeOptions = [10, 20, 50, 100]

  // ─── COMPUTED ────────────────────────────────────────────────────────────────

  /** Converte 'asc'/'desc' → 1/-1 richiesto da PrimeVue DataTable */
  const sortOrderNum = computed(() => (store.query.sortOrder === 'asc' ? 1 : -1))

  /** Primo elemento nella pagina corrente (0-based, per Paginator) */
  const paginatorFirst = computed(() => (store.query.page - 1) * store.query.pageSize)

  /** Testo descrittivo paginazione */
  const paginationInfo = computed(() => {
    const { page, pageSize } = store.query
    const total = store.totalCount
    if (total === 0) return 'Nessun risultato'
    const from = (page - 1) * pageSize + 1
    const to = Math.min(page * pageSize, total)
    return `${from}–${to} di ${total} tenant`
  })

  /** True se almeno un filtro è attivo */
  const hasActiveFilters = computed(
    () => !!searchText.value || selectedStatus.value !== null
  )

  // ─── LIFECYCLE ───────────────────────────────────────────────────────────────
  onMounted(() => loadList())

  // ─── METHODS ────────────────────────────────────────────────────────────────

  async function loadList() {
    await store.fetchList()
  }

  /** Debounce 400ms sulla ricerca testuale */
  function onSearchInput() {
    clearTimeout(searchDebounce)
    searchDebounce = setTimeout(() => {
      store.setQueryParams({ search: searchText.value, page: 1 })
      loadList()
    }, 400)
  }

  function onStatusChange() {
    store.setQueryParams({ isActive: selectedStatus.value, page: 1 })
    loadList()
  }

  function onPageSizeChange() {
    store.setQueryParams({ pageSize: selectedPageSize.value, page: 1 })
    loadList()
  }

  function onSort(event) {
    store.setQueryParams({
      sortField: event.sortField,
      sortOrder: event.sortOrder === 1 ? 'asc' : 'desc',
      page: 1,
    })
    loadList()
  }

  function onPage(event) {
    // PrimeVue Paginator: event.page è 0-based
    store.setQueryParams({ page: event.page + 1 })
    loadList()
  }

  function resetFilters() {
    searchText.value = ''
    selectedStatus.value = null
    store.setQueryParams({ search: '', isActive: null, page: 1 })
    loadList()
  }

  function goToNew() {
    router.push({ name: 'tenant-new' })
  }

  function goToDetail(id) {
    router.push({ name: 'tenant-detail', params: { id } })
  }

  function confirmDelete(tenant) {
    confirm.require({
      message: `Vuoi eliminare il tenant "${tenant.name}"? L'operazione non è reversibile.`,
      header: 'Conferma eliminazione',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Elimina',
      rejectLabel: 'Annulla',
      acceptClass: 'p-button-danger',
      accept: () => doDelete(tenant),
    })
  }

  async function refreshLicense(tenant) {
    refreshingId.value = tenant.id
    try {
      const { data } = await licenseApi.refreshCache(tenant.id)
      const count = data.activeFeatures?.length ?? 0
      toast.add({
        severity: 'success',
        summary: 'Cache aggiornata',
        detail: `Tenant "${tenant.name}": ${count} feature attive.`,
        life: 4000,
      })
    } catch (err) {
      if (!err?.response)
        toast.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile raggiungere il server.', life: 3000 })
    } finally {
      refreshingId.value = null
    }
  }

  async function doDelete(tenant) {
    try {
      await store.remove(tenant.id)
      toast.add({ severity: 'success', summary: 'Eliminato', detail: `Tenant "${tenant.name}" eliminato.`, life: 3000 })
      await session.loadTenants()  // ← aggiornamento selettore
    } catch (_) {
      // Il toast di errore è già mostrato dall'interceptor API
    }
  }
</script>

<style scoped>
  /* ── PAGE LAYOUT ─────────────────────────────────────────────────────────── */
  .tenant-list-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
    padding: 28px 32px;
    min-height: 100%;
  }

  /* ── HEADER ──────────────────────────────────────────────────────────────── */
  .page-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
  }

  .page-header__left {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .page-title {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 22px;
    font-weight: 700;
    color: var(--text);
    margin: 0;
  }

  .page-title__icon {
    color: var(--accent);
    font-size: 20px;
  }

  .page-subtitle {
    font-size: 13px;
    color: var(--text-dim);
  }

  /* ── FILTER BAR ──────────────────────────────────────────────────────────── */
  .filter-bar {
    display: flex;
    align-items: flex-end;
    gap: 12px;
    flex-wrap: wrap;
    background: var(--surface2);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 14px 16px;
  }

  .filter-bar__search {
    flex: 1;
    min-width: 200px;
  }

  .filter-label {
    display: block;
    font-size: 11px;
    color: var(--text-faint);
    text-transform: uppercase;
    letter-spacing: 0.8px;
    margin-bottom: 5px;
  }

  .filter-input {
    width: 100%;
    background: var(--surface) !important;
    border-color: var(--border) !important;
    color: var(--text) !important;
    font-size: 13px !important;
  }

  .filter-select {
    width: 160px;
    font-size: 13px;
  }

  .filter-select--sm {
    width: 90px;
  }

  /* ── BUTTONS ─────────────────────────────────────────────────────────────── */
  .btn-primary {
    background: var(--accent) !important;
    border-color: var(--accent) !important;
    color: #000 !important;
    font-weight: 600 !important;
    font-size: 13px !important;
  }

  .btn-ghost {
    background: transparent !important;
    border-color: var(--border) !important;
    color: var(--text-dim) !important;
  }

    .btn-ghost:disabled {
      opacity: 0.4 !important;
    }

  .row-actions {
    display: flex;
    justify-content: flex-end;
    gap: 4px;
  }

  .btn-row-action {
    background: transparent !important;
    border: none !important;
    color: var(--text-dim) !important;
    width: 32px !important;
    height: 32px !important;
    padding: 0 !important;
    border-radius: 6px !important;
  }

    .btn-row-action:hover {
      background: var(--surface2) !important;
      color: var(--accent) !important;
    }

  .btn-row-action--danger:hover {
    background: rgba(255, 80, 80, 0.1) !important;
    color: #ff5555 !important;
  }

  /* ── TABLE ───────────────────────────────────────────────────────────────── */
  .table-wrapper {
    background: var(--surface2);
    border: 1px solid var(--border);
    border-radius: 10px;
    overflow: hidden;
  }

  .domu-table {
    font-size: 13px;
  }

  .code-badge {
    background: var(--accent)18;
    color: var(--accent);
    border: 1px solid var(--accent)30;
    border-radius: 5px;
    padding: 2px 8px;
    font-size: 12px;
    font-family: 'JetBrains Mono', monospace;
    font-weight: 500;
  }

  .status-tag {
    font-size: 11px !important;
    padding: 2px 8px !important;
  }

  .meta-text {
    color: var(--text-dim);
    font-size: 12px;
  }

  /* ── EMPTY STATE ─────────────────────────────────────────────────────────── */
  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
    padding: 48px 0;
    color: var(--text-faint);
  }

  .empty-state__icon {
    font-size: 36px;
    opacity: 0.4;
  }

  /* ── PAGINATION ──────────────────────────────────────────────────────────── */
  .pagination-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 16px;
    border-top: 1px solid var(--border);
  }

  .pagination-info {
    font-size: 12px;
    color: var(--text-faint);
  }

  /* ── LOADING ─────────────────────────────────────────────────────────────── */
  .loading-spinner {
    font-size: 28px;
    color: var(--accent);
  }

  /* ── RESPONSIVE ──────────────────────────────────────────────────────────── */
  @media (max-width: 768px) {
    .tenant-list-page {
      padding: 16px;
    }

    .page-header {
      flex-direction: column;
    }

    .filter-bar {
      flex-direction: column;
      align-items: stretch;
    }

    .filter-select {
      width: 100%;
    }
  }
</style>
