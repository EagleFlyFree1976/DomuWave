# DomuWave — Riepilogo delle modifiche

**Data:** 2 giugno 2026
**A cura di:** Team di sviluppo

Di seguito l'elenco degli interventi e delle nuove funzionalità rilasciate nelle ultime giornate di lavoro.

---

## 1. Esercizi fiscali

- **Nuovo box "Saldo iniziale"** nel dettaglio dell'esercizio fiscale: i riquadri di riepilogo ora mostrano anche il saldo iniziale (o "Saldo anno precedente" se l'esercizio è collegato a quello precedente).
- **Riga dei totali** nella tabella "Saldi iniziali per unità": in fondo alla tabella viene ora mostrato il totale di ciascuna colonna (saldo iniziale, rate, conguaglio, saldo finale).

---

## 2. Visualizzazione importi contabili (personalizzabile)

- **Nuova pagina "Impostazioni visualizzazione"** (sezione Amministrazione): è possibile scegliere come mostrare i valori contabili (spese, incassi, saldi).
- Due modalità disponibili:
  - **Solo colore** — le uscite in rosso senza segno (comportamento storico);
  - **Segno esplicito** — le uscite con il segno meno e le entrate con il segno più.
- L'impostazione è **per singolo condominio/cliente** e viene applicata, ad esempio, ai riquadri del dettaglio esercizio fiscale. È presente un'anteprima in tempo reale durante la scelta.

---

## 3. Anagrafica condòmini

- **Aggiunto il numero di telefono** all'anagrafica del proprietario (in precedenza era presente solo per l'inquilino). Il campo è disponibile nella scheda di creazione/modifica degli occupanti.

---

## 4. Gestione delle spese

- **Nuovo flag "Invia 770"** sulla spesa: consente di indicare se la spesa deve essere inclusa nel modello 770.
- **Date documento e registrazione ora opzionali**: non è più obbligatorio compilarle al momento dell'inserimento della spesa.
- **Imputazione della spesa a un singolo immobile**: in alternativa alla ripartizione per tabella millesimale, è ora possibile assegnare una spesa interamente a un determinato immobile. La scelta avviene tramite un selettore "Tabella millesimale / Immobile specifico".
- **Riorganizzazione della scheda spesa**: migliorata la disposizione dei campi "A carico di" e "Invia 770" per una lettura più chiara.

---

## 5. Gestione consumi (es. acqua) — revisione del calcolo

È stato rivisto il meccanismo di ripartizione delle spese a consumo per allinearlo al funzionamento reale:

- **Importo ripartito automatico**: l'importo da ripartire non si inserisce più a mano. Viene calcolato automaticamente come **somma delle bollette** registrate durante l'anno sul conto associato a quel tipo di consumo.
- **Controllo di completezza**: la ripartizione viene eseguita solo se **tutte le unità dotate di contatore attivo** hanno una lettura registrata. In caso contrario il sistema segnala l'elenco delle unità mancanti.
- **Date delle letture precompilate**: all'inserimento delle letture, i campi data inizio e data fine vengono proposti automaticamente con il periodo dell'esercizio fiscale selezionato.
- **Ripartizione corretta nel consuntivo**: le spese di una categoria assegnata a un consumo **non** vengono più ripartite per millesimi, ma in base ai consumi effettivi. Questo si riflette correttamente nel rendiconto/consuntivo e nel conguaglio per unità.
- **Approvazione consumi semplificata**: l'approvazione della ripartizione consumi salva le quote per unità che confluiscono nel consuntivo; l'addebito vero e proprio resta legato all'approvazione del bilancio consuntivo.

---

## 6. Nuovi report

- **Report "Spese per tabella millesimale"** (sezione Contabilità): elenco delle spese di un esercizio fiscale raggruppate per tabella millesimale, con totale per gruppo e totale complessivo. Le spese imputate a un singolo immobile sono raggruppate in una sezione dedicata. Il report è **stampabile / esportabile in PDF**.
- **Report "Bilancio di ripartizione"** (sezione Contabilità): prospetto di sintesi per esercizio fiscale con una riga per i proprietari e, se presenti, una per gli inquilini di ogni unità. Le colonne sono dinamiche: una per ciascun **tipo di consumo** utilizzato (con quantità e importo) e una per ciascuna **tabella millesimale** (con millesimi e importo ripartito), più le **spese dirette** imputate al singolo immobile, gli **accrediti**, il **totale a carico**, il **versato** (quote condominiali incassate) e il **saldo** per unità. In fondo è presente la **riga dei totali** di colonna.

---

## 7. Altre migliorie

- **Tabella millesimale predefinita**: la tabella creata automaticamente alla creazione di un nuovo condominio si chiama ora **"Generale"** (in precedenza "Default").

---

### Note

- Alcune funzionalità richiedono il completamento dei dati (es. lettura dei contatori, registrazione delle bollette) per produrre i risultati attesi.
- Per qualsiasi chiarimento o per segnalare un comportamento inatteso, restiamo a disposizione.
