-- Aggiunge il riferimento all'esercizio di provenienza (precedente).
-- NULL per il primo esercizio del condominio.

ALTER TABLE FiscalYear
    ADD PreviousFiscalYearId INT NULL;

ALTER TABLE FiscalYear
    ADD CONSTRAINT FK_FiscalYear_PreviousFiscalYear
        FOREIGN KEY (PreviousFiscalYearId) REFERENCES FiscalYear(FiscalYearId);
