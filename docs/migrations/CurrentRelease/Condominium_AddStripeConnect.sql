-- ============================================================================
-- Condominium: campi per pagamenti online via Stripe Connect (Express).
--   StripeConnectedAccountId -> id del connected account Stripe del condominio
--                               (NULL finché l'amministratore non avvia l'onboarding).
--   StripeOnboardingComplete -> 1 quando l'onboarding è completo e il condominio
--                               può incassare le quote online.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'StripeConnectedAccountId'
)
BEGIN
    ALTER TABLE Condominium ADD StripeConnectedAccountId NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Condominium') AND name = 'StripeOnboardingComplete'
)
BEGIN
    ALTER TABLE Condominium ADD StripeOnboardingComplete BIT NOT NULL
        CONSTRAINT DF_Condominium_StripeOnboardingComplete DEFAULT 0;
END
GO

-- ============================================================================
-- Voce di menu "Pagamenti Online"
-- Inserita sotto "Amministrazione" (MenuId = 6), accanto a "Configurazione Email".
-- Idempotente.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM base_menues WHERE Action = '/impostazioni-pagamenti')
BEGIN
    DECLARE @NewMenuId INT = (SELECT ISNULL(MAX(MenuId), 0) + 1 FROM base_menues);

    INSERT INTO base_menues (MenuId, ParentMenuId, Icon, Description, Action, AuthorizationCode, PopulateEvent, IsEnabled, OrderKey, Tags)
    VALUES (@NewMenuId, 6, 'pi-credit-card', 'Pagamenti Online', '/impostazioni-pagamenti', NULL, NULL, 1, 23, NULL);
END
GO
