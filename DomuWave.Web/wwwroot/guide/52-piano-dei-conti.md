---
title: "Piano dei conti"
slug: "piano-dei-conti"
section: "Contabilità"
order: 52
---

Il **piano dei conti** è l'archivio dei conti contabili (entrate, uscite, voci patrimoniali) usati in budget e spese. I conti possono essere organizzati in **gerarchia** e portano le regole di **ripartizione** delle spese.

## Creare un conto
1. Apri **Piano dei conti** e premi **+ Nuovo Conto**.
2. Compila i campi obbligatori: **Codice** (es. `1000`), **Nome** e **Tipo** (*Entrata*, *Uscita*, *Patrimoniale*).
3. Imposta, se serve: **Categoria**, **Tabella millesimale di default**, **Conto padre** (per la gerarchia) e la regola **A carico di**.
4. Salva.

### Ripartizione delle spese
Per ogni conto scegli il **metodo di calcolo**:
- **Standard** — ripartizione 100% in base ai millesimi.
- **Misto** — combina millesimi e altri pesi: indichi la **% millesimale** e i pesi per **piano** e **numero di abitanti**.

### A carico di
Definisce chi paga la spesa imputata a quel conto:
- **Proprietario**
- **Inquilino** (se presente)
- **Automatico** — inquilino attivo se c'è, altrimenti il proprietario.

## Gerarchia dei conti
Un conto può avere dei **sotto-conti**. In quel caso il suo importo è la **somma** dei discendenti e non è modificabile direttamente. Un conto con sotto-conti **non può essere eliminato** finché esistono i figli.

## Eliminare un conto
- Se il conto è usato in **budget approvati/chiusi**: viene disattivato senza ricalcoli.
- Se è usato in un **budget in bozza**: devi indicare un **conto sostitutivo** a cui riassegnare le voci.
- Se non è usato da nessuna parte: viene semplicemente disattivato.

## Categorie
Le **categorie** (es. *Ordinaria*, *Straordinaria*, *Manutenzione*) servono a organizzare i conti. Le gestisci da **Gestisci Categorie**: ogni categoria ha un **Nome** (obbligatorio) e una descrizione, può essere riutilizzata su più conti e importata dai **template**.

## Strumenti utili
- **Applica template** — inizializza il piano dei conti da un modello predefinito.
- **Copia da…** — copia i conti da un altro condominio.
- **🔁 Clona** — duplica un conto (richiede un nuovo codice).
