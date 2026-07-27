-- Allinea le CondominiumFee la cui rata (CondominiumInstallment) è già stata cancellata (soft-delete)
-- ma che erano rimaste erroneamente IsDeleted = 0, causando importi ancora conteggiati nel rendiconto.
-- Vedi fix: CondominiumInstallmentService.DeleteAsync ora propaga il soft-delete alle Fees collegate.

SELECT f.Id, f.InstallmentId, f.UnitId, f.AmountDue, f.AmountPaid, f.IsDeleted AS Fee_IsDeleted, i.IsDeleted AS Installment_IsDeleted
FROM CondominiumFee f
INNER JOIN CondominiumInstallment i ON i.Id = f.InstallmentId
WHERE i.IsDeleted = 1
  AND f.IsDeleted = 0;

-- Decommentare per applicare la correzione:
/*
UPDATE f
SET f.IsDeleted = 1,
    f.LastUpdateDate = SYSUTCDATETIME()
FROM CondominiumFee f
INNER JOIN CondominiumInstallment i ON i.Id = f.InstallmentId
WHERE i.IsDeleted = 1
  AND f.IsDeleted = 0;
*/
