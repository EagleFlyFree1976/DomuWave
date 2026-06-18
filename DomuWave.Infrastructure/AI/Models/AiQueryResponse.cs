namespace DomuWave.Services.AI.Models
{
    /// <summary>
    /// Risposta del modulo AI alla domanda dell'amministratore.
    /// </summary>
    public class AiQueryResponse
    {
        /// <summary>Risposta finale in italiano formulata dall'AI.</summary>
        public string Answer { get; set; }

        /// <summary>Nome del tool eventualmente eseguito (diagnostica/telemetria).</summary>
        public string ToolUsed { get; set; }

        /// <summary>true se la richiesta è andata a buon fine.</summary>
        public bool Success { get; set; }

        /// <summary>Messaggio di errore in caso di esito negativo.</summary>
        public string ErrorMessage { get; set; }
    }
}
