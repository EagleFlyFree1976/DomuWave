-- ============================================================================
-- AiUsageLog — Contatore giornaliero di query AI per tenant/utente
-- Usato per il rate limiting del modulo AI Assistant (function calling).
-- Idempotente.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AiUsageLog')
BEGIN
    CREATE TABLE dbo.AiUsageLog (
        AiUsageLogId BIGINT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId     UNIQUEIDENTIFIER NOT NULL,
        UserId       BIGINT           NOT NULL,
        QueryDate    DATE             NOT NULL,
        QueryCount   INT              NOT NULL DEFAULT 1,
        -- Audit
        IsDeleted    BIT              NOT NULL DEFAULT 0,
        CreationDate DATETIME2(3)     NOT NULL,
        CONSTRAINT UQ_AiUsage UNIQUE (TenantId, UserId, QueryDate)
    );
END
GO
