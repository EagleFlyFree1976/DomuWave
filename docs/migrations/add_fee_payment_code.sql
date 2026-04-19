-- Aggiunge il codice univoco di riconciliazione alle quote condominiali
ALTER TABLE CondominiumFee ADD PaymentCode NVARCHAR(20) NULL;

-- Popola i record esistenti con un codice generato dal loro Id
UPDATE CondominiumFee
SET PaymentCode = 'DW-' + RIGHT('0000000' + CAST(Id AS NVARCHAR(7)), 7)
WHERE PaymentCode IS NULL;

-- Rendi il campo NOT NULL e aggiungi indice univoco per tenant
ALTER TABLE CondominiumFee ALTER COLUMN PaymentCode NVARCHAR(20) NOT NULL;

CREATE UNIQUE INDEX UQ_CondominiumFee_PaymentCode
    ON CondominiumFee (PaymentCode)
    WHERE IsDeleted = 0;
