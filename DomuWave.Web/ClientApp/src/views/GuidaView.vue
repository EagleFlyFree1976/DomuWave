<template>
  <div class="guida-view">
    <!-- ── Hero ── -->
    <header class="guida-hero">
      <div>
        <h1>Guida alle funzionalità</h1>
        <p class="text-secondary">
          Manuale d'uso dei moduli operativi di VizaDomus. Usa l'indice per spostarti
          rapidamente tra le sezioni.
        </p>
      </div>
      <i class="pi pi-book guida-hero-icon"></i>
    </header>

    <div class="guida-layout">
      <!-- ── Indice ── -->
      <nav class="guida-toc card">
        <span class="guida-toc-title">Indice</span>
        <ul>
          <li v-for="s in sections" :key="s.id">
            <a :href="`#${s.id}`"
               class="guida-toc-link"
               :class="{ active: activeId === s.id }"
               @click.prevent="goTo(s.id)">
              <i class="pi" :class="s.icon"></i>
              <span>{{ s.title }}</span>
            </a>
          </li>
        </ul>
      </nav>

      <!-- ── Contenuto ── -->
      <main class="guida-content">
        <section v-for="s in sections" :key="s.id" :id="s.id" class="card guida-section">
          <h2><i class="pi" :class="s.icon"></i> {{ s.title }}</h2>

          <template v-for="(block, i) in s.blocks" :key="i">
            <h3 v-if="block.sub" class="guida-sub">{{ block.sub }}</h3>
            <p v-if="block.text" class="guida-text" v-html="block.text"></p>
            <ul v-if="block.items" class="guida-list">
              <li v-for="(it, j) in block.items" :key="j" v-html="it"></li>
            </ul>
            <ol v-if="block.steps" class="guida-steps">
              <li v-for="(st, j) in block.steps" :key="j" v-html="st"></li>
            </ol>
            <div v-if="block.example" class="guida-example">
              <span class="guida-example-tag"><i class="pi pi-lightbulb"></i> Esempio</span>
              <p v-html="block.example"></p>
            </div>
            <div v-if="block.tip" class="guida-tip">
              <i class="pi pi-info-circle"></i>
              <p v-html="block.tip"></p>
            </div>
          </template>
        </section>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'

// ── Contenuto guida (escluse le funzionalità riservate ai SuperAdmin) ─────────
const sections = [
  {
    id: 'accesso', title: 'Accesso e sicurezza', icon: 'pi-lock',
    blocks: [
      { text: 'Per usare la piattaforma devi autenticarti e, se gestisci più condomini, selezionare quello su cui vuoi lavorare. Quasi tutte le sezioni operative mostrano dati solo dopo aver scelto un condominio.' },
      { sub: 'Login', steps: [
        'Apri la pagina di login e inserisci username/email e password.',
        'Premi <strong>Accedi</strong>: verrai portato alla Dashboard.',
        'Se la sessione è ancora attiva da un accesso precedente, l\'app salta il login e apre direttamente la Dashboard.',
      ] },
      { sub: 'Recupero password', steps: [
        'Nella pagina di login clicca su <strong>Password dimenticata</strong>.',
        'Inserisci l\'email associata al tuo account e invia la richiesta.',
        'Apri l\'email ricevuta e segui il link per impostare una nuova password.',
      ] },
      { sub: 'Selezione del condominio', text:
        'Il selettore del condominio si trova in basso nella barra laterale. Cambiando condominio, tutte le sezioni (unità, budget, spese, rate…) si aggiornano automaticamente sul condominio scelto.' },
      { tip: 'Se una sezione appare vuota con il messaggio "Seleziona un condominio", apri il selettore in basso a sinistra e scegline uno: i dati compariranno subito.' },
    ],
  },
  {
    id: 'dashboard', title: 'Dashboard', icon: 'pi-home',
    blocks: [
      { text: 'La Dashboard è la pagina iniziale dopo il login. Offre una visione d\'insieme e scorciatoie verso i moduli più usati.' },
      { items: [
        'Riepilogo dei dati principali del condominio selezionato.',
        'Accessi rapidi alle funzioni di uso frequente (spese, rate, comunicazioni).',
        'Indicatori utili per controllare lo stato generale della gestione.',
      ] },
      { tip: 'Usa la Dashboard come punto di partenza quotidiano: da qui controlli a colpo d\'occhio scadenze e attività in sospeso prima di entrare nei singoli moduli.' },
    ],
  },
  {
    id: 'condomini', title: 'Condomini e unità', icon: 'pi-building',
    blocks: [
      { sub: 'Elenco condomini', text:
        'Mostra tutti i condomini a cui hai accesso. Da qui selezioni un condominio ed entri nel suo dettaglio, con tutte le sotto-sezioni (edifici, unità, panoramica occupanti, budget, ecc.).' },
      { sub: 'Dettaglio condominio', items: [
        'Dati anagrafici: denominazione, codice fiscale, indirizzo.',
        'Dati tecnici: edifici, scale, numero di piani, presenza ascensore.',
        'Dati amministrativi: recapiti, parametri gestionali.',
      ] },
      { sub: 'Saldo iniziale di cassa', text:
        'Nei dati amministrativi del condominio puoi impostare il <strong>Saldo iniziale di cassa</strong>: le disponibilità liquide con cui il condominio parte. Lo trovi anche nella pagina <strong>Rendiconto</strong>, sopra i prospetti. Confluisce come avanzo/disponibilità iniziale nei <strong>Flussi di cassa</strong> e nella <strong>Situazione patrimoniale</strong>, ma <strong>solo per il primo esercizio</strong> del condominio: per gli esercizi successivi la liquidità di apertura deriva automaticamente dalla chiusura dell\'esercizio precedente.' },
      { sub: 'Unità immobiliari', text:
        'Ogni unità rappresenta un appartamento, un box, un negozio, ecc. I campi tipici sono scala, piano, interno, destinazione d\'uso e metri quadri. Il <strong>piano</strong> è particolarmente importante: viene usato per la ripartizione delle spese con criterio misto (vedi “Piano dei conti”).' },
      { example: 'Un negozio al piano terra si inserisce con <span class="mono">Piano = 0</span>. In questo modo non parteciperà alla quota “per altezza” delle spese ascensore, ma resterà soggetto alla quota “per valore”.' },
      { sub: 'Panoramica occupanti', text:
        'Riepiloga per ogni unità chi è il proprietario e chi l\'eventuale inquilino, così puoi verificare rapidamente che tutte le unità abbiano un occupante collegato prima di generare rate e comunicazioni.' },
    ],
  },
  {
    id: 'esercizi', title: 'Esercizi fiscali', icon: 'pi-calendar',
    blocks: [
      { text: 'L\'esercizio fiscale è il periodo contabile (di norma un anno) a cui si riferiscono budget, spese e rate. Ogni movimento contabile appartiene a un esercizio.' },
      { sub: 'Creare un esercizio', steps: [
        'Apri <strong>Esercizi fiscali</strong> e clicca <strong>+ Nuovo esercizio</strong>.',
        'Imposta codice (es. l\'anno), descrizione, data di inizio e di fine.',
        'Salva: l\'esercizio nasce in stato <strong>Bozza</strong>.',
      ] },
      { sub: 'Stati e ciclo di vita', items: [
        '<strong>Bozza</strong>: appena creato, ancora modificabile ed eliminabile. È il momento per impostare i saldi iniziali delle unità.',
        '<strong>Aperto</strong>: si registrano spese, rate e incassi. <em>Non possono esistere due esercizi aperti per lo stesso condominio.</em>',
        '<strong>In chiusura</strong>: si possono ancora registrare movimenti, ma non creare nuovi esercizi sovrapposti.',
        '<strong>Chiuso</strong> / <strong>Bloccato</strong>: l\'esercizio è consolidato e protetto da modifiche.',
      ] },
      { example: 'Tipico passaggio di anno: porti l\'esercizio 2025 da “Aperto” a “In chiusura”, crei e apri l\'esercizio 2026, poi chiudi definitivamente il 2025 quando il consuntivo è approvato.' },
      { tip: 'I saldi iniziali delle unità si impostano quando l\'esercizio è in Bozza, dalla riga dell\'esercizio. Sono il punto di partenza per il calcolo di crediti/debiti dei condòmini.' },
    ],
  },
  {
    id: 'piano-conti', title: 'Piano dei conti', icon: 'pi-list',
    blocks: [
      { text: 'Il piano dei conti è l\'elenco strutturato delle voci contabili (entrate e uscite). Ogni spesa viene imputata a un conto, che ne determina la classificazione e — soprattutto — il criterio con cui viene ripartita tra le unità.' },
      { sub: 'Struttura e categorie', items: [
        'Definisci conti con codice, descrizione e gerarchia coerente per la reportistica.',
        'Associa i conti alle categorie (e sottocategorie) per raggrupparli in modo chiaro nei report.',
      ] },
      { sub: 'Criterio di ripartizione del conto', text:
        'Su ogni conto scegli <strong>come</strong> le sue spese si distribuiscono tra le unità. Sono disponibili due metodi:' },
      { items: [
        '<strong>Standard</strong> (predefinito): la spesa è ripartita al 100% secondo la tabella millesimale associata — cioè <em>per valore</em> (millesimi di proprietà).',
        '<strong>Misto</strong> (valore/altezza, art. 1124 c.c. per scale e ascensori): la spesa è divisa in due quote — una <em>per valore</em>, pari alla percentuale millesimale impostata sul conto e ripartita secondo la tabella millesimale; l\'altra <em>per altezza/uso</em>, ripartita in proporzione al fattore <span class="mono">Peso&nbsp;Piano&nbsp;×&nbsp;piano&nbsp;+&nbsp;Peso&nbsp;Abitanti&nbsp;×&nbsp;abitanti</span>.',
      ] },
      { text:
        'Con il criterio Misto il <strong>piano terra</strong> (piano 0) non paga la quota per altezza — il suo fattore altezza vale zero — ma contribuisce comunque alla quota per valore, come richiesto dalla giurisprudenza costante sull\'art. 1124.' },
      { example:
        'Conto “Ascensore – manutenzione” con criterio <strong>Misto</strong>: percentuale millesimale = <span class="mono">50</span>, Peso Piano = <span class="mono">1</span>, Peso Abitanti = <span class="mono">0</span>. Risultato: metà spesa ripartita per millesimi, metà in proporzione al piano. Un appartamento al 3° piano paga la quota altezza piena; il negozio al piano terra paga solo la metà “per valore”.' },
      { tip: 'Per un criterio 1124 “puro” (metà valore / metà altezza) imposta Peso Abitanti = 0. Se invece vuoi pesare anche il numero di occupanti (es. spese di pulizia scale), valorizza Peso Abitanti.' },
    ],
  },
  {
    id: 'millesimali', title: 'Tabelle millesimali', icon: 'pi-percentage',
    blocks: [
      { text: 'Le tabelle millesimali esprimono, per ogni unità, la quota di partecipazione alle spese. Un condominio ne ha in genere diverse: una generale e altre specifiche (scale, ascensore, riscaldamento…).' },
      { sub: 'Creare e compilare una tabella', steps: [
        'Crea la tabella dando un nome chiaro (es. “Generale”, “Scala A”, “Ascensore”).',
        'Inserisci il valore in millesimi per ogni unità.',
        'Controlla la somma: per una tabella generale dovrebbe essere 1000.',
      ] },
      { sub: 'Attivazione', text:
        'Solo le tabelle attive vengono proposte nelle spese e usate per le ripartizioni. Abilita le tabelle che devono entrare in uso e lascia disattivate le bozze.' },
      { example: 'Per le spese dell\'ascensore crei una tabella “Ascensore” con i millesimi specifici e la colleghi al conto “Ascensore”. Le bollette dell\'energia dell\'ascensore useranno quella tabella, non la generale.' },
      { tip: 'Se la somma dei millesimi non torna, la ripartizione risulterà sbilanciata: verifica e correggi i valori prima di registrare spese su quella tabella.' },
    ],
  },
  {
    id: 'budget', title: 'Budget (preventivo e consuntivo)', icon: 'pi-wallet',
    blocks: [
      { text: 'Il budget pianifica e rendiconta le spese dell\'esercizio. Esistono due tipi: il <strong>Preventivo</strong>, che stima le spese future e genera le rate; il <strong>Consuntivo</strong>, che chiude i conti a fine periodo confrontando previsto e speso.' },
      { sub: 'Creare un budget', steps: [
        'Seleziona l\'esercizio fiscale dalla tendina in alto.',
        'Clicca <strong>+ Nuovo budget</strong> e scegli il tipo (Preventivo o Consuntivo).',
        'Apri <strong>Voci</strong> per inserire le voci di spesa previste e controllare il totale.',
      ] },
      { sub: 'Approvazione del Preventivo', steps: [
        '<strong>Pre-approva</strong>: il preventivo passa da “Bozza” a “In approvazione” e non è più modificabile (puoi comunque riportarlo in Bozza).',
        'Collega o crea l\'<strong>assemblea</strong> ordinaria per la delibera.',
        '<strong>Approva definitivamente</strong>: il budget diventa “Approvato” e da quel momento vengono generate le rate.',
      ] },
      { sub: 'Approvazione del Consuntivo', text:
        'Il consuntivo si approva direttamente dalla Bozza. Una volta approvato, alimenta il rendiconto e il calcolo dei conguagli.' },
      { sub: 'Stati del budget', items: [
        '<strong>Bozza</strong>: in lavorazione, modificabile.',
        '<strong>In approvazione</strong>: solo per il preventivo, in attesa di delibera assembleare.',
        '<strong>Approvato</strong>: definitivo; per il preventivo genera le rate.',
        '<strong>Chiuso</strong>: archiviato a fine periodo.',
      ] },
      { example: 'Flusso completo di un preventivo: crei il budget → inserisci le voci → Pre-approva → crei l\'assemblea → dopo la delibera “Approva definitivamente” → l\'app genera automaticamente le rate per i condòmini.' },
      { tip: 'Non può esistere più di un preventivo (o consuntivo) approvato per lo stesso esercizio: se un budget dello stesso tipo è già approvato, gli altri risultano “Non approvabili”.' },
    ],
  },
  {
    id: 'movimenti', title: 'Movimenti (spese, entrate, patrimoniali)', icon: 'pi-euro',
    blocks: [
      { text: 'La sezione <strong>Movimenti</strong> raccoglie tutte le registrazioni contabili dell\'esercizio. Ogni movimento è di un tipo: <strong>Spesa</strong> (uscita), <strong>Entrata</strong> o <strong>Patrimoniale</strong>.' },
      { sub: 'Registrare un movimento', steps: [
        'Clicca <strong>+ Inserisci spesa</strong> (oppure entrata / patrimoniale).',
        'Compila data documento, fornitore, importi (imponibile, IVA, eventuale ritenuta) e il conto.',
        'Per le spese da ripartire, seleziona la <strong>tabella millesimale</strong> corretta.',
        'Salva: la spesa viene registrata e ripartita automaticamente tra le unità secondo il criterio del conto.',
      ] },
      { sub: 'Tipologie di spesa', text:
        'Ogni spesa ha una tipologia che aiuta a filtrarla e a costruire i report: Manutenzione, Pulizie, Sicurezza, Utenze, Professionale, Altro.' },
      { sub: 'Pagamento e filtri', items: [
        'Registra il pagamento di una spesa per portarla da <strong>Non evasa</strong> a <strong>Evasa</strong>.',
        'Filtra l\'elenco per esercizio, tipo di movimento, tipologia di spesa o stato di pagamento.',
        'Usa la ricerca per descrizione o numero documento e ordina per data o importo.',
      ] },
      { example: 'Registri la fattura dell\'idraulico (Spesa, tipo Manutenzione) sul conto “Manutenzione ordinaria” con la tabella “Generale”: l\'importo viene subito suddiviso per millesimi su tutte le unità. Quando paghi, registri il pagamento e la spesa diventa “Evasa”.' },
      { tip: 'Imputando una spesa a una <strong>singola unità</strong> (anziché a una tabella millesimale), l\'intero importo viene addebitato solo a quell\'unità — utile per costi a carico di un solo condòmino.' },
    ],
  },
  {
    id: 'rate', title: 'Rate e quote', icon: 'pi-credit-card',
    blocks: [
      { text: 'Le rate sono le richieste di pagamento inviate ai condòmini. Le quote sono gli importi dovuti da ciascuna unità per ogni rata.' },
      { sub: 'Generazione delle rate', text:
        'Le rate del preventivo vengono generate automaticamente quando il budget preventivo viene approvato. Puoi comunque aggiungere rate manuali con <strong>+ Nuova rata</strong> quando serve.' },
      { sub: 'Incassi e riconciliazione', text:
        'Quando un condòmino paga, registri l\'incasso sulla sua quota: lo stato passa da non saldato a saldato e il saldo si aggiorna. Per i bonifici puoi usare la <strong>riconciliazione per causale</strong>.' },
      { sub: 'Riconciliazione di un bonifico', steps: [
        'Incolla la causale del bonifico nel box <strong>Riconciliazione bonifico</strong> e premi <strong>Cerca</strong>.',
        'L\'app individua le quote collegate a quel codice e mostra dovuto, pagato e saldo.',
        'Usa <strong>Salda tutto</strong> per registrare in un colpo solo il pagamento di tutte le quote trovate.',
      ] },
      { sub: 'Comunicazione ai condòmini', text:
        'Con <strong>📧 Notifica condòmini</strong> invii ai condòmini il promemoria delle rate e degli importi dovuti.' },
      { sub: 'Pagamento online (Stripe)', text:
        'Se l\'amministratore collega un account Stripe al condominio (menu <strong>Amministrazione → Pagamenti Online</strong>), il condòmino vede il pulsante <strong>💳 Paga online</strong> sulle proprie quote e può pagare con carta. L\'incasso arriva direttamente sul conto del condominio; la quota viene saldata automaticamente alla conferma del pagamento.' },
      { example: 'Un condòmino paga 3 rate con un unico bonifico riportando in causale il codice quota. Incolli la causale, l\'app trova le 3 quote, clicchi “Salda tutto” e tutte risultano pagate.' },
    ],
  },
  {
    id: 'fornitori', title: 'Fornitori e contratti', icon: 'pi-truck',
    blocks: [
      { sub: 'Anagrafica fornitori', items: [
        'Inserisci ragione sociale, P.IVA/codice fiscale e contatti.',
        'Tieni aggiornati stato e tipologia del fornitore.',
      ] },
      { sub: 'Contratti', text:
        'A ogni fornitore puoi associare i contratti, con riferimenti e scadenze, così da avere sotto controllo manutenzioni e rinnovi.' },
      { example: 'Inserisci la ditta dell\'ascensore come fornitore e registri il contratto di manutenzione annuale con la sua scadenza: quando registri le fatture, le colleghi a questo fornitore.' },
    ],
  },
  {
    id: 'fatture-elettroniche', title: 'Fatture elettroniche', icon: 'pi-file-import',
    blocks: [
      { text: 'Questo modulo scarica automaticamente le <strong>fatture passive</strong> del condominio dal Cassetto Fiscale (Sistema di Interscambio), evitando l\'inserimento manuale. DomuWave non accede direttamente al portale dell\'Agenzia delle Entrate: si appoggia a un <strong>provider accreditato</strong> (intermediario SdI) tramite la sua API. Lo trovi nel menu <strong>Amministrazione → Fatture Elettroniche</strong>.' },
      { sub: 'Prerequisito: delega all\'intermediario', text:
        'Una tantum, l\'amministratore deve delegare il provider scelto come intermediario sul portale <strong>Fatture e Corrispettivi</strong> dell\'Agenzia delle Entrate, così che possa ricevere le fatture per conto del condominio. È un passaggio che si fa fuori da DomuWave.' },
      { sub: 'Configurare il provider', steps: [
        'Apri <strong>Fatture Elettroniche</strong> con il condominio selezionato.',
        'Nel riquadro <strong>Configurazione provider</strong> scegli il provider SdI (Acube, Aruba o Fatture in Cloud).',
        'Inserisci la <strong>chiave API</strong> fornita dal provider.',
        'La <strong>Partita IVA di ricezione</strong> usa automaticamente quella dell\'anagrafica del condominio; compila il campo solo se serve un <strong>override</strong> (P.IVA diversa da quella anagrafica).',
        'Premi <strong>Salva configurazione</strong>: la chiave viene memorizzata in forma cifrata e non è più visibile.',
      ] },
      { sub: 'Scaricare le fatture', steps: [
        'Imposta l\'intervallo di date <strong>Dal / Al</strong>.',
        'Premi <strong>Scarica fatture</strong>: il sistema interroga il provider e importa le fatture ricevute nel periodo.',
        'Le fatture già presenti non vengono duplicate; per ognuna il sistema cerca di riconoscere il <strong>fornitore</strong> in base alla partita IVA.',
      ] },
      { sub: 'Stato delle fatture', items: [
        '<strong>Nuova</strong>: scaricata, non ancora collegata a una spesa.',
        '<strong>Collegata</strong>: associata a una spesa registrata in contabilità.',
        '<strong>Ignorata</strong>: scartata manualmente perché non rilevante.',
      ] },
      { tip: 'Il pulsante “Scarica fatture” resta disattivato finché provider e chiave API non sono configurati e il condominio non ha una partita IVA (dell\'anagrafica o impostata come override). Se è grigio, completa prima il riquadro di configurazione.' },
      { tip: 'La chiave API è un dato sensibile: viene salvata cifrata e non viene mai più mostrata. Per cambiarla basta inserirne una nuova e salvare; lasciando il campo vuoto, quella esistente resta invariata.' },
    ],
  },
  {
    id: 'documenti', title: 'Documenti', icon: 'pi-folder',
    blocks: [
      { sub: 'Archivio', text:
        'Carica e conserva i documenti del condominio: regolamento, verbali di assemblea, polizze, documentazione tecnica, fatture.' },
      { sub: 'Consultazione', items: [
        'Filtra i documenti per trovare rapidamente quello che cerchi.',
        'Scarica i file quando necessario.',
      ] },
      { tip: 'Mantieni una nomenclatura coerente dei file (es. “2026 – Verbale assemblea ordinaria”) per ritrovarli facilmente negli anni.' },
    ],
  },
  {
    id: 'comunicazioni', title: 'Comunicazioni', icon: 'pi-megaphone',
    blocks: [
      { sub: 'Creare una comunicazione', steps: [
        'Inserisci titolo, testo e il periodo di visibilità (data di pubblicazione e di scadenza).',
        'Se vuoi, abilita l\'invio via email ai condòmini.',
        'Pubblica: la comunicazione comparirà nella bacheca nel periodo indicato.',
      ] },
      { example: 'Pubblichi l\'avviso di interruzione dell\'acqua per lavori: imposti la data di inizio e fine visibilità e attivi l\'invio via email per avvisare subito tutti.' },
      { sub: 'Sondaggi in bacheca', text:
        'Dalla <strong>Bacheca</strong> (Bacheca & Messaggi) puoi creare un <strong>sondaggio</strong>: spunta “Crea un sondaggio”, inserisci almeno due opzioni e scegli se consentire la <strong>scelta multipla</strong>, se renderlo <strong>anonimo</strong> (si vedono solo i conteggi, non chi ha votato) e un\'eventuale <strong>scadenza</strong>. I condòmini votano dalla bacheca; i risultati diventano visibili dopo aver votato. È una consultazione informale e non sostituisce una delibera assembleare.' },
    ],
  },
  {
    id: 'checklist', title: 'Messa in esercizio (checklist)', icon: 'pi-check-square',
    blocks: [
      { text: 'Segui questo ordine per avviare correttamente la gestione di un nuovo condominio:' },
      { steps: [
        'Completa i dati anagrafici e l\'indirizzo del condominio.',
        'Inserisci le unità immobiliari e collega proprietari/inquilini.',
        'Crea, compila e attiva le tabelle millesimali (almeno la generale).',
        'Completa il piano dei conti e le categorie, impostando il criterio di ripartizione dei conti.',
        'Crea e apri l\'esercizio fiscale corrente, impostando i saldi iniziali.',
        'Inserisci i fornitori principali.',
        'Crea e approva il budget preventivo.',
        'Verifica le rate generate.',
        'Carica i documenti iniziali e invia le prime comunicazioni.',
      ] },
    ],
  },
  {
    id: 'buone-pratiche', title: 'Buone pratiche', icon: 'pi-star',
    blocks: [
      { items: [
        'Mantieni anagrafiche (unità, occupanti, fornitori) sempre aggiornate.',
        'Usa categorie e conti coerenti: rendono leggibili report e rendiconti.',
        'Verifica periodicamente che la somma dei millesimi sia corretta e che le rate quadrino.',
        'Imposta il criterio di ripartizione corretto sui conti “ascensore” e “scale” prima di registrare le relative spese.',
        'Archivia con ordine documenti e comunicazioni.',
      ] },
    ],
  },
  {
    id: 'impostazioni', title: 'Impostazioni', icon: 'pi-cog',
    blocks: [
      { text:
        'La pagina <strong>Impostazioni</strong> raccoglie le preferenze valide per tutto lo studio (tenant): aspetto e formato dei valori contabili.' },
      { sub: 'Logo' },
      { text:
        'Carica il <strong>logo</strong> dello studio: comparirà nella barra laterale e in cima a tutti i <strong>report esportati in PDF</strong> (rendiconto, bilancio di ripartizione, consuntivo) e negli <strong>avvisi di pagamento</strong> e nelle <strong>comunicazioni</strong> generati dal sistema.' },
      { text:
        'Formati ammessi: PNG, JPG, WebP, SVG · dimensione massima 512 KB. Puoi sostituirlo o rimuoverlo in qualsiasi momento.' },
      { tip: 'I report <strong>Excel</strong> riportano l\'intestazione con condominio ed esercizio ma non l\'immagine del logo: nei fogli di calcolo il logo grafico è presente solo nella versione PDF.' },
      { sub: 'Convenzione segno valori' },
      { text:
        'Scegli come mostrare spese, incassi e saldi: <strong>Solo colore</strong> (uscite in rosso senza segno) oppure <strong>Segno esplicito</strong> (uscite con −, entrate con +).' },
    ],
  },
  {
    id: 'supporto', title: 'Supporto', icon: 'pi-question-circle',
    blocks: [
      { text:
        'Per supporto tecnico o richieste di configurazione avanzata, contatta l\'amministratore di sistema o il team IT.' },
    ],
  },
]

// ── Navigazione / scroll-spy ──────────────────────────────────────────────────
const activeId = ref(sections[0].id)
let observer = null

function goTo(id) {
  const el = document.getElementById(id)
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'start' })
    activeId.value = id
  }
}

onMounted(() => {
  observer = new IntersectionObserver(
    (entries) => {
      const visible = entries
        .filter(e => e.isIntersecting)
        .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)
      if (visible.length) activeId.value = visible[0].target.id
    },
    { rootMargin: '-20% 0px -70% 0px', threshold: 0 },
  )
  sections.forEach(s => {
    const el = document.getElementById(s.id)
    if (el) observer.observe(el)
  })
})

onBeforeUnmount(() => observer?.disconnect())
</script>

<style scoped>
.guida-view {
  max-width: 1100px;
  margin: 0 auto;
  padding: 1.5rem;
}

/* Hero */
.guida-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}
.guida-hero h1 {
  margin: 0 0 0.35rem;
  font-size: 1.6rem;
}
.guida-hero p { margin: 0; max-width: 640px; }
.guida-hero-icon {
  font-size: 2.5rem;
  color: var(--accent);
  opacity: 0.7;
  flex-shrink: 0;
}

/* Layout indice + contenuto */
.guida-layout {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 1.5rem;
  align-items: start;
}

/* Indice */
.guida-toc {
  position: sticky;
  top: 1rem;
  padding: 1rem;
}
.guida-toc-title {
  display: block;
  font-size: 0.72rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
  margin-bottom: 0.6rem;
}
.guida-toc ul { list-style: none; margin: 0; padding: 0; }
.guida-toc-link {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  padding: 0.45rem 0.6rem;
  border-radius: 7px;
  color: var(--text-secondary);
  text-decoration: none;
  font-size: 0.85rem;
  cursor: pointer;
  transition: background 0.12s, color 0.12s;
}
.guida-toc-link:hover { background: var(--accent-glow); color: var(--text); }
.guida-toc-link.active {
  background: var(--accent-glow);
  color: var(--accent);
  font-weight: 600;
}
.guida-toc-link .pi { font-size: 0.85rem; width: 16px; text-align: center; }

/* Contenuto */
.guida-content { display: flex; flex-direction: column; gap: 1.25rem; }
.guida-section { padding: 1.5rem; scroll-margin-top: 1rem; }
.guida-section h2 {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin: 0 0 1rem;
  font-size: 1.2rem;
}
.guida-section h2 .pi { color: var(--accent); font-size: 1.1rem; }
.guida-sub {
  margin: 1.1rem 0 0.4rem;
  font-size: 0.95rem;
  color: var(--text);
}
.guida-text { margin: 0.4rem 0; color: var(--text-secondary); line-height: 1.55; }
.guida-list {
  margin: 0.3rem 0 0.4rem;
  padding-left: 1.2rem;
  color: var(--text-secondary);
  line-height: 1.6;
}
.guida-list li { margin-bottom: 0.25rem; }

/* Passi numerati */
.guida-steps {
  margin: 0.4rem 0 0.5rem;
  padding-left: 1.3rem;
  color: var(--text-secondary);
  line-height: 1.6;
}
.guida-steps li { margin-bottom: 0.4rem; padding-left: 0.2rem; }

/* Box esempio */
.guida-example {
  margin: 0.7rem 0;
  padding: 0.8rem 1rem;
  border-radius: 8px;
  background: var(--accent-glow);
  border: 1px solid var(--border);
  border-left: 3px solid var(--accent);
}
.guida-example-tag {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--accent);
  margin-bottom: 0.35rem;
}
.guida-example p { margin: 0; color: var(--text); line-height: 1.55; font-size: 0.9rem; }

/* Box nota / tip */
.guida-tip {
  display: flex;
  gap: 0.6rem;
  margin: 0.7rem 0;
  padding: 0.7rem 0.9rem;
  border-radius: 8px;
  background: var(--bg-base);
  border: 1px dashed var(--border-active);
}
.guida-tip .pi { color: var(--accent); margin-top: 0.15rem; flex-shrink: 0; }
.guida-tip p { margin: 0; color: var(--text-secondary); line-height: 1.55; font-size: 0.88rem; }

@media (max-width: 860px) {
  .guida-layout { grid-template-columns: 1fr; }
  .guida-toc { position: static; }
}
</style>
