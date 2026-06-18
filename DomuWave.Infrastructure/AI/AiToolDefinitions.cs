using System.Collections.Generic;
using System.Text.Json;

namespace DomuWave.Services.AI
{
    /// <summary>
    /// Definizione dei tool esposti al modello Anthropic (function calling).
    /// Ogni tool corrisponde a una capacità del service layer esistente; il routing
    /// effettivo è in <see cref="AiToolDispatcher"/>.
    /// </summary>
    public static class AiToolDefinitions
    {
        /// <summary>Restituisce la definizione di tutti i tool come elementi JSON.</summary>
        public static IReadOnlyList<JsonElement> GetAllTools()
        {
            var defs = new[]
            {
                GetCondominoPayments,
                GetOverdueFees,
                GetInstallmentStatus,
                GetExpenseSummary,
                GetBudgetVsActual,
                GetCondominiumList,
                GetOwnerBalance,
            };

            var result = new List<JsonElement>(defs.Length);
            foreach (var json in defs)
                result.Add(JsonDocument.Parse(json).RootElement.Clone());
            return result;
        }

        private const string GetCondominoPayments = """
        {
          "name": "get_condomino_payments",
          "description": "Recupera lo stato pagamenti (rate/quote condominiali) di un condomino cercandolo per nome o cognome. Usare quando l'utente chiede dello stato pagamenti, morosità o quote di una persona specifica.",
          "input_schema": {
            "type": "object",
            "properties": {
              "owner_name": { "type": "string", "description": "Nome o cognome del proprietario/condomino." },
              "condominium_id": { "type": "integer", "description": "ID condominio. Omettere per cercare su tutti i condomini gestiti." },
              "year": { "type": "integer", "description": "Anno fiscale. Default: anno corrente." }
            },
            "required": ["owner_name"]
          }
        }
        """;

        private const string GetOverdueFees = """
        {
          "name": "get_overdue_fees",
          "description": "Restituisce l'elenco dei morosi (quote scadute e non pagate) di un condominio. Usare per domande su chi è in ritardo con i pagamenti.",
          "input_schema": {
            "type": "object",
            "properties": {
              "condominium_id": { "type": "integer", "description": "ID condominio di cui elencare i morosi." }
            },
            "required": ["condominium_id"]
          }
        }
        """;

        private const string GetInstallmentStatus = """
        {
          "name": "get_installment_status",
          "description": "Restituisce lo stato delle rate aperte/scadute di un condominio (numero rata, scadenza, importo, stato).",
          "input_schema": {
            "type": "object",
            "properties": {
              "condominium_id": { "type": "integer", "description": "ID condominio." },
              "only_overdue": { "type": "boolean", "description": "Se true restituisce solo le rate scadute; altrimenti tutte quelle aperte. Default false." }
            },
            "required": ["condominium_id"]
          }
        }
        """;

        private const string GetExpenseSummary = """
        {
          "name": "get_expense_summary",
          "description": "Riepilogo delle spese di un condominio per un anno: totale lordo, numero documenti, spese non pagate. Usare per domande sulle spese complessive.",
          "input_schema": {
            "type": "object",
            "properties": {
              "condominium_id": { "type": "integer", "description": "ID condominio." },
              "year": { "type": "integer", "description": "Anno fiscale. Default: anno corrente." }
            },
            "required": ["condominium_id"]
          }
        }
        """;

        private const string GetBudgetVsActual = """
        {
          "name": "get_budget_vs_actual",
          "description": "Confronta il preventivo (Budget tipo Preventivo) con il consuntivo/spese effettive di un condominio per un anno. Usare per domande su scostamenti o spese non coperte dal preventivo.",
          "input_schema": {
            "type": "object",
            "properties": {
              "condominium_id": { "type": "integer", "description": "ID condominio." },
              "year": { "type": "integer", "description": "Anno fiscale. Default: anno corrente." }
            },
            "required": ["condominium_id"]
          }
        }
        """;

        private const string GetCondominiumList = """
        {
          "name": "get_condominium_list",
          "description": "Elenca i condomini attivi gestiti dall'amministratore (nome + ID). Usare per disambiguare un condominio citato dall'utente.",
          "input_schema": {
            "type": "object",
            "properties": {}
          }
        }
        """;

        private const string GetOwnerBalance = """
        {
          "name": "get_owner_balance",
          "description": "Restituisce il saldo residuo complessivo di un condomino (somma dei balance delle sue quote), cercandolo per nome o cognome.",
          "input_schema": {
            "type": "object",
            "properties": {
              "owner_name": { "type": "string", "description": "Nome o cognome del proprietario/condomino." },
              "condominium_id": { "type": "integer", "description": "ID condominio. Omettere per cercare su tutti i condomini gestiti." }
            },
            "required": ["owner_name"]
          }
        }
        """;
    }
}
