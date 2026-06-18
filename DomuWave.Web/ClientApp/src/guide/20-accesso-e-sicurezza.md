---
title: "Accesso e sicurezza"
slug: "accesso-e-sicurezza"
section: "Per iniziare"
order: 20
---

## Accedere alla piattaforma
1. Apri la pagina di **login**.
2. Inserisci **email** (o username) e **password**.
3. Premi **Accedi**.

Se hai già una sessione attiva, verrai portato direttamente alla **dashboard**.

## Ho dimenticato la password
1. Nella pagina di login clicca **"Password dimenticata"**.
2. Inserisci l'**email** del tuo account e invia la richiesta.
3. Controlla la casella di posta: riceverai un'email con un **link di reimpostazione**.
4. Apri il link e imposta una **nuova password**.

> Se non ricevi l'email, controlla la cartella spam. Il link ha una scadenza: se è scaduto, ripeti la procedura.

## Registrare un nuovo studio
Dalla pagina pubblica è possibile avviare la registrazione di un nuovo studio di amministrazione. La procedura, guidata in pochi passaggi, chiede:
- **email e password** del primo account amministratore;
- il **nome dello studio/azienda**;
- facoltativamente i dati del **primo condominio**.

Al termine ricevi un'**email di conferma**: clicca il link per attivare l'account.

## Ruoli e permessi
DomuWave distingue i contenuti visibili in base al **ruolo** dell'utente:

- **Amministratore / operatore** — accede ai moduli operativi (condomìni, contabilità, comunicazioni, ecc.).
- **Condòmino** — accede in sola lettura ai dati del proprio condominio (documenti, rate, comunicazioni).

Molte sezioni operative richiedono che sia stato **selezionato un condominio**: finché non lo selezioni, le relative voci di menu restano nascoste.

## Gestione autorizzazioni (SuperAdmin)
Gli utenti **SuperAdmin** trovano nel menu la voce **Autorizzazioni**: una pagina a due pannelli per governare in modo granulare cosa può fare ciascun ruolo o gruppo.

Il pannello di sinistra ha tre schede:

- **Risorse** — l'elenco delle aree funzionali proteggibili (es. *Condomìni*, *Spese*, *Fornitori*). Da qui puoi:
  - **creare** una nuova risorsa con il pulsante **+**, indicando un **codice** (univoco, non più modificabile in seguito), una **descrizione** e l'**area** di appartenenza;
  - **modificare** descrizione e area di una risorsa esistente selezionandola;
  - **eliminare** una risorsa, purché non sia già assegnata ad alcun ruolo, gruppo o utente.
- **Ruoli** e **Gruppi** — selezionando una voce, il pannello di destra mostra i **permessi assegnati** in forma di matrice: una riga per risorsa e cinque colonne (**Visualizza, Crea, Modifica, Elimina, Azione**). Fai clic su una cella per attivare/disattivare il singolo livello; la **ricerca** in alto filtra le risorse e, se cerchi una risorsa esistente ma non ancora assegnata, compare un pulsante **+ Aggiungi** per assegnarla al volo.

Sempre dalla scheda **Ruoli** il SuperAdmin può:

- **Nuovo ruolo** (pulsante **+** in cima alla lista) — crea un ruolo vuoto indicando **codice**, **descrizione** e **modulo** di appartenenza.
- **Modifica** — cambia **codice** e/o **descrizione** del ruolo selezionato (il codice è univoco).
- **Clona** — crea un nuovo ruolo copiando codice/descrizione (modificabili) e **tutti i permessi** del ruolo selezionato.
- **Copia da…** — unisce in questo ruolo i permessi di un altro ruolo: le risorse in comune vengono sovrascritte, le altre restano invariate.

Le modifiche ai permessi vengono salvate automaticamente al cambio di ciascuna cella.
