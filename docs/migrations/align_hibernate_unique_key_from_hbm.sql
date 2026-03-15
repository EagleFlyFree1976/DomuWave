-- Align dbo.hibernate_unique_key.next_hi based on current data in tables mapped by NHibernate hilo generators.
-- Source of truth for mappings: DomuWave.Infrastructure/Models/Mappings/*.hbm.xml

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('tempdb..#defs') IS NOT NULL DROP TABLE #defs;
    IF OBJECT_ID('tempdb..#out') IS NOT NULL DROP TABLE #out;

    CREATE TABLE #defs (
        EntityType NVARCHAR(200) NOT NULL,
        TableName SYSNAME NOT NULL,
        IdColumn SYSNAME NOT NULL,
        MaxLo INT NOT NULL
    );

    INSERT INTO #defs (EntityType, TableName, IdColumn, MaxLo)
    VALUES
        (N'AccountBalance', N'AccountBalance', N'Id', 10),
        (N'Budget', N'Budget', N'Id', 10),
        (N'BudgetItem', N'BudgetItem', N'Id', 10),
        (N'ChartOfAccounts', N'ChartOfAccounts', N'Id', 10),
        (N'ChartOfAccountsCategory', N'ChartOfAccountsCategory', N'Id', 10),
        (N'ChartOfAccountsCategoryTemplate', N'ChartOfAccountsCategoryTemplate', N'Id', 10),
        (N'ChartOfAccountsTemplate', N'ChartOfAccountsTemplate', N'Id', 10),
        (N'ChartOfAccountsTemplateItem', N'ChartOfAccountsTemplateItem', N'Id', 10),
        (N'Communication', N'Communication', N'Id', 10),
        (N'CommunicationRead', N'CommunicationRead', N'Id', 10),
        (N'Condominium', N'Condominium', N'Id', 10),
        (N'CondominiumAddress', N'CondominiumAddress', N'Id', 10),
        (N'CondominiumCadastralData', N'CondominiumCadastralData', N'Id', 10),
        (N'CondominiumFee', N'CondominiumFee', N'Id', 10),
        (N'CondominiumInstallment', N'CondominiumInstallment', N'Id', 10),
        (N'Document', N'Document', N'Id', 10),
        (N'DocumentAccess', N'DocumentAccess', N'Id', 10),
        (N'DynamicFile', N'DynamicFile', N'Id', 10),
        (N'DynamicFileContent', N'DynamicFileContent', N'Id', 10),
        (N'Expense', N'Expense', N'Id', 10),
        (N'ExpenseAllocation', N'ExpenseAllocation', N'Id', 10),
        (N'ExtraordinaryWork', N'ExtraordinaryWork', N'Id', 10),
        (N'FiscalYear', N'FiscalYear', N'FiscalYearId', 100),
        (N'Maintenance', N'Maintenance', N'Id', 10),
        (N'MenuItem', N'base_menues', N'MenuId', 32767),
        (N'Message', N'Message', N'Id', 10),
        (N'MillesimalTable', N'MillesimalTable', N'Id', 10),
        (N'Payment', N'Payment', N'PaymentId', 10),
        (N'RealEstateUnit', N'RealEstateUnit', N'Id', 10),
        (N'Receipt', N'Receipt', N'Id', 10),
        (N'Supplier', N'Supplier', N'Id', 10),
        (N'SupplierContract', N'SupplierContract', N'Id', 10),
        (N'UnitMillesimal', N'UnitMillesimal', N'Id', 10),
        (N'UnitOpeningBalance', N'UnitOpeningBalance', N'Id', 10),
        (N'UnitOwner', N'UnitOwner', N'Id', 10),
        (N'UnitTenant', N'UnitTenant', N'Id', 10),
        (N'UserTenant', N'UserTenant', N'Id', 10),
        (N'WorkQuote', N'WorkQuote', N'Id', 10),
        (N'WorkQuoteDocument', N'WorkQuoteDocument', N'Id', 10);

    CREATE TABLE #out (
        EntityType NVARCHAR(200) NOT NULL,
        TableName SYSNAME NOT NULL,
        IdColumn SYSNAME NOT NULL,
        MaxId BIGINT NOT NULL,
        MaxLo INT NOT NULL,
        ComputedNextHi INT NOT NULL,
        OldNextHi INT NULL,
        FinalNextHi INT NOT NULL,
        ActionTaken NVARCHAR(20) NOT NULL
    );

    DECLARE @EntityType NVARCHAR(200);
    DECLARE @TableName SYSNAME;
    DECLARE @IdColumn SYSNAME;
    DECLARE @MaxLo INT;
    DECLARE @MaxId BIGINT;
    DECLARE @Block BIGINT;
    DECLARE @Computed INT;
    DECLARE @Old INT;
    DECLARE @Final INT;
    DECLARE @Action NVARCHAR(20);
    DECLARE @Sql NVARCHAR(MAX);

    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT EntityType, TableName, IdColumn, MaxLo
    FROM #defs
    ORDER BY EntityType;

    OPEN c;
    FETCH NEXT FROM c INTO @EntityType, @TableName, @IdColumn, @MaxLo;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @MaxId = 0;
        SET @Sql = N'SELECT @mOut = ISNULL(MAX(CAST(' + QUOTENAME(@IdColumn) + N' AS BIGINT)), 0) FROM dbo.' + QUOTENAME(@TableName) + N';';
        EXEC sys.sp_executesql @Sql, N'@mOut BIGINT OUTPUT', @mOut = @MaxId OUTPUT;

        SET @Block = CAST(@MaxLo AS BIGINT) + 1;
        SET @Computed = CASE
            WHEN @MaxId <= 0 THEN 1
            ELSE CAST(FLOOR((1.0 * @MaxId) / @Block) + 1 AS INT)
        END;

        SELECT @Old = next_hi
        FROM dbo.hibernate_unique_key
        WHERE entity_type = @EntityType;

        IF @Old IS NULL
        BEGIN
            INSERT INTO dbo.hibernate_unique_key (next_hi, entity_type)
            VALUES (@Computed, @EntityType);
            SET @Final = @Computed;
            SET @Action = N'INSERT';
        END
        ELSE
        BEGIN
            SET @Final = CASE WHEN @Old < @Computed THEN @Computed ELSE @Old END;
            UPDATE dbo.hibernate_unique_key
               SET next_hi = @Final
             WHERE entity_type = @EntityType;
            SET @Action = N'UPDATE';
        END

        INSERT INTO #out (EntityType, TableName, IdColumn, MaxId, MaxLo, ComputedNextHi, OldNextHi, FinalNextHi, ActionTaken)
        VALUES (@EntityType, @TableName, @IdColumn, @MaxId, @MaxLo, @Computed, @Old, @Final, @Action);

        FETCH NEXT FROM c INTO @EntityType, @TableName, @IdColumn, @MaxLo;
    END

    CLOSE c;
    DEALLOCATE c;

    COMMIT TRANSACTION;

    SELECT EntityType, TableName, IdColumn, MaxId, MaxLo, ComputedNextHi, OldNextHi, FinalNextHi, ActionTaken
    FROM #out
    ORDER BY EntityType;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
