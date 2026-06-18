namespace DomuWave.Services.AI.Models
{
    /// <summary>
    /// Risultato strutturato dell'esecuzione di un tool, da rinviare ad Anthropic.
    /// Solo dati aggregati/strutturati transitano nelle API (vedi nota GDPR del piano).
    /// </summary>
    public class AiToolResult
    {
        /// <summary>Nome del tool eseguito.</summary>
        public string ToolName { get; set; }

        /// <summary>true se il tool ha prodotto un risultato valido.</summary>
        public bool Success { get; set; }

        /// <summary>
        /// Payload JSON-serializzabile da restituire al modello come tool_result.
        /// Può essere un oggetto anonimo, una lista o un messaggio di errore.
        /// </summary>
        public object Data { get; set; }

        public static AiToolResult Ok(string toolName, object data)
            => new() { ToolName = toolName, Success = true, Data = data };

        public static AiToolResult Error(string toolName, string message)
            => new() { ToolName = toolName, Success = false, Data = new { error = message } };
    }
}
