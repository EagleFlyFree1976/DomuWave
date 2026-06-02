-- ============================================================================
-- Voce di menu "Report spese per tabella millesimale"
-- Inserita sotto "Contabilità" (MenuId = 3), dopo "Rendiconto".
-- Idempotente: non duplica se l'Action è già presente.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/report-spese-millesimali')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 3, 'pi-file-export', 'Report spese per millesimi', '/report-spese-millesimali', NULL, NULL, 1, 41, NULL);
END
GO
