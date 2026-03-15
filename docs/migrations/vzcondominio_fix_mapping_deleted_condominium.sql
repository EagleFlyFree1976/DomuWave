-- ============================================================
-- Hotfix: riallinea mapping VZ condomini quando punta a record IsDeleted=1
-- Caso noto: VZId=11 -> DomuWaveId=177 (IsDeleted=1), corretto DomuWaveId=176
--
-- Eseguire su DomuWave PRIMA di rilanciare Stage 2.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1) Correggi mapping del condominio
    UPDATE m
       SET m.DomuWaveId = 176,
           m.RunCode = CONCAT(ISNULL(m.RunCode, ''), '|MANUAL-FIX-', FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss'))
    FROM dbo.Map_VZ_Condominium m
    WHERE m.VZId = 11
      AND m.DomuWaveId = 177;

    -- 2) Riallinea FiscalYear già migrati sul condominio corretto
    UPDATE fy
       SET fy.CondominiumId = 176
    FROM dbo.FiscalYear fy
    INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.DomuWaveId = fy.FiscalYearId
    INNER JOIN stg.VZ_FiscalYear src ON src.VZId = mf.VZId
    WHERE src.VZCondominiumId = 11
      AND fy.CondominiumId = 177;

    -- 3) Riallinea Budget già migrati (dipendono da FiscalYear/Condominium)
    UPDATE b
       SET b.CondominiumId = 176
    FROM dbo.Budget b
    INNER JOIN dbo.Map_VZ_Budget mb ON mb.DomuWaveBudgetId = b.Id
    INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.VZId = mb.VZFiscalYearId
    INNER JOIN stg.VZ_FiscalYear src ON src.VZId = mf.VZId
    WHERE src.VZCondominiumId = 11
      AND b.CondominiumId = 177;

    -- 4) Riallinea Expense già migrate
    UPDATE e
       SET e.CondominiumId = 176
    FROM dbo.Expense e
    INNER JOIN dbo.Map_VZ_Expense me ON me.DomuWaveId = e.Id
    INNER JOIN dbo.Map_VZ_FiscalYear mf ON mf.DomuWaveId = e.FiscalYearId
    INNER JOIN stg.VZ_FiscalYear src ON src.VZId = mf.VZId
    WHERE src.VZCondominiumId = 11
      AND e.CondominiumId = 177;

    -- 5) Riallinea CondominiumInstallment già migrate
    UPDATE ci
       SET ci.CondominiumId = 176
    FROM dbo.CondominiumInstallment ci
    INNER JOIN dbo.Map_VZ_Installment mi ON mi.DomuWaveInstallmentId = ci.Id
    INNER JOIN stg.VZ_BudgetInstallment bi
        ON bi.VZFiscalYearId = mi.VZFiscalYearId
       AND bi.DueDate = mi.DueDate
       AND bi.Description = mi.Description
    INNER JOIN stg.VZ_FiscalYear src ON src.VZId = bi.VZFiscalYearId
    WHERE src.VZCondominiumId = 11
      AND ci.CondominiumId = 177;

    COMMIT TRANSACTION;

    -- Verifica finale
    SELECT m.VZId, m.DomuWaveId, c.Name, c.Code, c.IsDeleted
    FROM dbo.Map_VZ_Condominium m
    LEFT JOIN dbo.Condominium c ON c.Id = m.DomuWaveId
    WHERE m.VZId = 11;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrLine INT = ERROR_LINE();
    RAISERROR('Hotfix failed at line %d: %s', 16, 1, @ErrLine, @ErrMsg);
END CATCH;
GO
