-- Migration: aggiungi colonna IsEnabled a MillesimalTable
-- Il flag permette di disabilitare tabelle già in uso (in spese o piani dei conti)
-- senza poterle eliminare.

ALTER TABLE MillesimalTable
    ADD IsEnabled BIT NOT NULL DEFAULT 1;
