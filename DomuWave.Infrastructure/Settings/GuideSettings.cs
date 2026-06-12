namespace DomuWave.Services.Settings
{
    /// <summary>
    /// Impostazioni della sezione "Guida" (appsettings → "Guide").
    /// </summary>
    public class GuideSettings
    {
        /// <summary>
        /// Cartella che contiene gli articoli Markdown della guida.
        /// Può essere assoluta o relativa alla cartella dell'applicazione.
        /// Default: "wwwroot/guide".
        /// </summary>
        public string ContentRoot { get; set; } = "wwwroot/guide";

        /// <summary>Lunghezza massima della domanda accettata dall'endpoint AI (Fase 2).</summary>
        public int MaxQuestionLength { get; set; } = 1000;

        /// <summary>Impostazioni del provider AI (Fase 2). ApiKey vuota = AI non configurata.</summary>
        public GuideAnthropicSettings Anthropic { get; set; } = new();

        /// <summary>Limiti di frequenza per l'endpoint AI pubblico (Fase 2).</summary>
        public GuideRateLimitSettings RateLimit { get; set; } = new();
    }

    public class GuideAnthropicSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model  { get; set; } = "claude-opus-4-8";
    }

    public class GuideRateLimitSettings
    {
        public int PermitPerWindow { get; set; } = 10;
        public int WindowSeconds   { get; set; } = 60;
    }
}
