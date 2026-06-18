<template>
  <div class="ai-msg" :class="role === 'user' ? 'ai-msg--user' : 'ai-msg--assistant'">
    <div class="ai-msg-avatar" v-if="role === 'assistant'">
      <i class="pi pi-sparkles"></i>
    </div>
    <div class="ai-msg-bubble">
      <p class="ai-msg-text">{{ content }}</p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  role: { type: String, default: 'assistant' },   // 'user' | 'assistant'
  content: { type: String, default: '' },
})
</script>

<style scoped>
.ai-msg {
  display: flex;
  gap: 0.5rem;
  align-items: flex-end;
  margin-bottom: 0.75rem;
}
.ai-msg--user { justify-content: flex-end; }

.ai-msg-avatar {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--accent-glow);
  color: var(--accent);
  font-size: 0.8rem;
}

.ai-msg-bubble {
  max-width: 80%;
  padding: 0.6rem 0.85rem;
  border-radius: 14px;
  font-size: 0.85rem;
  line-height: 1.45;
}
.ai-msg--assistant .ai-msg-bubble {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-bottom-left-radius: 4px;
  color: var(--text);
}
.ai-msg--user .ai-msg-bubble {
  background: var(--accent);
  color: #fff;
  border-bottom-right-radius: 4px;
}

.ai-msg-text {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
