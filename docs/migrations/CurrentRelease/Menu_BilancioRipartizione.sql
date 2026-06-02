-- ============================================================================
-- Voce di menu "Bilancio di ripartizione"
-- Inserita sotto "Contabilità" (MenuId = 3), dopo "Report spese per millesimi".
-- Idempotente.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/report-bilancio-ripartizione')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 3, 'pi-table', 'Bilancio di ripartizione', '/report-bilancio-ripartizione', NULL, NULL, 1, 42, NULL);
END
GO
