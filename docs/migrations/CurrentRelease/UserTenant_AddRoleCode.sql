SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================================
-- UserTenant.RoleCode: ruolo dell'utente NELLO SPECIFICO tenant.
--
-- Risolve il caso "stesso utente, ruolo diverso per tenant":
--   es. Pippo è amministratore nel proprio tenant, ma condòmino nel tenant di Pluto.
-- Valori: 'Admin' | 'Condomino'. NULL = legacy → fallback al ruolo globale auth.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.UserTenant') AND name = 'RoleCode'
)
BEGIN
    ALTER TABLE dbo.UserTenant ADD RoleCode NVARCHAR(50) NULL;
END
GO

-- ── Backfill ────────────────────────────────────────────────────────────────
-- Un'associazione utente↔tenant è "Condomino" se l'utente ha almeno un'unità
-- (proprietario o inquilino) in un condominio di QUEL tenant; altrimenti "Admin".

-- 1. Condomino: l'utente ha unità nel tenant
UPDATE ut
SET ut.RoleCode = 'Condomino'
FROM dbo.UserTenant ut
WHERE ut.RoleCode IS NULL
  AND ut.IsDeleted = 0
  AND (
        EXISTS (
            SELECT 1
            FROM dbo.UnitOwner o
            JOIN dbo.RealEstateUnit ru ON ru.Id = o.UnitId
            JOIN dbo.Condominium c     ON c.Id  = ru.CondominiumId
            WHERE o.UserId = ut.UserId
              AND o.IsDeleted = 0
              AND c.TenantId = ut.TenantId
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.UnitTenant t
            JOIN dbo.RealEstateUnit ru ON ru.Id = t.UnitId
            JOIN dbo.Condominium c     ON c.Id  = ru.CondominiumId
            WHERE t.UserId = ut.UserId
              AND t.IsDeleted = 0
              AND c.TenantId = ut.TenantId
        )
      );
GO

-- 2. Admin: tutto il resto (associazioni non riconducibili a unità)
UPDATE dbo.UserTenant
SET RoleCode = 'Admin'
WHERE RoleCode IS NULL
  AND IsDeleted = 0;
GO

-- ── Crea le associazioni UserTenant MANCANTI per i condòmini storici ─────────
-- Alcuni condòmini hanno unità ma nessuna riga UserTenant verso quel tenant
-- (creati con un flusso precedente). Le inseriamo come RoleCode='Condomino'.

;WITH MissingLinks AS (
    SELECT DISTINCT
        CAST(o.UserId AS INT) AS UserId,
        c.TenantId            AS TenantId
    FROM dbo.UnitOwner o
    JOIN dbo.RealEstateUnit ru ON ru.Id = o.UnitId
    JOIN dbo.Condominium c     ON c.Id  = ru.CondominiumId
    WHERE o.IsDeleted = 0
    UNION
    SELECT DISTINCT
        CAST(t.UserId AS INT),
        c.TenantId
    FROM dbo.UnitTenant t
    JOIN dbo.RealEstateUnit ru ON ru.Id = t.UnitId
    JOIN dbo.Condominium c     ON c.Id  = ru.CondominiumId
    WHERE t.IsDeleted = 0
),
ToInsert AS (
    SELECT ml.UserId, ml.TenantId,
           ROW_NUMBER() OVER (ORDER BY ml.UserId, ml.TenantId) AS rn
    FROM MissingLinks ml
    WHERE ml.UserId > 0
      AND NOT EXISTS (
            SELECT 1 FROM dbo.UserTenant ut
            WHERE ut.UserId = ml.UserId AND ut.TenantId = ml.TenantId AND ut.IsDeleted = 0
          )
)
INSERT INTO dbo.UserTenant
    (Id, UserId, TenantId, IsDefault, IsActive, RoleCode,
     CreatedById, CreationDate, IsDeleted)
SELECT
    (SELECT ISNULL(MAX(Id),0) FROM dbo.UserTenant) + ti.rn,
    ti.UserId, ti.TenantId, 0, 1, 'Condomino',
    0, SYSUTCDATETIME(), 0
FROM ToInsert ti;
GO

-- Riallinea l'hilo di UserTenant sopra il max Id effettivo
DECLARE @maxId INT = (SELECT ISNULL(MAX(Id),0) FROM dbo.UserTenant);
UPDATE hibernate_unique_key
SET next_hi = (@maxId / 10) + 2
WHERE entity_type = 'UserTenant' AND next_hi <= (@maxId / 10) + 1;
GO
