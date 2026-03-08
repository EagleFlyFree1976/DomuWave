<template>
  <div class="section-card" :class="[statusKey, { 'section-card--blocked': blocked }]">
    <div class="section-header" @click="!blocked && (open = !open)">
      <div class="section-header-left">
        <span class="step-badge">{{ step }}</span>
        <span class="section-icon">{{ icon }}</span>
        <div>
          <span class="section-title">{{ title }}</span>
          <span class="section-desc">{{ desc }}</span>
        </div>
      </div>
      <div class="section-header-right">
        <span v-if="blocked" class="status-badge status-badge--blocked">In attesa</span>
        <span v-else class="status-badge" :class="`status-badge--${statusKey}`">{{ statusLabel }}</span>
        <i v-if="!blocked" class="pi" :class="open ? 'pi-chevron-up' : 'pi-chevron-down'" />
        <i v-else class="pi pi-lock" style="color:var(--text-muted);font-size:0.85rem" />
      </div>
    </div>

    <div v-if="open && !blocked" class="section-body">
      <div
        v-for="(c, i) in section?.checks ?? []"
        :key="i"
        class="check-row"
        :class="checkRowClass(c)"
      >
        <span class="check-icon">
          <i class="pi" :class="checkIconClass(c)" />
        </span>
        <div class="check-content">
          <span class="check-label">{{ c.label }}</span>
          <span v-if="c.detail" class="check-detail">{{ c.detail }}</span>
        </div>
        <router-link v-if="!c.isOk && link" :to="link" class="check-link">
          {{ linkLabel }} →
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  step:      Number,
  title:     String,
  icon:      String,
  desc:      String,
  section:   Object,   // { status: 0|1|2|3, checks: [{isOk, isWarn, label, detail}] }
  link:      String,
  linkLabel: String,
  blocked:   { type: Boolean, default: false },
})

const open = ref(true)

// Backend serializes enum as integer: Ok=0, Warn=1, Error=2, Na=3
// or as string if [JsonConverter] is applied
const STATUS_MAP = { 0: 'ok', 1: 'warn', 2: 'error', 3: 'na', Ok: 'ok', Warn: 'warn', Error: 'error', Na: 'na' }
const LABEL_MAP  = { ok: 'Completato', warn: 'Attenzione', error: 'Da completare', na: 'N/A' }

const statusKey   = computed(() => STATUS_MAP[props.section?.status ?? 2] ?? 'error')
const statusLabel = computed(() => LABEL_MAP[statusKey.value] ?? '')

function checkRowClass(c) {
  if (c.isOk)   return 'check-row--ok'
  if (c.isWarn) return 'check-row--warn'
  return 'check-row--error'
}

function checkIconClass(c) {
  if (c.isOk)   return 'pi-check-circle'
  if (c.isWarn) return 'pi-exclamation-circle'
  return 'pi-times-circle'
}
</script>

<style scoped>
/* ── SECTION CARD ─────────────────────────────────────────────────────────── */
.section-card {
  background: var(--surface2, var(--surface));
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
  border-left: 4px solid var(--border);
}
.section-card.ok    { border-left-color: #22c55e; }
.section-card.warn  { border-left-color: #f59e0b; }
.section-card.error { border-left-color: #ef4444; }
.section-card.na    { border-left-color: var(--border); }
.section-card--blocked {
  opacity: 0.55;
  filter: grayscale(0.4);
}
.section-card--blocked .section-header { cursor: default; }

/* ── STEP BADGE ──────────────────────────────────────────────────────────── */
.step-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  min-width: 22px;
  border-radius: 50%;
  background: var(--border);
  color: var(--text-muted);
  font-size: 0.7rem;
  font-weight: 700;
  flex-shrink: 0;
}

/* ── HEADER ──────────────────────────────────────────────────────────────── */
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  cursor: pointer;
  user-select: none;
  transition: background 0.12s;
}
.section-header:hover { background: rgba(255,255,255,0.03); }

.section-header-left {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}
.section-icon  { font-size: 1.2rem; line-height: 1; margin-top: 2px; }
.section-title { display: block; font-size: 0.9rem; font-weight: 600; color: var(--text-primary); }
.section-desc  { display: block; font-size: 0.75rem; color: var(--text-muted); margin-top: 2px; max-width: 520px; }

.section-header-right { display: flex; align-items: center; gap: 10px; flex-shrink: 0; }

/* ── STATUS BADGE ────────────────────────────────────────────────────────── */
.status-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 3px 9px;
  border-radius: 20px;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.status-badge--ok      { background: rgba(34,197,94,.15);   color: #22c55e; }
.status-badge--warn    { background: rgba(245,158,11,.15);  color: #f59e0b; }
.status-badge--error   { background: rgba(239,68,68,.15);   color: #ef4444; }
.status-badge--na      { background: rgba(120,120,120,.12); color: var(--text-muted); }
.status-badge--blocked { background: rgba(120,120,120,.12); color: var(--text-muted); }

/* ── SECTION BODY ────────────────────────────────────────────────────────── */
.section-body {
  border-top: 1px solid var(--border);
  padding: 10px 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

/* ── CHECK ROW ───────────────────────────────────────────────────────────── */
.check-row {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 7px 10px;
  border-radius: 7px;
  background: var(--surface);
}
.check-row--ok    .check-icon { color: #22c55e; }
.check-row--warn  .check-icon { color: #f59e0b; }
.check-row--error .check-icon { color: #ef4444; }

.check-icon   { font-size: 0.95rem; margin-top: 1px; flex-shrink: 0; }
.check-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}
.check-label  { font-size: 0.82rem; color: var(--text-primary); font-weight: 500; }
.check-detail { font-size: 0.75rem; color: var(--text-muted); }

.check-link {
  font-size: 0.75rem;
  color: var(--accent);
  white-space: nowrap;
  text-decoration: none;
  margin-left: auto;
  flex-shrink: 0;
}
.check-link:hover { text-decoration: underline; }
</style>
