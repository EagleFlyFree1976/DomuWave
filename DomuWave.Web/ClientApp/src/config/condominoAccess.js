/**
 * condominoAccess.js
 *
 * Unica fonte di verità per il perimetro della "versione condòmino" (profile == 3).
 *
 * Definisce quali route/sezioni sono accessibili a un utente Condomino e con
 * quale livello (lettura o scrittura). Usato da:
 *   - router/index.js   → navigation guard che blocca le route non consentite
 *   - AppSidebar.vue    → filtro del menu laterale
 *   - usePermissions.js → calcolo di canEdit/canCreate per-sezione
 *
 * Le chiavi sono i path "logici" (senza prefisso dinamico tipo /condomini/:id).
 * Per le route annidate sotto /condomini/:id usiamo il suffisso (es. 'unita').
 */

/**
 * Sezioni che il condòmino può VEDERE.
 * Tutto ciò che non è elencato qui è negato (deny-by-default).
 *
 * Per ogni sezione visibile:
 *   `edit`   → può modificare gli elementi esistenti (propri, filtrati lato backend)
 *   `create` → può creare nuovi elementi
 * Entrambi assenti = sola lettura. Il condòmino non elimina mai (canDelete = false).
 *
 * Nota: il pagamento online delle rate NON è un `edit` generico ma un'azione
 * dedicata, gestita con un pulsante a parte gated su `isCondomino`.
 */
export const CONDOMINO_SECTIONS = {
  // ── Trasversali ────────────────────────────────────────────────
  '/dashboard':      {},
  '/guida':          {},

  // ── Condominio (lettura) ──────────────────────────────────────
  '/condomini':      {},

  // ── Anagrafica propria: modifica le proprie unità/occupanti,
  //    ma non può crearne/eliminarne di nuove ───────────────────
  'unita':           { edit: true },
  'panoramica':      { edit: true },

  // ── Struttura condominio (lettura) ────────────────────────────
  'edifici':         {},
  'scale':           {},
  'tabelle-millesimali': {},

  // ── Contabilità / bilancio (lettura) ─────────────────────────
  'budget':          {},
  'consuntivo':      {},
  'rendiconto':      {},
  'spese':           {},
  'report-spese-millesimali':    {},
  'report-bilancio-ripartizione': {},
  'esercizi-fiscali': {},

  // ── Rate & pagamenti (lettura; paga online = azione dedicata) ─
  'rate':            {},

  // ── Fornitori / fatture (lettura) ────────────────────────────
  'fornitori':       {},
  'fatture-elettroniche': {},

  // ── Consumi / manutenzioni (lettura) ─────────────────────────
  'consumi':         {},
  'manutenzioni':    {},
  'lavori-straordinari': {},

  // ── Documenti / comunicazioni (lettura) ──────────────────────
  'documenti':       {},
  'comunicazioni':   {},

  // ── Bacheca / Centro comunicazioni (legge, vota, scrive post) ─
  'centro-comunicazioni': { create: true, edit: true },

  // ── Assemblee (lettura) ──────────────────────────────────────
  'assemblee':       {},
}

/**
 * Estrae la chiave di sezione da un path completo.
 * Es. '/condomini/12/unita' → 'unita'
 *     '/dashboard'          → '/dashboard'
 *     '/rate'               → 'rate'
 */
export function sectionKey(path) {
  if (!path) return ''
  // Path trasversali a prefisso fisso
  if (path === '/dashboard') return '/dashboard'
  if (path === '/guida') return '/guida'
  if (path === '/condomini') return '/condomini'

  // Prendi l'ultimo segmento "parlante" (ignora gli id numerici)
  const segments = path.split('/').filter(Boolean)
  for (let i = segments.length - 1; i >= 0; i--) {
    const seg = segments[i]
    if (!/^\d+$/.test(seg)) return seg
  }
  return path
}

/** True se il condòmino può accedere (vedere) alla sezione di questo path. */
export function condominoCanAccess(path) {
  // Sempre consentite: la pagina di dettaglio del proprio condominio
  if (/^\/condomini\/\d+\/?$/.test(path)) return true
  const key = sectionKey(path)
  return Object.prototype.hasOwnProperty.call(CONDOMINO_SECTIONS, key)
}

/** True se il condòmino può MODIFICARE elementi esistenti nella sezione. */
export function condominoCanEdit(path) {
  const section = CONDOMINO_SECTIONS[sectionKey(path)]
  return section ? section.edit === true : false
}

/** True se il condòmino può CREARE nuovi elementi nella sezione. */
export function condominoCanCreate(path) {
  const section = CONDOMINO_SECTIONS[sectionKey(path)]
  return section ? section.create === true : false
}
