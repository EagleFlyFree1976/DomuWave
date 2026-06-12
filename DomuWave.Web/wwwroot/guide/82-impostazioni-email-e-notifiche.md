---
title: "Impostazioni email e notifiche"
slug: "impostazioni-email-e-notifiche"
section: "Gestione"
order: 82
---

Perché DomuWave possa inviare comunicazioni e avvisi via email, devi **configurare il server SMTP** e, se vuoi, personalizzare i **template** dei messaggi.

## Configurazione email (SMTP)
In **Configurazione Email** imposti il server di posta:
- **Server SMTP** (es. `smtp.gmail.com`) e **Porta** (di default `587`), con eventuale **SSL/TLS**.
- **Username** e **Password** di autenticazione (in modifica, lascia la password vuota per non cambiarla).
- **Indirizzo** e **Nome del mittente** (es. *Amministrazione Condominio*).

Premi **Salva configurazione**. Con **Invia test** spedisci un'email di prova a un indirizzo per verificare che tutto funzioni. Lo stato attivo/disattivo è indicato in alto.

## Template delle notifiche
In **Template Notifiche** crei modelli riutilizzabili per i messaggi automatici. Per ogni template indichi:
- **Nome** e **Tipo di comunicazione** (Avviso, Assemblea, Manutenzione, Urgente, Informazione, Avviso di pagamento).
- **Oggetto** e **Corpo**, che possono contenere **variabili** racchiuse tra doppie graffe, sostituite automaticamente al momento dell'invio.

Esempi di variabili disponibili: nome del destinatario, numero unità, nome condominio, titolo e testo della comunicazione, dati dell'amministratore, codice esercizio, scadenza, importo, codice di pagamento, IBAN, data e luogo dell'assemblea.

Un'**anteprima** in tempo reale mostra il messaggio con valori di esempio. Puoi impostare un template come **predefinito** per il suo tipo e, con **Ripristina default**, creare i template standard mancanti.
