-- ============================================================================
-- auth.base_Users: indice mancante su Token.
--   UserService.GetByTokenAsync (CPQ.Core) esegue WHERE Token = @token ad
--   OGNI richiesta autenticata (TokenAuthorizeAttribute, un filtro action
--   applicato a tutti gli endpoint API). Senza indice dedicato la query fa
--   uno scan su base_Users; osservato in produzione un costo di ~500-570ms
--   su connessione/piano di query "freddi", che si propaga a cascata su
--   tutte le richieste in corso in quel momento (endpoint diversi ma stessa
--   attesa su Get by token). Aggiunto per accelerare l'autenticazione via
--   token su ogni richiesta API.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('auth.base_Users') AND name = 'IX_base_Users_Token'
)
BEGIN
    CREATE INDEX IX_base_Users_Token ON auth.base_Users (Token) WHERE Token IS NOT NULL;
END
GO
