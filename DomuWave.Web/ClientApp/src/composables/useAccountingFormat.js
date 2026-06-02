import { ref } from 'vue'
import { tenantDisplaySettingsApi } from '@/services/api'

// Convenzioni segno (allineate all'enum backend AccountingSignConvention)
export const SIGN_CONVENTION = {
  SOLO_COLORE:     0, // magnitudine + colore (default storico)
  SEGNO_ESPLICITO: 1, // uscite con -, entrate con +
}

// Stato singleton condiviso tra tutti i consumatori del composable.
const signConvention = ref(SIGN_CONVENTION.SOLO_COLORE)
const loaded         = ref(false)
let   loadingPromise = null

/**
 * Carica le impostazioni di visualizzazione del tenant (una sola volta).
 * Fail-safe: in caso di errore mantiene il default SoloColore.
 */
async function load(force = false) {
  if (loaded.value && !force) return
  if (loadingPromise) return loadingPromise

  loadingPromise = tenantDisplaySettingsApi.get()
    .then(({ data }) => {
      signConvention.value = data?.accountingSignConvention ?? SIGN_CONVENTION.SOLO_COLORE
      loaded.value = true
    })
    .catch(() => { /* default già impostato */ })
    .finally(() => { loadingPromise = null })

  return loadingPromise
}

const nf = new Intl.NumberFormat('it-IT', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

/**
 * Formatta un valore contabile applicando la convenzione del tenant.
 * @param {number|null} value  importo (sempre fornito come magnitudine o valore reale)
 * @param {'expense'|'income'|'balance'} kind
 *   - 'expense' = uscita: in SegnoEsplicito mostrata negativa
 *   - 'income'  = entrata: in SegnoEsplicito mostrata positiva (+)
 *   - 'balance' = saldo netto: mostrato sempre con il suo segno reale
 */
function fmtAccounting(value, kind = 'balance') {
  if (value == null) return '—'
  const n = Number(value)

  if (signConvention.value === SIGN_CONVENTION.SEGNO_ESPLICITO) {
    if (kind === 'expense') {
      // L'uscita riduce il saldo → segno meno sulla magnitudine
      const mag = Math.abs(n)
      return mag === 0 ? '€ 0,00' : `€ -${nf.format(mag)}`
    }
    if (kind === 'income') {
      const mag = Math.abs(n)
      return mag === 0 ? '€ 0,00' : `€ +${nf.format(mag)}`
    }
    // balance: segno reale (Intl gestisce il meno)
    return '€ ' + nf.format(n)
  }

  // SoloColore: magnitudine col valore così com'è (segno reale solo per balance),
  // il colore è responsabilità del template.
  if (kind === 'expense' || kind === 'income') return '€ ' + nf.format(Math.abs(n))
  return '€ ' + nf.format(n)
}

export function useAccountingFormat() {
  return {
    signConvention,
    loaded,
    load,
    fmtAccounting,
    SIGN_CONVENTION,
  }
}
