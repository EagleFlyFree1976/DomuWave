<template>
  <!-- Feature non consumabile: mostra sempre il contenuto se abilitata -->
  <template v-if="!isConsumable">
    <slot v-if="isEnabled(feature)" />
    <slot v-else name="disabled" />
  </template>

  <!-- Feature consumabile -->
  <template v-else>
    <!-- Crediti disponibili: mostra il contenuto -->
    <slot v-if="!isExhausted(feature)" />

    <!-- Crediti esauriti: mostra lo slot "exhausted" oppure il fallback predefinito -->
    <template v-else>
      <slot name="exhausted">
        <a :href="buyUrl" class="feature-gate-buy">
          <span class="feature-gate-icon">⚠️</span>
          {{ exhaustedLabel }}
        </a>
      </slot>
    </template>

    <!-- Warning crediti in esaurimento: sempre visibile quando attivo -->
    <div v-if="isWarning(feature)" class="feature-gate-warning">
      <slot name="warning">
        ⚠️ Rimangono solo <strong>{{ remaining(feature) }}</strong> {{ unitLabel }} disponibili
      </slot>
    </div>
  </template>
</template>

<script setup>
import { computed } from 'vue'
import { useFeatureStatus } from '@/composables/useFeatureStatus'

const props = defineProps({
  /** Codice della feature (es. 'ATTACHMENTS') */
  feature: {
    type: String,
    required: true,
  },
  /** URL della pagina acquisto. Default: /licenze */
  buyUrl: {
    type: String,
    default: '/licenze',
  },
  /** Testo del badge quando i crediti sono esauriti */
  exhaustedLabel: {
    type: String,
    default: null,
  },
  /** Nome delle unità per il messaggio di warning (es. "allegati", "SMS") */
  unitLabel: {
    type: String,
    default: 'crediti',
  },
})

const { isEnabled, isExhausted, isWarning, remaining } = useFeatureStatus()

// Una feature è consumabile se ha un limite configurato
const isConsumable = computed(() => remaining(props.feature) !== null || isExhausted(props.feature))

const exhaustedLabel = computed(() =>
  props.exhaustedLabel ?? `${props.feature} esauriti — Acquista`
)
</script>

<style scoped>
.feature-gate-buy {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 500;
  color: var(--accent-red, #ef4444);
  border: 1px solid var(--accent-red, #ef4444);
  border-radius: 6px;
  padding: 6px 12px;
  text-decoration: none;
  transition: background .15s;
}
.feature-gate-buy:hover {
  background: color-mix(in srgb, var(--accent-red, #ef4444) 10%, transparent);
}
.feature-gate-icon { font-size: 14px; }

.feature-gate-warning {
  font-size: 13px;
  color: var(--accent-red, #ef4444);
  background: color-mix(in srgb, var(--accent-red, #ef4444) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--accent-red, #ef4444) 30%, transparent);
  border-radius: 6px;
  padding: 6px 12px;
  margin-top: 6px;
}
</style>
