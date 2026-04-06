-- ============================================================
-- Riporto morosità: aggiunge campi di dettaglio a UnitOpeningBalance
--
-- Nuovi campi:
--   RateAddebitate  = Σ CondominiumFee.AmountDue  (preventivo)
--   RateIncassate   = Σ CondominiumFee.AmountPaid (preventivo)
--   QuotaConsuntiva = quota consuntiva dell'unità (impostata all'approvazione del Consuntivo)
--   SaldoConguaglio = QuotaConsuntiva - RateAddebitate (+debito / -credito da conguaglio)
--
-- TotalMovements (esistente) diventa:
--   TotalMovements = (RateAddebitate - RateIncassate) + SaldoConguaglio
--
-- ClosingBalance (esistente) rimane:
--   ClosingBalance = OpeningBalance + TotalMovements
-- ============================================================

ALTER TABLE UnitOpeningBalance
    ADD RateAddebitate  DECIMAL(18,4) NOT NULL DEFAULT 0,
        RateIncassate   DECIMAL(18,4) NOT NULL DEFAULT 0,
        QuotaConsuntiva DECIMAL(18,4) NOT NULL DEFAULT 0,
        SaldoConguaglio DECIMAL(18,4) NOT NULL DEFAULT 0;
GO

-- Backfill: per i record esistenti, TotalMovements era già
-- (RateAddebitate - RateIncassate) senza conguaglio, quindi:
--   RateAddebitate  = TotalMovements  (approssimazione: tutto su addebiti, nessun pagamento)
-- Non possiamo ricostruire esattamente RateAddebitate/RateIncassate
-- dai dati storici senza ricalcolo completo, lasciamo a zero e
-- lasciamo il TotalMovements invariato per compatibilità.
-- (Un eventuale ricalcolo manuale può essere eseguito alla prossima chiusura esercizio.)
GO
