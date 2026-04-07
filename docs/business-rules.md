# DomuWave — Regole di Business

Questo documento raccoglie tutte le regole di business applicate dal backend di DomuWave.
Le regole sono ordinate per dominio e riflettono i vincoli implementati nei Consumer e nei Service.

---

## Indice

1. [Condominio](#1-condominio)
2. [Unità Immobiliare](#2-unità-immobiliare)
3. [Esercizio Fiscale](#3-esercizio-fiscale)
4. [Budget](#4-budget)
5. [Voci di Budget (BudgetItem)](#5-voci-di-budget-budgetitem)
6. [Spese (Expense)](#6-spese-expense)
7. [Piano dei Conti (ChartOfAccounts)](#7-piano-dei-conti-chartofaccounts)
8. [Tabella Millesimale](#8-tabella-millesimale)
9. [Saldo Iniziale Unità (UnitOpeningBalance)](#9-saldo-iniziale-unità-unitopeningbalance)
10. [Saldo Conto (AccountBalance)](#10-saldo-conto-accountbalance)
11. [Rate Condominiali (CondominiumInstallment)](#11-rate-condominiali-condominiuminstallment)

---

## 1. Condominio

### Creazione
- Il codice condominio è **obbligatorio**.
- Il codice deve essere **unico** a livello di tenant: non possono esistere due condomini con lo stesso codice.

### Aggiornamento
- Il codice condominio è **obbligatorio**.
- Il nuovo codice deve essere **unico** all'interno del tenant (escluso il condominio stesso).

---

## 2. Unità Immobiliare

### Creazione
- Il numero interno (`InternalNumber`) è **obbligatorio**.
- Il numero interno deve essere **unico** all'interno dello stesso condominio.

---

## 3. Esercizio Fiscale

### Stati e transizioni

```
Draft (1) → Open (2) → Closing (3) → Closed (4) → Locked (5)
```

| Transizione | Endpoint | Condizioni |
|---|---|---|
| Crea (→ Draft) | `POST /api/fiscal-years` | Vedi regole creazione |
| Draft → Open | `POST /fiscal-years/{id}/open` | Stato deve essere Draft; nessun altro esercizio Open o Closing per lo stesso condominio |
| Open → Closing | `POST /fiscal-years/{id}/start-closing` | Stato deve essere Open |
| Closing → Closed | `POST /fiscal-years/{id}/close` | Stato deve essere Open o Closing; deve esistere un budget Consuntivo Approvato o Chiuso; nessuna spesa in stato "PagataParzialmente" |
| Closed → Locked | `POST /fiscal-years/{id}/lock` | Stato deve essere Closed |

### Creazione (→ Draft)
- La `EndDate` deve essere **successiva** alla `StartDate`.
- Se viene specificato un `PreviousFiscalYearId`:
  - L'esercizio precedente deve appartenere **allo stesso condominio**.
  - La `StartDate` del nuovo esercizio deve essere **esattamente il giorno successivo** alla `EndDate` dell'esercizio precedente.
  - L'esercizio precedente **non deve essere già utilizzato** da un altro esercizio come predecessore (ogni esercizio può essere predecessore di al massimo uno successivo).
- Le date **non devono sovrapporsi** con altri esercizi dello stesso condominio.
- Il **codice** deve essere unico per condominio.

### Modifica
- Non è possibile modificare un esercizio in stato **Closed** o **Locked**.
- La `EndDate` può essere modificata solo su esercizi in stato **Draft** o **Open**.
- La nuova `EndDate` deve essere successiva alla `StartDate`.
- La nuova `EndDate` non deve causare **sovrapposizione** con altri esercizi.

### Chiusura (Closing → Closed)
- Deve esistere almeno un budget di tipo **Consuntivo** in stato **Approved** o **Closed** per l'esercizio.
- Non devono esistere spese in stato **PagataParzialmente** legate all'esercizio.

### Eliminazione
- È possibile eliminare **solo** gli esercizi in stato **Draft**.

---

## 4. Budget

### Tipi
| Valore | Tipo |
|---|---|
| 1 | Preventivo (previsionale) |
| 2 | Consuntivo (rendiconto) |

### Stati e workflow

```
Draft (1) → Approved (2) → Closed (3)
           ↑_____________|  (Reopen: solo SuperAdmin)
```

### Creazione (→ Draft)
- Per ogni combinazione **condominio + esercizio fiscale + tipo** può esistere **al massimo un budget**.
  Il secondo tentativo con lo stesso tipo restituisce errore.

### Approvazione (Draft → Approved)
- Il budget deve essere in stato **Draft**.
- Non deve esistere già un budget dello stesso tipo in stato **Approved** o **Closed** per lo stesso condominio + esercizio.
- Il **piano dei conti** del condominio deve avere almeno un conto attivo per ciascuno dei tre tipi: **Entrata**, **Uscita**, **Patrimoniale**.
- **Effetti dell'approvazione:**
  - *Preventivo*: vengono generate automaticamente le **rate condominiali** (`CondominiumInstallment`) e le **quote per unità** (`CondominiumFee`), calcolate proporzionalmente ai millesimi.
  - *Consuntivo*: vengono scritti `QuotaConsuntiva` e `SaldoConguaglio` su `UnitOpeningBalance` per ogni unità. `TotalMovements` e `ClosingBalance` **non vengono toccati** qui: vengono calcolati definitivamente solo alla chiusura dell'esercizio, incorporando sia l'insoluto delle rate sia il conguaglio.

### Chiusura budget (Approved → Closed)
- Il budget deve essere in stato **Approved**.
- Non deve esistere già un budget dello stesso tipo in stato **Closed** per lo stesso condominio + esercizio.

### Riapertura (Approved → Draft)
- Solo il **SuperAdmin** può riaprire un budget approvato.
- Il budget deve essere in stato **Approved** (non Closed).

### Eliminazione
- È possibile eliminare **solo** i budget in stato **Draft**.

### Ricalcolo voci (solo Consuntivo)
- Il ricalcolo automatico delle voci è disponibile **solo per i budget Consuntivi**.
- Non è possibile ricalcolare le voci di un budget in stato **Closed**.

### Generazione rate
- Le rate possono essere generate solo da un budget in stato **Approved** o **Closed**.
- Non è possibile rigenerare le rate di un budget già **Closed**.
- Se le rate per un budget sono già state generate, la rigenrazione viene ignorata (skip).

### Popolamento automatico voci alla creazione
- **Preventivo**: le voci vengono copiate dall'ultimo Consuntivo dell'esercizio precedente (con eventuale percentuale di maggiorazione). Se non esiste un Consuntivo sorgente, vengono create voci vuote per tutti i conti attivi del piano dei conti.
- **Consuntivo**: le voci vengono create aggregando le spese registrate nell'esercizio, raggruppate per conto.

---

## 5. Voci di Budget (BudgetItem)

### Creazione manuale
- È possibile aggiungere voci **solo a budget in stato Draft**.
- Il conto (`AccountId`) deve esistere e non essere cancellato.
- All'aggiunta di una voce per un conto figlio, vengono **create automaticamente** le voci per tutti i conti antenati (parent) se non già presenti nel budget (struttura gerarchica).

---

## 6. Spese (Expense)

### Creazione
- La **descrizione** è obbligatoria.
- Il **tipo spesa** è obbligatorio.
- La **data documento** è obbligatoria.
- La **data di registrazione** è obbligatoria.
- L'**importo lordo** deve essere maggiore di zero.
- L'**importo IVA** non può essere negativo.
- L'**importo IVA** non può essere superiore all'importo lordo.
- La **tabella millesimale** associata deve essere abilitata (`IsEnabled = true`).
- Se viene specificato un `FiscalYearId`, l'esercizio deve esistere; altrimenti viene selezionato automaticamente l'esercizio **non Locked** con `StartDate` più recente del condominio.
- Se nessun esercizio fiscale attivo è disponibile, la creazione è bloccata.
- Le spese possono essere registrate **solo** su esercizi in stato **Open** o **Closing**.
  - Stato **Draft** → errore: l'esercizio non è ancora aperto.
  - Stato **Closed** → errore: l'esercizio è chiuso.
  - Stato **Locked** → errore: l'esercizio è bloccato.

### Aggiornamento
- Stesse validazioni della creazione su: descrizione, tipo spesa, date, importi lordo/IVA, tabella millesimale.

---

## 7. Piano dei Conti (ChartOfAccounts)

### Creazione
- Il **codice conto** è obbligatorio.
- Il **nome conto** è obbligatorio.
- Il **tipo conto** è obbligatorio (Entrata / Uscita / Patrimoniale).
- Il codice deve essere **unico** per condominio.

### Aggiornamento
- Il **codice conto** è obbligatorio.
- Il **nome conto** è obbligatorio.
- Il nuovo codice deve essere **unico** per condominio (escluso il conto stesso).

### Eliminazione
- Non è possibile eliminare un conto che ha **sotto-conti** associati (gerarchia): occorre eliminare prima i figli.
- Se il conto è usato in **voci di budget in stato Draft**, è necessario specificare un **conto sostitutivo**: le voci e le spese Draft vengono riassegnate automaticamente al sostitutivo.
- Il conto sostitutivo non può coincidere con il conto da eliminare.
- Se il conto è usato in **voci di budget approvati o chiusi**, la cancellazione logica è comunque permessa: le FK storiche rimangono valide (i movimenti passati conservano il riferimento al conto cancellato).

### Categorie (ChartOfAccountsCategory)
- Il **nome categoria** è obbligatorio.
- Il nome deve essere **unico** per tenant.
- Non è possibile eliminare una categoria **utilizzata** da uno o più conti.

---

## 8. Tabella Millesimale

### Abilitazione / Disabilitazione
- Ogni tabella millesimale può essere **abilitata o disabilitata** tramite `PATCH /api/millesimal-tables/{id}/enabled`.
- Una tabella **disabilitata** (`IsEnabled = false`):
  - Non è selezionabile nella form di creazione/modifica di una **spesa**.
  - Non è selezionabile come tabella millesimale default nella form del **piano dei conti**.
  - Rimane visibile nell'elenco della gestione tabelle (con badge "Disabilitata").
- In fase di **modifica** di una spesa già esistente, la tabella attualmente associata rimane visibile nel selettore anche se disabilitata (per non perdere il dato in modifica).
- Una tabella disabilitata **non viene usata** dalla generazione automatica delle rate all'approvazione del budget Preventivo.

### Eliminazione
- Non è possibile eliminare la **tabella predefinita** (codice `DEF`).
- Non è possibile eliminare una tabella impostata come **predefinita** in uno o più conti del piano dei conti (disabilitarla invece).
- Non è possibile eliminare una tabella **utilizzata** in una o più spese (disabilitarla invece).

### Creazione spese
- La tabella millesimale specificata in una spesa deve essere **abilitata** (`IsEnabled = true`). Il tentativo di salvare una spesa con una tabella disabilitata restituisce 400 Bad Request.

### Blocco operativo per anomalie millesimali

Le seguenti operazioni vengono **bloccate con errore 400** se la tabella millesimale abilitata del condominio presenta anomalie:

| Operazione | Endpoint |
|---|---|
| Approvazione budget (Preventivo o Consuntivo) | `POST /api/budgets/{id}/approve` |
| Generazione/rigenerazione rate | `POST /api/budgets/{id}/generate-installments` |
| Calcolo conguaglio | `GET /api/fiscal-years/{id}/conguaglio` |

**Anomalie rilevate (nell'ordine):**

1. **Nessuna tabella abilitata** — non esiste alcuna tabella con `IsEnabled = true` per il condominio.
2. **Tabella senza voci** — la tabella abilitata non ha righe `UnitMillesimal` associate.
3. **Totale millesimi incoerente** — la somma delle voci (`Σ UnitMillesimal.Millesimal`) differisce dal `TotalMillesimal` dichiarato della tabella di più di **0,01** (tolleranza di arrotondamento).

### Voci millesimali (UnitMillesimal)
- Per ogni coppia **(unità, tabella)** può esistere **una sola voce**.

---

## 9. Saldo Iniziale Unità e Riporto Morosità (UnitOpeningBalance)

### Struttura del saldo

Per ogni unità immobiliare e ogni esercizio fiscale esiste un record `UnitOpeningBalance` che traccia:

| Campo | Descrizione | Quando viene scritto |
|---|---|---|
| `OpeningBalance` | Saldo riportato dall'esercizio precedente (morosità o credito pregresso) | Apertura nuovo esercizio (Draft → Open) |
| `RateAddebitate` | Σ `CondominiumFee.AmountDue` del Preventivo | Chiusura esercizio (Closing → Closed) |
| `RateIncassate` | Σ `CondominiumFee.AmountPaid` del Preventivo | Chiusura esercizio (Closing → Closed) |
| `QuotaConsuntiva` | Quota reale dell'unità = TotaleSpese × (Millesimi / TotMillesimi) | Approvazione budget Consuntivo |
| `SaldoConguaglio` | `QuotaConsuntiva − RateAddebitate` (+debito / −credito da conguaglio) | Approvazione budget Consuntivo |
| `TotalMovements` | `(RateAddebitate − RateIncassate) + SaldoConguaglio` | Chiusura esercizio (fonte di verità definitiva) |
| `ClosingBalance` | `OpeningBalance + TotalMovements` | Chiusura esercizio |

### Riporto morosità (carry-forward)

All'apertura del nuovo esercizio (transizione Draft → Open), per ogni unità:

```
nuovo.OpeningBalance = precedente.ClosingBalance
nuovo.RateAddebitate = RateIncassate = QuotaConsuntiva = SaldoConguaglio = TotalMovements = ClosingBalance = 0
```

- `OpeningBalance > 0`: il condòmino porta in avanti una **morosità** (ha pagato meno di quanto dovuto).
- `OpeningBalance < 0`: il condòmino porta in avanti un **credito** (ha pagato più di quanto dovuto o ha ricevuto un rimborso da conguaglio).
- `OpeningBalance = 0`: prima situazione neutra o primo esercizio.

### Flusso di calcolo nell'esercizio

```
1. Preventivo approvato    → generate CondominiumFee (RateAddebitate / RateIncassate si aggiornano via pagamenti)
2. Consuntivo approvato    → scritti QuotaConsuntiva e SaldoConguaglio su UnitOpeningBalance
3. Chiusura esercizio      → TotalMovements e ClosingBalance calcolati definitivamente
4. Apertura esercizio succ → ClosingBalance propagato come OpeningBalance (riporto morosità)
```

### Regole di editabilità

- Non è possibile modificare il saldo iniziale se l'esercizio è in stato **Closed** o **Locked**.
- `OpeningBalance` può essere impostato **manualmente solo per il primo esercizio** del condominio (quello senza esercizi precedenti con `EndDate` anteriore).
- Per gli esercizi successivi `OpeningBalance` viene **propagato automaticamente** dal `ClosingBalance` dell'esercizio precedente.
- `RateAddebitate`, `RateIncassate`, `QuotaConsuntiva`, `SaldoConguaglio`, `TotalMovements`, `ClosingBalance` sono **sempre calcolati automaticamente** e non modificabili manualmente.

---

## 10. Saldo Conto (AccountBalance)

- Non è possibile modificare il saldo iniziale di un conto se l'esercizio è in stato **Closed** o **Locked**.
- Il saldo iniziale è modificabile manualmente **solo per il primo esercizio** del condominio.
- Per gli esercizi successivi il saldo iniziale viene **ereditato automaticamente** dal saldo finale dell'esercizio precedente.

---

## 11. Rate Condominiali (CondominiumInstallment)

- Le rate vengono **generate automaticamente** all'approvazione di un budget **Preventivo**.
- La generazione ripartisce il totale del budget tra le unità in proporzione ai **millesimi** della tabella millesimale abilitata del condominio.
- Il residuo di arrotondamento viene assegnato all'**ultima** rata / ultima unità.
- Se le rate sono già state generate per un budget, la rigenerazione viene **ignorata** (idempotente).

---

## Riepilogo lookup di stato

### FiscalYear
| Id | Nome |
|---|---|
| 1 | Draft (Bozza) |
| 2 | Open (Aperto) |
| 3 | Closing (In chiusura) |
| 4 | Closed (Chiuso) |
| 5 | Locked (Bloccato) |

### Budget
| Id | Nome |
|---|---|
| 1 | Draft (Bozza) |
| 2 | Approved (Approvato) |
| 3 | Closed (Chiuso) |

### BudgetType
| Valore | Nome |
|---|---|
| 1 | Preventivo |
| 2 | Consuntivo |

### ChargeabilityType
| Valore | Nome |
|---|---|
| 0 | Owner (Proprietario) |
| 1 | Tenant (Inquilino) |
| 2 | Auto |
