namespace DomuWave.Services.AI.Models
{
    /// <summary>
    /// Singolo messaggio della conversazione AI (storico chat).
    /// </summary>
    public class AiMessage
    {
        /// <summary>"user" oppure "assistant".</summary>
        public string Role { get; set; }

        /// <summary>Contenuto testuale del messaggio.</summary>
        public string Content { get; set; }
    }
}
