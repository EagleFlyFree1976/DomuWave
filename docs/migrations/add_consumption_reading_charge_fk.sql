-- Aggiunge FK ChargeId su ConsumptionReading per tracciare a quale ripartizione
-- appartiene ogni lettura. NULL = lettura non ancora ripartita.

ALTER TABLE ConsumptionReading
    ADD ChargeId INT NULL;

ALTER TABLE ConsumptionReading
    ADD CONSTRAINT FK_ConsumptionReading_Charge
        FOREIGN KEY (ChargeId) REFERENCES ConsumptionCharge(Id);
