-- ============================================================
-- Migration: aggiunge DisplayName a RealEstateUnit
-- ============================================================

ALTER TABLE RealEstateUnit ADD DisplayName NVARCHAR(200) NULL;

-- Popola DisplayName con l'ultimo proprietario attivo per ogni unità
UPDATE u
SET u.DisplayName = (
    SELECT TOP 1
        CASE
            WHEN ISNULL(o.LastName, '') <> '' THEN o.LastName
            ELSE o.FirstName
        END
    FROM UnitOwner o
    WHERE o.UnitId = u.Id
      AND o.IsActive = 1
      AND o.IsDeleted = 0
    ORDER BY o.LastName
)
FROM RealEstateUnit u;
