-- Rimuove il vincolo di unicità MeterId+FiscalYearId su ConsumptionReading
-- per supportare N letture per contatore per esercizio fiscale.

ALTER TABLE ConsumptionReading
    DROP CONSTRAINT UQ_ConsumptionReading_MeterFiscalYear;
