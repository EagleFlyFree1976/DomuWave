---
title: "Condomìni e unità immobiliari"
slug: "condomini-e-unita"
section: "Anagrafiche"
order: 40
---

## Creare un condominio
1. Apri **Condomìni** e premi **+ Nuovo condominio**.
2. Compila i dati. I campi **obbligatori** sono **Nome**, **N° unità**, **N° scale** e **Giorno scadenza rata**.
3. Premi **Salva**.

Il **Codice** viene generato automaticamente dal nome (es. `CON-2024-001`), ma puoi modificarlo. Il modulo è diviso in sezioni:

- **Dati anagrafici** — Nome, Codice, Codice fiscale, Partita IVA.
- **Dati tecnici** — N° unità, N° scale, N° piani, Anno di costruzione, Millesimi totali, e le caselle *Ascensore*, *Riscaldamento centralizzato*, *Portineria* (selezionando Ascensore compare anche **N° ascensori**).
- **Gestione amministrativa** — Frequenza rate (*Mensile, Trimestrale, Semestrale, Annuale*), **Giorno scadenza rata** (1–31), inizio/fine mandato, ultima assemblea.
- **Indirizzo** — Via, Civico, CAP, Città, Provincia, Nazione.
- **Note** e casella **Condominio attivo**.

In **modifica** sono disponibili anche **Dati bancari** (IBAN, intestatario, banca) e i dati dell'**Amministratore** (nome, telefono, email).

> La **Partita IVA**, se compilata, viene validata; **Fine mandato** non può precedere l'**Inizio mandato**.

## Il dettaglio del condominio
Clicca sul nome di un condominio per aprire il **dettaglio**, che riepiloga in sola lettura dati anagrafici, tecnici, amministrativi, bancari, indirizzo e note. Usa **✎ Modifica** in alto a destra per aggiornare i dati.

Nell'elenco, la colonna **Setup** mostra una pillola tipo `3/6`: indica quanti passaggi di configurazione sono completati. Cliccala per aprire la **checklist** del condominio.

## Unità immobiliari
Da un condominio apri **Unità** per gestire gli immobili.

1. Premi **+ Nuova unità**.
2. L'unico campo **obbligatorio** è il **Piano**.
3. Salva.

Per ogni unità puoi indicare:
- **Identificazione** — N° interno, Edificio, Scala, Piano.
- **Classificazione** — Tipo unità (*Residenziale, Commerciale, Artigianale, Direzionale, Autorimessa, Cantina, Deposito, Altro*), Categoria catastale, Stato occupazione (*Occupata proprietario, Occupata inquilino, Libera, Non abitabile*).
- **Dati catastali e superfici** — Foglio, Particella, Subalterno, Superficie (mq), Vani, Rendita catastale, Numero abitanti.
- **Note** e casella **Unità attiva**.

Azioni disponibili su ogni unità:
- **👤 Occupanti** — gestisci proprietari e inquilini (vedi *Occupanti*).
- **₿ Bilancio iniziale** — imposta il saldo di apertura per un esercizio fiscale.
- **⧉ Clona unità** — duplica l'unità (mantiene i dati catastali, azzera il numero interno).
- **✎ Modifica** e **✕ Elimina**.

Puoi filtrare l'elenco per **edificio**, **scala**, **tipo** e **stato**, oppure cercare per numero interno.

## Bilancio iniziale dell'unità
Dal pulsante **₿ Bilancio iniziale** scegli l'**esercizio fiscale** e inserisci il **Saldo di apertura**:
- valore **positivo** = credito dell'unità verso il condominio;
- valore **negativo** = debito.

Se l'esercizio è **chiuso**, il bilancio è mostrato in sola lettura e non è modificabile.

## Edifici e scale
Se il condominio è composto da più corpi di fabbrica, usa **Edifici** per censirli (Nome obbligatorio; indirizzo, anno, piani, ascensore facoltativi) e **Scale** per le scale.

Le scale possono essere create una a una oppure **generate automaticamente**: indica il numero di scale da creare e l'app aggiunge solo quelle mancanti, assegnando nomi progressivi (A, B, C… poi 1, 2, 3…). Le scale **senza edificio** valgono per l'intero condominio.
