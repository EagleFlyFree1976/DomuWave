namespace DomuWave.Services.Settings
{
    /// <summary>
    /// Impostazioni del modulo AI Assistant (appsettings → "AnthropicAI").
    /// </summary>
    public class AnthropicAiSettings
    {
        /// <summary>
        /// API key Anthropic. In appsettings è cifrata con EncryptString();
        /// se valorizzata con prefisso cifrato viene decifrata all'uso.
        /// ApiKey vuota = feature AI disabilitata.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Modello da usare. Convenzione di progetto: claude-sonnet-4-6.</summary>
        public string Model { get; set; } = "claude-sonnet-4-6";

        /// <summary>max_tokens per risposte sintetiche.</summary>
        public int MaxTokens { get; set; } = 1024;

        /// <summary>Limite giornaliero di query per utente (rate limiting).</summary>
        public int MaxQueriesPerUserPerDay { get; set; } = 50;
    }
}
