---
title: "Budget: preventivo e consuntivo"
slug: "budget"
section: "Contabilità"
order: 56
---

Il **budget** raccoglie le voci di spesa ed entrata di un esercizio. Esistono due tipi:
- **Preventivo** — le spese previste; alla sua approvazione si **generano le rate**.
- **Consuntivo** — le spese effettive a fine periodo, base per il **conguaglio**.

Per ogni esercizio può esistere **un solo preventivo** e **un solo consuntivo** nello stato approvato/chiuso.

## Creare un budget
1. Apri **Budget** e seleziona l'**esercizio fiscale**.
2. Premi **+ Nuovo budget** e scegli il **Tipo** (Preventivo o Consuntivo).
3. Per un preventivo puoi opzionalmente partire da un **consuntivo di base** applicando una **maggiorazione %**.
4. Salva.

## Inserire le voci
Premi **Voci** per aprire l'editor. Le voci seguono la gerarchia del **piano dei conti**, suddivise nei tab **Uscite / Entrate / Patrimoniale**. Per ogni voce-foglia inserisci la **competenza** (importo); i conti padre mostrano in automatico la **somma** dei figli. In alto trovi i totali di **Entrate**, **Uscite** e **Saldo**.

Con **+ Aggiungi voci** scegli dal piano dei conti quali voci includere (sono selezionabili solo le foglie non già presenti).

## Workflow del preventivo
1. **Bozza** → **Pre-approva** (passa in *In approvazione*).
2. **In approvazione** → **Approva definitivamente**: qui indichi il **numero di rate** (1–24) e la **prima scadenza**; le successive vengono distanziate di un mese. All'approvazione le **rate vengono generate**.
3. **Approvato** → **Chiudi**.

Dalla fase *In approvazione* puoi anche **creare un'assemblea** collegata. Da un preventivo approvato, **Rate ↗** porta alle rate generate; **Genera rate** permette di rigenerarle.

## Workflow del consuntivo
1. **Bozza** → **Approva** (salva automaticamente rate/quote).
2. **Approvato** → **Chiudi**.

Strumenti del consuntivo:
- **Ricalcola voci** — ricarica gli importi dalle spese registrate (sovrascrive le modifiche manuali).
- **Rigenera ripartizioni** — riapplica i millesimi a tutte le spese.
- **Dettaglio** — apre l'analisi delle spese ripartite per unità e per conto.

## Modifica ed eliminazione
- **✎ Modifica** è possibile in Bozza o In approvazione.
- **🗑 Elimina** solo in Bozza.
