-- Tabella di appoggio per le registrazioni in attesa di conferma (pagamento)
-- L'Id (GUID) diventa l'Id del Tenant al momento della conferma

CREATE TABLE PendingRegistration (
    Id                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Email               NVARCHAR(200)    NOT NULL,
    PasswordHash        NVARCHAR(500)    NOT NULL,
    TenantName          NVARCHAR(200)    NOT NULL,
    Status              INT              NOT NULL DEFAULT 0,   -- 0=Pending, 1=Confirmed, 2=Expired
    CreatedAt           DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    ConfirmedAt         DATETIME2        NULL,
    ExpiresAt           DATETIME2        NOT NULL DEFAULT DATEADD(DAY, 7, GETUTCDATE())
);

CREATE INDEX IX_PendingRegistration_Email  ON PendingRegistration (Email);
CREATE INDEX IX_PendingRegistration_Status ON PendingRegistration (Status);
