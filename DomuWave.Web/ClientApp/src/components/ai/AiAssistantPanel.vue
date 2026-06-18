<template>
  <!-- Pulsante flottante di apertura -->
  <button
    v-if="!ai.isOpen"
    type="button"
    class="ai-fab"
    title="Assistente AI"
    @click="ai.open()"
  >
    <i class="pi pi-sparkles"></i>
  </button>

  <!-- Overlay + pannello -->
  <transition name="ai-fade">
    <div v-if="ai.isOpen" class="ai-overlay" @click.self="ai.close()">
      <aside class="ai-panel" role="dialog" aria-label="Assistente AI">

        <!-- Header -->
        <header class="ai-panel-header">
          <div class="ai-panel-title">
            <i class="pi pi-sparkles"></i>
            <span>Assistente AI</span>
          </div>
          <div class="ai-panel-actions">
            <button class="btn-icon" title="Nuova chat" @click="ai.clearChat()">
              <i class="pi pi-refresh"></i>
            </button>
            <button class="btn-icon" title="Chiudi" @click="ai.close()">
              <i class="pi pi-times"></i>
            </button>
          </div>
        </header>

        <!-- Area messaggi -->
        <div ref="scrollArea" class="ai-panel-body">
          <template v-if="ai.messages.length === 0">
            <div class="ai-empty">
              <p class="ai-empty-text">
                Ciao! Posso aiutarti a consultare i dati dei tuoi condomini.
              </p>
              <AiSuggestions @select="onSuggestion" />
            </div>
          </template>

          <AiMessage
            v-for="(m, i) in ai.messages"
            :key="i"
            :role="m.role"
            :content="m.content"
          />

          <div v-if="ai.loading" class="ai-typing">
            <span class="dot"></span><span class="dot"></span><span class="dot"></span>
          </div>
        </div>

        <!-- Input -->
        <footer class="ai-panel-footer">
          <textarea
            ref="inputEl"
            v-model="draft"
            class="ai-input"
            rows="1"
            placeholder="Scrivi una domanda…"
            :disabled="ai.loading"
            @keydown.enter.exact.prevent="send"
          ></textarea>
          <button
            class="ai-send-btn"
            :disabled="ai.loading || !draft.trim()"
            title="Invia"
            @click="send"
          >
            <i class="pi pi-send"></i>
          </button>
        </footer>

      </aside>
    </div>
  </transition>
</template>

<script setup>
import { ref, nextTick, watch } from 'vue'
import { useAiStore } from '@/stores/aiStore'
import { useAppStore } from '@/stores/app'
import AiMessage from './AiMessage.vue'
import AiSuggestions from './AiSuggestions.vue'

const ai = useAiStore()
const app = useAppStore()

const draft = ref('')
const scrollArea = ref(null)
const inputEl = ref(null)

async function send() {
  const text = draft.value.trim()
  if (!text || ai.loading) return
  draft.value = ''
  await ai.sendQuestion(text, app.selectedCondominioId ?? null)
}

function onSuggestion(s) {
  draft.value = s
  send()
}

async function scrollToBottom() {
  await nextTick()
  if (scrollArea.value) scrollArea.value.scrollTop = scrollArea.value.scrollHeight
}

// Scroll automatico all'ultimo messaggio / durante il caricamento
watch(() => [ai.messages.length, ai.loading], scrollToBottom)
watch(() => ai.isOpen, (open) => { if (open) nextTick(() => inputEl.value?.focus()) })
</script>

<style scoped>
.ai-fab {
  position: fixed;
  right: 1.5rem;
  bottom: 1.5rem;
  width: 54px;
  height: 54px;
  border-radius: 50%;
  border: none;
  background: var(--accent);
  color: #fff;
  font-size: 1.3rem;
  cursor: pointer;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.25);
  z-index: 1200;
  transition: transform 0.15s;
}
.ai-fab:hover { transform: scale(1.06); }

.ai-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.25);
  z-index: 1300;
  display: flex;
  justify-content: flex-end;
}

.ai-panel {
  width: 420px;
  max-width: 100vw;
  height: 100vh;
  background: var(--bg-base);
  border-left: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  box-shadow: -8px 0 24px rgba(0, 0, 0, 0.15);
}

.ai-panel-header {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.85rem 1rem;
  border-bottom: 1px solid var(--border);
}
.ai-panel-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
  color: var(--text);
}
.ai-panel-title .pi-sparkles { color: var(--accent); }
.ai-panel-actions { display: flex; gap: 0.25rem; }

.btn-icon {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  width: 32px;
  height: 32px;
  border-radius: 8px;
}
.btn-icon:hover { background: var(--bg-surface); color: var(--text); }

.ai-panel-body {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
}

.ai-empty-text {
  font-size: 0.85rem;
  color: var(--text-secondary);
  margin: 0 0 0.5rem;
}

.ai-typing {
  display: flex;
  gap: 4px;
  padding: 0.5rem 0.25rem;
}
.ai-typing .dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--text-muted);
  animation: ai-blink 1.2s infinite ease-in-out both;
}
.ai-typing .dot:nth-child(2) { animation-delay: 0.2s; }
.ai-typing .dot:nth-child(3) { animation-delay: 0.4s; }
@keyframes ai-blink {
  0%, 80%, 100% { opacity: 0.2; }
  40% { opacity: 1; }
}

.ai-panel-footer {
  flex-shrink: 0;
  display: flex;
  gap: 0.5rem;
  align-items: flex-end;
  padding: 0.75rem 1rem;
  border-top: 1px solid var(--border);
}
.ai-input {
  flex: 1;
  resize: none;
  max-height: 120px;
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 0.55rem 0.7rem;
  font-size: 0.85rem;
  font-family: inherit;
  background: var(--bg-surface);
  color: var(--text);
}
.ai-input:focus { outline: none; border-color: var(--border-active); }

.ai-send-btn {
  flex-shrink: 0;
  width: 38px;
  height: 38px;
  border-radius: 10px;
  border: none;
  background: var(--accent);
  color: #fff;
  cursor: pointer;
}
.ai-send-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.ai-fade-enter-active, .ai-fade-leave-active { transition: opacity 0.2s; }
.ai-fade-enter-from, .ai-fade-leave-to { opacity: 0; }
</style>
