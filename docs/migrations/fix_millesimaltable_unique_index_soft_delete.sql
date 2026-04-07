-- Fix: IX_MillesimalTable_Code deve escludere i record soft-deleted (IsDeleted = 1)
-- Senza questo filtro il codice univoco viene violato al secondo seed
-- perché il vecchio record (IsDeleted=1) occupa ancora il vincolo.

DROP INDEX IF EXISTS IX_MillesimalTable_Code ON MillesimalTable;

CREATE UNIQUE INDEX IX_MillesimalTable_Code
    ON MillesimalTable (CondominiumId, Code)
    WHERE IsDeleted = 0;
