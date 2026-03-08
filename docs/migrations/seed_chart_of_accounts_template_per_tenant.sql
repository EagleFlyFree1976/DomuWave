-- ============================================================
-- Backfill: Seed "Piano dei Conti Standard Italiano" template
-- for every existing tenant that does not yet have it.
--
-- NHibernate hilo (max_lo = 10):
--   ID = next_hi * (max_lo + 1) + lo   (lo = 0..10)
-- We start next_hi at the current value stored in hibernate_unique_key
-- and advance it by 1 for each new row we need to insert.
-- After the script, next_hi is updated so NHibernate won't reuse our IDs.
-- ============================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

DECLARE @Now         DATETIME2       = SYSUTCDATETIME();
DECLARE @SystemUser  NVARCHAR(200)   = N'Sistema';
DECLARE @SystemId    INT             = 1;
DECLARE @TemplateName NVARCHAR(200)  = N'Piano dei Conti Standard Italiano';
DECLARE @Description  NVARCHAR(500)  = N'Template standard con le voci più comuni per la gestione condominiale italiana.';
DECLARE @MaxLo        INT            = 10;  -- must match hbm max_lo

-- ── Read current hilo counters ───────────────────────────────────────────────
DECLARE @TplNextHi  INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'ChartOfAccountsTemplate');
DECLARE @ItemNextHi INT = (SELECT next_hi FROM hibernate_unique_key WHERE entity_type = 'ChartOfAccountsTemplateItem');

-- ── Tenant cursor ─────────────────────────────────────────────────────────────
DECLARE @TenantId UNIQUEIDENTIFIER;

DECLARE tenant_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT Id FROM Tenant WHERE IsDeleted = 0;

OPEN tenant_cur;
FETCH NEXT FROM tenant_cur INTO @TenantId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Skip if template already exists for this tenant
    IF NOT EXISTS (
        SELECT 1 FROM ChartOfAccountsTemplate
        WHERE TenantId = @TenantId AND Name = @TemplateName AND IsDeleted = 0
    )
    BEGIN
        -- ── Compute template ID (hi-lo) ──────────────────────────────────────
        DECLARE @TplId INT = @TplNextHi * (@MaxLo + 1);   -- use lo = 0
        SET @TplNextHi = @TplNextHi + 1;

        INSERT INTO ChartOfAccountsTemplate
            (Id, TenantId, Name, Description, IsActive, IsDeleted,
             CreatedById, CreatedByFullName, LastUpdatedById, LastUpdatedByFullName,
             CreationDate, LastUpdateDate)
        VALUES
            (@TplId, @TenantId, @TemplateName, @Description, 1, 0,
             @SystemId, @SystemUser, NULL, NULL,
             @Now, NULL);

        -- ── Items: we need 34 items, each gets its own hi block ──────────────
        -- Build a temp table of (code, name, type, parentCode, sortOrder)
        DECLARE @Items TABLE (
            RowNum     INT IDENTITY(1,1),
            Code       NVARCHAR(50),
            Name       NVARCHAR(200),
            Type       INT,
            ParentCode NVARCHAR(50) NULL,
            SortOrder  INT
        );

        INSERT INTO @Items (Code, Name, Type, ParentCode, SortOrder) VALUES
        -- Uscite
        ('U',    N'Uscite',                             2, NULL,   10),
        ('U.01', N'Pulizie e igiene',                   2, 'U',    20),
        ('U.02', N'Giardinaggio e aree verdi',          2, 'U',    30),
        ('U.03', N'Manutenzione ordinaria',             2, 'U',    40),
        ('U.04', N'Manutenzione straordinaria',         2, 'U',    50),
        ('U.05', N'Ascensore',                          2, 'U',    60),
        ('U.06', N'Riscaldamento centralizzato',        2, 'U',    70),
        ('U.07', N'Acqua comune',                       2, 'U',    80),
        ('U.08', N'Energia elettrica parti comuni',     2, 'U',    90),
        ('U.09', N'Gas parti comuni',                   2, 'U',   100),
        ('U.10', N'Portineria e vigilanza',             2, 'U',   110),
        ('U.11', N'Assicurazioni',                      2, 'U',   120),
        ('U.12', N'Onorario amministratore',            2, 'U',   130),
        ('U.13', N'Spese legali e consulenze',          2, 'U',   140),
        ('U.14', N'Spese bancarie e postali',           2, 'U',   150),
        ('U.15', N'Imposte e tasse',                    2, 'U',   160),
        ('U.16', N'Fornitura materiali',                2, 'U',   170),
        ('U.17', N'Lavori strutturali',                 2, 'U',   180),
        ('U.18', N'Impianti tecnologici',               2, 'U',   190),
        ('U.19', N'Fondo di riserva',                   2, 'U',   200),
        ('U.20', N'Spese varie',                        2, 'U',   210),
        -- Entrate
        ('E',    N'Entrate',                            1, NULL,  300),
        ('E.01', N'Quote condominiali ordinarie',       1, 'E',   310),
        ('E.02', N'Quote straordinarie',                1, 'E',   320),
        ('E.03', N'Interessi attivi',                   1, 'E',   330),
        ('E.04', N'Proventi da locazione spazi comuni', 1, 'E',   340),
        ('E.05', N'Rimborsi assicurativi',              1, 'E',   350),
        ('E.06', N'Entrate varie',                      1, 'E',   360),
        -- Patrimoniale
        ('P',    N'Patrimoniale',                       3, NULL,  500),
        ('P.01', N'Fondo cassa',                        3, 'P',   510),
        ('P.02', N'Conto corrente bancario',            3, 'P',   520),
        ('P.03', N'Crediti verso condomini',            3, 'P',   530),
        ('P.04', N'Debiti verso fornitori',             3, 'P',   540),
        ('P.05', N'Fondo di riserva (patrimonio)',      3, 'P',   550);

        -- Assign IDs and insert items (parents first, then children)
        DECLARE @ItemId    TABLE (Code NVARCHAR(50), Id INT);

        -- First pass: roots (no parent)
        DECLARE @Code      NVARCHAR(50);
        DECLARE @ItemName  NVARCHAR(200);
        DECLARE @ItemType  INT;
        DECLARE @ItemSort  INT;
        DECLARE @NewItemId INT;

        DECLARE item_cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT Code, Name, Type, SortOrder FROM @Items WHERE ParentCode IS NULL ORDER BY SortOrder;
        OPEN item_cur;
        FETCH NEXT FROM item_cur INTO @Code, @ItemName, @ItemType, @ItemSort;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @NewItemId = @ItemNextHi * (@MaxLo + 1);
            SET @ItemNextHi = @ItemNextHi + 1;
            INSERT INTO ChartOfAccountsTemplateItem
                (Id, TenantId, TemplateId, ParentItemId, Code, Name, Description, Type, SortOrder,
                 IsActive, IsDeleted, CreatedById, CreatedByFullName, LastUpdatedById, LastUpdatedByFullName,
                 CreationDate, LastUpdateDate)
            VALUES
                (@NewItemId, @TenantId, @TplId, NULL, @Code, @ItemName, NULL, @ItemType, @ItemSort,
                 1, 0, @SystemId, @SystemUser, NULL, NULL, @Now, NULL);
            INSERT INTO @ItemId (Code, Id) VALUES (@Code, @NewItemId);
            FETCH NEXT FROM item_cur INTO @Code, @ItemName, @ItemType, @ItemSort;
        END;
        CLOSE item_cur; DEALLOCATE item_cur;

        -- Second pass: children
        DECLARE @ParentCode NVARCHAR(50);
        DECLARE @ParentId   INT;
        DECLARE child_cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT Code, Name, Type, ParentCode, SortOrder FROM @Items WHERE ParentCode IS NOT NULL ORDER BY SortOrder;
        OPEN child_cur;
        FETCH NEXT FROM child_cur INTO @Code, @ItemName, @ItemType, @ParentCode, @ItemSort;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @ParentId   = (SELECT Id FROM @ItemId WHERE Code = @ParentCode);
            SET @NewItemId  = @ItemNextHi * (@MaxLo + 1);
            SET @ItemNextHi = @ItemNextHi + 1;
            INSERT INTO ChartOfAccountsTemplateItem
                (Id, TenantId, TemplateId, ParentItemId, Code, Name, Description, Type, SortOrder,
                 IsActive, IsDeleted, CreatedById, CreatedByFullName, LastUpdatedById, LastUpdatedByFullName,
                 CreationDate, LastUpdateDate)
            VALUES
                (@NewItemId, @TenantId, @TplId, @ParentId, @Code, @ItemName, NULL, @ItemType, @ItemSort,
                 1, 0, @SystemId, @SystemUser, NULL, NULL, @Now, NULL);
            FETCH NEXT FROM child_cur INTO @Code, @ItemName, @ItemType, @ParentCode, @ItemSort;
        END;
        CLOSE child_cur; DEALLOCATE child_cur;

        DELETE FROM @Items;
        DELETE FROM @ItemId;
    END; -- IF NOT EXISTS

    FETCH NEXT FROM tenant_cur INTO @TenantId;
END;

CLOSE tenant_cur;
DEALLOCATE tenant_cur;

-- ── Update hilo counters so NHibernate won't reuse our IDs ───────────────────
UPDATE hibernate_unique_key SET next_hi = @TplNextHi  WHERE entity_type = 'ChartOfAccountsTemplate';
UPDATE hibernate_unique_key SET next_hi = @ItemNextHi WHERE entity_type = 'ChartOfAccountsTemplateItem';

COMMIT TRANSACTION;
PRINT 'Seed completato con successo.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;
