---
title: "Consumi e contatori"
slug: "consumi"
section: "Contabilità"
order: 64
---

Il modulo **Consumi** gestisce contatori e letture per ripartire le spese in base ai consumi effettivi (acqua, gas, energia, ecc.) invece che ai soli millesimi. È organizzato in quattro tab.

## 1. Tipi di consumo
Definisci cosa misuri. Per ogni tipo indichi **Nome**, **Unità di misura** (es. `mc`, `kWh`) e il **conto spesa** collegato (un conto di tipo Uscita). Esempio: *Acqua* in `mc`.

## 2. Contatori
Associa un contatore a un'unità: scegli **Tipo di consumo**, **Unità immobiliare** e inserisci la **matricola** del contatore.

## 3. Letture
1. Seleziona **Tipo di consumo** ed **Esercizio fiscale**: l'app mostra una riga per contatore.
2. Inserisci **lettura iniziale/finale** e le relative **date**. Il **consumo** è calcolato in automatico.
3. Salva la singola lettura (✓) oppure usa **Salva tutte** per il salvataggio in blocco.

L'app controlla in tempo reale la coerenza: la data iniziale deve precedere la finale, la lettura finale non può essere inferiore all'iniziale e c'è continuità con la lettura precedente. Una lettura già usata in una **ripartizione approvata** è in sola lettura.

## 4. Ripartizioni
1. Seleziona l'**esercizio fiscale**: vedi una scheda per ogni ripartizione.
2. In bozza puoi **Ricalcola** (dalle letture), **Modifica importi** manualmente (la somma deve coincidere con il totale delle bollette) e, se serve, tornare al calcolo **Auto**.
3. Con **✓ Approva** la ripartizione genera le **quote** per ciascuna unità.

Se ci sono unità **senza letture**, l'app mostra un avviso con il collegamento alle letture mancanti.
