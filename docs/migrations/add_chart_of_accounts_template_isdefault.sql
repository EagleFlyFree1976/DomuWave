-- Migration: aggiunge il flag IsDefault alla tabella ChartOfAccountsTemplate
-- Regola: un solo template per tenant può avere IsDefault = 1
-- Il template standard creato alla creazione del tenant viene automaticamente
-- impostato come predefinito dal ChartOfAccountsTemplateSeedService.

ALTER TABLE ChartOfAccountsTemplate
    ADD IsDefault BIT NOT NULL DEFAULT 0;

-- Per i tenant esistenti: imposta IsDefault=1 sul template il cui nome corrisponde
-- al template standard, oppure sul primo template attivo per ciascun tenant.
-- Eseguire manualmente dopo la migrazione se necessario, oppure usare il
-- pulsante "Imposta predefinito" nell'interfaccia di gestione template.
