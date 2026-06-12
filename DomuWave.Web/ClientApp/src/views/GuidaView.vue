<template>
  <div class="guida" :class="{ 'is-public': !isAuthenticated }">
    <!-- ── Topbar ── -->
    <header class="g-topbar">
      <RouterLink class="g-brand" :to="isAuthenticated ? '/dashboard' : '/'">
        <span class="g-brand-mark">
          <svg viewBox="0 0 40 40" fill="none">
            <rect x="4" y="14" width="14" height="22" rx="2" fill="#2e9c6c" />
            <rect x="22" y="6" width="14" height="30" rx="2" fill="#c9a55c" />
          </svg>
        </span>
        DomuWave
      </RouterLink>
      <span class="g-topbar-title">Guida all'utilizzo</span>
      <RouterLink class="g-back" :to="isAuthenticated ? '/dashboard' : '/'">
        {{ isAuthenticated ? '← Torna all’app' : '← Home' }}
      </RouterLink>
    </header>

    <div class="g-body">
      <!-- ── Sidebar: ricerca + navigazione ── -->
      <aside class="g-nav">
        <div class="g-search">
          <input
            class="g-search-input"
            type="search"
            v-model="query"
            placeholder="Cerca nella guida…"
            aria-label="Cerca nella guida"
          />
        </div>

        <!-- Risultati ricerca -->
        <nav v-if="query.trim()" class="g-results">
          <p v-if="!searchResults.length" class="g-empty">Nessun risultato per “{{ query }}”.</p>
          <ul v-else>
            <li v-for="a in searchResults" :key="a.slug">
              <a href="#" @click.prevent="open(a.slug)" :class="{ active: a.slug === currentSlug }">
                <span class="g-result-title">{{ a.title }}</span>
                <span class="g-result-section">{{ a.section }}</span>
              </a>
            </li>
          </ul>
        </nav>

        <!-- Navigazione per sezioni -->
        <nav v-else class="g-sections">
          <div v-for="s in sections" :key="s.title" class="g-section">
            <div class="g-section-title">{{ s.title }}</div>
            <ul>
              <li v-for="a in s.articles" :key="a.slug">
                <a href="#" @click.prevent="open(a.slug)" :class="{ active: a.slug === currentSlug }">
                  {{ a.title }}
                </a>
              </li>
            </ul>
          </div>
        </nav>
      </aside>

      <!-- ── Contenuto articolo ── -->
      <main class="g-content">
        <article v-if="current" :id="current.slug" class="g-article">
          <div class="g-article-eyebrow">{{ current.section }}</div>
          <h1 class="g-article-title">{{ current.title }}</h1>
          <div class="g-markdown" v-html="renderedHtml"></div>
        </article>
        <div v-else class="g-empty-state">Seleziona un argomento dalla guida.</div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MarkdownIt from 'markdown-it'
import DOMPurify from 'dompurify'
import { useAuthStore } from '@/stores/authStore'
import { guideArticles, guideSections, getArticleBySlug } from '@/guide'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const isAuthenticated = computed(() => authStore.isAuthenticated)

const md = new MarkdownIt({ html: false, linkify: true, breaks: false })

const sections = guideSections
const query = ref('')

// ── Articolo corrente (da route param, default = primo articolo) ─────────────
const currentSlug = computed(() => route.params.slug || guideArticles[0]?.slug || '')
const current = computed(() => getArticleBySlug(currentSlug.value) || guideArticles[0] || null)
const renderedHtml = computed(() =>
  current.value ? DOMPurify.sanitize(md.render(current.value.body)) : ''
)

// ── Ricerca client-side (titolo + sezione + corpo) ───────────────────────────
const searchResults = computed(() => {
  const q = query.value.trim().toLowerCase()
  if (!q) return []
  return guideArticles.filter(a =>
    a.title.toLowerCase().includes(q) ||
    a.section.toLowerCase().includes(q) ||
    a.plain.toLowerCase().includes(q)
  )
})

function open(slug) {
  if (slug !== route.params.slug) router.push(`/guida/${slug}`)
}

// Porta in cima al cambio articolo
watch(currentSlug, () => {
  document.querySelector('.g-content')?.scrollTo({ top: 0, behavior: 'smooth' })
})

onMounted(() => {
  // Se si arriva su /guida senza slug, normalizza al primo articolo
  if (!route.params.slug && guideArticles[0]) {
    router.replace(`/guida/${guideArticles[0].slug}`)
  }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600&display=swap');

/* Tema di default = dentro l'app (scuro). .is-public sovrascrive col tema chiaro. */
.guida {
  --g-bg: #0f172a;
  --g-surface: #111c33;
  --g-border: #1e293b;
  --g-text: #e2e8f0;
  --g-text-soft: #94a3b8;
  --g-muted: #64748b;
  --g-accent: #34d399;
  --g-accent-soft: rgba(52, 211, 153, 0.12);

  font-family: 'Outfit', system-ui, sans-serif;
  background: var(--g-bg);
  color: var(--g-text);
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.guida.is-public {
  --g-bg: #f6f2e9;
  --g-surface: #fffdf8;
  --g-border: rgba(20, 32, 26, 0.12);
  --g-text: #14201a;
  --g-text-soft: #4d5c54;
  --g-muted: #8a978f;
  --g-accent: #2e9c6c;
  --g-accent-soft: rgba(46, 156, 108, 0.12);
}

/* ── Topbar ── */
.g-topbar {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 14px 24px;
  border-bottom: 1px solid var(--g-border);
  background: var(--g-surface);
  position: sticky;
  top: 0;
  z-index: 5;
}
.g-brand {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
  font-size: 19px;
  color: var(--g-text);
  text-decoration: none;
}
.g-brand-mark { width: 28px; height: 28px; }
.g-brand-mark svg { width: 100%; height: 100%; }
.g-topbar-title {
  font-size: 14px;
  color: var(--g-text-soft);
  border-left: 1px solid var(--g-border);
  padding-left: 16px;
}
.g-back {
  margin-left: auto;
  font-size: 14px;
  color: var(--g-accent);
  text-decoration: none;
}
.g-back:hover { text-decoration: underline; }

/* ── Body layout ── */
.g-body {
  display: grid;
  grid-template-columns: 280px 1fr;
  flex: 1;
  min-height: 0;
}

/* ── Sidebar nav ── */
.g-nav {
  border-right: 1px solid var(--g-border);
  background: var(--g-surface);
  padding: 18px 14px;
  overflow-y: auto;
  max-height: calc(100vh - 57px);
  position: sticky;
  top: 57px;
}
.g-search { margin-bottom: 16px; }
.g-search-input {
  width: 100%;
  box-sizing: border-box;
  padding: 10px 14px;
  border-radius: 10px;
  border: 1px solid var(--g-border);
  background: var(--g-bg);
  color: var(--g-text);
  font-family: inherit;
  font-size: 14px;
  outline: none;
}
.g-search-input:focus { border-color: var(--g-accent); }
.g-search-input::placeholder { color: var(--g-muted); }

.g-section { margin-bottom: 18px; }
.g-section-title {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--g-muted);
  margin-bottom: 8px;
  padding: 0 8px;
}
.g-sections ul, .g-results ul { list-style: none; margin: 0; padding: 0; }
.g-sections a, .g-results a {
  display: block;
  padding: 8px 10px;
  border-radius: 8px;
  color: var(--g-text-soft);
  text-decoration: none;
  font-size: 14px;
  line-height: 1.35;
  transition: background 0.12s, color 0.12s;
}
.g-sections a:hover, .g-results a:hover { background: var(--g-accent-soft); color: var(--g-text); }
.g-sections a.active, .g-results a.active {
  background: var(--g-accent-soft);
  color: var(--g-accent);
  font-weight: 600;
}
.g-results a { display: flex; flex-direction: column; gap: 2px; }
.g-result-section { font-size: 11px; color: var(--g-muted); }
.g-empty { font-size: 13px; color: var(--g-muted); padding: 8px 10px; }

/* ── Content ── */
.g-content {
  overflow-y: auto;
  max-height: calc(100vh - 57px);
  padding: 36px 44px 80px;
}
.g-article { max-width: 760px; }
.g-article-eyebrow {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--g-accent);
  margin-bottom: 8px;
}
.g-article-title { font-size: 30px; font-weight: 600; margin: 0 0 24px; color: var(--g-text); }
.g-empty-state { color: var(--g-muted); padding: 40px; }

/* ── Markdown rendering ── */
.g-markdown { font-size: 15.5px; line-height: 1.7; color: var(--g-text); }
.g-markdown :deep(h2) {
  font-size: 20px; font-weight: 600; margin: 28px 0 12px; color: var(--g-text);
}
.g-markdown :deep(h3) {
  font-size: 16px; font-weight: 600; margin: 20px 0 8px; color: var(--g-text);
}
.g-markdown :deep(p) { margin: 0 0 14px; }
.g-markdown :deep(ul), .g-markdown :deep(ol) { margin: 0 0 14px; padding-left: 22px; }
.g-markdown :deep(li) { margin: 4px 0; }
.g-markdown :deep(strong) { color: var(--g-text); font-weight: 600; }
.g-markdown :deep(a) { color: var(--g-accent); }
.g-markdown :deep(code) {
  background: var(--g-accent-soft);
  padding: 1px 6px; border-radius: 5px; font-size: 0.9em;
}

@media (max-width: 800px) {
  .g-body { grid-template-columns: 1fr; }
  .g-nav { position: static; max-height: none; border-right: none; border-bottom: 1px solid var(--g-border); }
  .g-content { max-height: none; padding: 24px 20px 60px; }
}
</style>
