// Caricatore degli articoli della guida.
//
// Gli articoli sono file Markdown con frontmatter YAML in `src/guide/*.md`.
// Vengono importati a build-time come stringhe raw (eager) tramite import.meta.glob,
// così la guida è interamente client-side e non richiede chiamate al backend.
//
// In Fase 2 lo stesso insieme di file Markdown è letto anche dal backend (RAG):
// `src/guide/` resta l'unica fonte canonica dei contenuti.

const modules = import.meta.glob('./*.md', { query: '?raw', import: 'default', eager: true })

// ── Parsing frontmatter (--- ... --- in testa al file) ───────────────────────
// Frontmatter minimale e di forma fissa: parsiamo a mano coppie chiave: valore,
// rimuovendo eventuali apici. Niente dipendenze YAML.
function parseFrontmatter(raw) {
  const match = /^---\s*\r?\n([\s\S]*?)\r?\n---\s*\r?\n?([\s\S]*)$/.exec(raw)
  if (!match) return { meta: {}, body: raw.trim() }

  const meta = {}
  for (const line of match[1].split(/\r?\n/)) {
    const m = /^\s*([A-Za-z0-9_-]+)\s*:\s*(.*)\s*$/.exec(line)
    if (!m) continue
    let value = m[2].trim()
    if ((value.startsWith('"') && value.endsWith('"')) ||
        (value.startsWith("'") && value.endsWith("'"))) {
      value = value.slice(1, -1)
    }
    meta[m[1]] = value
  }
  return { meta, body: match[2].trim() }
}

// Rimuove la sintassi Markdown per ottenere testo semplice (usato dalla ricerca).
function toPlainText(md) {
  return md
    .replace(/```[\s\S]*?```/g, ' ')      // blocchi di codice
    .replace(/`[^`]*`/g, ' ')             // codice inline
    .replace(/!\[[^\]]*\]\([^)]*\)/g, ' ')// immagini
    .replace(/\[([^\]]*)\]\([^)]*\)/g, '$1') // link → testo
    .replace(/^[#>\-*+\d.]+\s*/gm, ' ')   // marcatori di lista/heading
    .replace(/[*_~]/g, ' ')               // enfasi
    .replace(/\s+/g, ' ')
    .trim()
}

// ── Costruzione elenco articoli ──────────────────────────────────────────────
const articles = Object.entries(modules)
  .map(([path, raw]) => {
    const { meta, body } = parseFrontmatter(raw)
    const slug = meta.slug || path.replace(/^.*\/(\d+-)?/, '').replace(/\.md$/, '')
    return {
      slug,
      title: meta.title || slug,
      section: meta.section || 'Guida',
      order: Number(meta.order ?? 999),
      body,
      plain: toPlainText(body),
    }
  })
  .sort((a, b) => a.order - b.order)

// Articoli in ordine, pronti per nav e render.
export const guideArticles = articles

// Sezioni ordinate, ognuna con i suoi articoli (ordine = min order dei figli).
export const guideSections = (() => {
  const map = new Map()
  for (const a of articles) {
    if (!map.has(a.section)) map.set(a.section, { title: a.section, order: a.order, articles: [] })
    const s = map.get(a.section)
    s.order = Math.min(s.order, a.order)
    s.articles.push(a)
  }
  return [...map.values()].sort((x, y) => x.order - y.order)
})()

export function getArticleBySlug(slug) {
  return articles.find(a => a.slug === slug) || null
}
