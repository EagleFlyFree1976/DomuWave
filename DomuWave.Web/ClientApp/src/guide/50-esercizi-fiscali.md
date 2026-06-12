---
title: "Esercizi fiscali"
slug: "esercizi-fiscali"
section: "Contabilità"
order: 50
---

L'**esercizio fiscale** è il periodo contabile (in genere un anno) su cui si registrano spese, rate, movimenti e saldi. Quasi tutte le operazioni di contabilità sono legate a un esercizio.

## Creare un esercizio
1. Apri **Esercizi fiscali** e premi **+ Nuovo esercizio**.
2. Compila i campi obbligatori: **Codice** (es. `2025`), **Data inizio** e **Data fine** (deve essere successiva all'inizio). La **Descrizione** è facoltativa.
3. Se non è il primo esercizio, indica l'**esercizio precedente**: le date vengono precompilate e il saldo di chiusura precedente viene propagato.
4. Salva.

> Può esistere **un solo esercizio aperto** per condominio alla volta.

## Saldi iniziali
Per il **primo** esercizio puoi inserire manualmente i **saldi iniziali per unità** (pulsante **Saldi iniziali** quando l'esercizio è in Bozza). Per gli esercizi successivi i saldi vengono propagati automaticamente dalla chiusura del precedente; con **Propaga saldi** puoi ricopiarli di nuovo.

Espandendo un esercizio vedi il **riepilogo finanziario** (saldo anno precedente, spese totali e pagate, rate emesse, incassi, saldo) e i dettagli **per conto** e **per unità**.

## Stati dell'esercizio
L'esercizio attraversa questi stati:

1. **Bozza** — appena creato: puoi inserire i saldi iniziali, modificare o eliminare l'esercizio.
2. **Aperto** — registrazioni attive (spese, rate, ecc.).
3. **In chiusura** — calcoli finali in corso.
4. **Chiuso** — definitivo.
5. **Bloccato** — nessuna modifica consentita.

### Azioni principali
- **Apri** — porta da Bozza ad Aperto.
- **Avvia chiusura** — da Aperto a In chiusura.
- **Chiudi** — da In chiusura a Chiuso.
- **Blocca** — da Chiuso a Bloccato.
- **Riporta in Bozza** — disponibile su un esercizio **Aperto**, per correggere i saldi iniziali; consentito solo se **non** ci sono movimenti collegati (budget approvati, spese, rate).
- **✎ Modifica** (descrizione e data fine) e **🗑 Elimina** (solo in Bozza).

> Le **spese** si possono registrare solo quando l'esercizio è **Aperto** o **In chiusura**.
