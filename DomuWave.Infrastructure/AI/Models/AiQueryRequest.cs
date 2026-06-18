using System;
using System.Collections.Generic;

namespace DomuWave.Services.AI.Models
{
    /// <summary>
    /// Richiesta in linguaggio naturale inviata dall'amministratore al modulo AI.
    /// </summary>
    public class AiQueryRequest
    {
        /// <summary>Testo libero della domanda dell'amministratore.</summary>
        public string Question { get; set; }

        /// <summary>Tenant corrente. Valorizzato lato controller dall'header X-Tenant-Id.</summary>
        public Guid TenantId { get; set; }

        /// <summary>ID condominio opzionale. null = tutti i condomini gestiti.</summary>
        public int? CondominiumId { get; set; }

        /// <summary>Anno fiscale opzionale. null = anno corrente.</summary>
        public int? FiscalYear { get; set; }

        /// <summary>Storico della conversazione (escluso l'ultimo messaggio utente).</summary>
        public List<AiMessage> History { get; set; } = new();
    }
}
