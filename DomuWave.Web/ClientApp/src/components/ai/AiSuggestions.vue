<template>
  <div class="ai-suggestions">
    <p class="ai-suggestions-title">Prova a chiedere:</p>
    <button
      v-for="s in suggestions"
      :key="s"
      type="button"
      class="ai-suggestion-chip"
      @click="$emit('select', s)"
    >
      {{ s }}
    </button>
  </div>
</template>

<script setup>
defineEmits(['select'])

const suggestions = [
  'Quali condomini sono in ritardo con le rate?',
  'Qual è lo stato pagamenti del sig. Rossi?',
  'Riepilogo spese del condominio di quest\'anno',
  'Elenca i condomini che gestisco',
]
</script>

<style scoped>
.ai-suggestions {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  padding: 0.5rem 0;
}
.ai-suggestions-title {
  margin: 0 0 0.25rem;
  font-size: 0.75rem;
  color: var(--text-muted);
}
.ai-suggestion-chip {
  text-align: left;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 0.5rem 0.7rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
  cursor: pointer;
  transition: border-color 0.15s, color 0.15s;
}
.ai-suggestion-chip:hover {
  border-color: var(--border-active);
  color: var(--text);
}
</style>
