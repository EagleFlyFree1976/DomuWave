using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using CPQ.Core.Security;
using DomuWave.Services.AI.Models;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.AI
{
    /// <summary>
    /// Orchestratore del modulo AI Assistant. Costruisce il contesto del tenant,
    /// dialoga con la Messages API di Anthropic in modalità function calling ed esegue
    /// i tool sui service layer esistenti.
    /// </summary>
    public class AiOrchestratorService : IAiOrchestratorService
    {
        private const string AnthropicEndpoint = "/v1/messages";

        private readonly ICondominiumService _condominiumService;
        private readonly AiToolDispatcher _dispatcher;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AnthropicAiSettings _settings;
        private readonly ILogger<AiOrchestratorService> _logger;

        public AiOrchestratorService(
            ICondominiumService condominiumService,
            AiToolDispatcher dispatcher,
            IHttpClientFactory httpClientFactory,
            IOptions<AnthropicAiSettings> settings,
            ILogger<AiOrchestratorService> logger)
        {
            _condominiumService = condominiumService;
            _dispatcher = dispatcher;
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<AiQueryResponse> HandleQueryAsync(
            AiQueryRequest request, IUser currentUser, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return Fail("La domanda è vuota.");

            var apiKey = ResolveApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
                return Fail("Il modulo AI non è configurato (API key mancante).");

            var condominiums = await _condominiumService
                .GetActiveCondominiumsAsync(request.TenantId, currentUser, ct)
                .ConfigureAwait(false);

            var currentYear = request.FiscalYear ?? DateTime.Now.Year;
            var systemPrompt = BuildSystemPrompt(condominiums, currentYear);
            var tools = AiToolDefinitions.GetAllTools();
            var messages = BuildMessages(request);

            // Logging GDPR-safe: solo la domanda dell'utente, mai la risposta AI.
            _logger.LogInformation("AI query (tenant {TenantId}): {Question}", request.TenantId, request.Question);

            var toolUsed = (string)null;

            // Loop di esecuzione: max 5 round per evitare cicli infiniti di tool use.
            for (var round = 0; round < 5; round++)
            {
                var response = await CallAnthropicAsync(apiKey, systemPrompt, messages, tools, ct)
                    .ConfigureAwait(false);

                if (response.StopReason == "tool_use")
                {
                    // Riporta l'intero content dell'assistant (incluso il blocco tool_use).
                    messages.Add(new { role = "assistant", content = response.RawContent });

                    var toolResults = new List<object>();
                    foreach (var block in response.ToolUseBlocks)
                    {
                        toolUsed = block.Name;
                        var result = await _dispatcher
                            .DispatchAsync(block.Name, block.Input, request.TenantId, currentYear, currentUser, ct)
                            .ConfigureAwait(false);

                        toolResults.Add(new
                        {
                            type = "tool_result",
                            tool_use_id = block.Id,
                            content = JsonSerializer.Serialize(result.Data)
                        });
                    }

                    messages.Add(new { role = "user", content = toolResults });
                    continue;
                }

                return new AiQueryResponse
                {
                    Answer = response.Text,
                    ToolUsed = toolUsed,
                    Success = true
                };
            }

            return Fail("L'assistente non è riuscito a completare la richiesta.");
        }

        // ─── System prompt ────────────────────────────────────────────────────
        private static string BuildSystemPrompt(IList<Condominium> condominiums, int currentYear)
        {
            var condList = condominiums.Count == 0
                ? "(nessun condominio attivo)"
                : string.Join(", ", condominiums.Select(c => $"{c.Name} (ID:{c.Id})"));

            return $"""
            Sei l'assistente AI di DomuWave, il gestionale per amministratori condominiali italiani.
            Rispondi sempre in italiano formale.

            Condomini gestiti da questo amministratore: {condList}
            Anno fiscale corrente: {currentYear}

            Regole:
            - Usa i tool per recuperare i dati. Non inventare mai cifre.
            - Se il nome del condomino o del condominio è ambiguo, chiedi conferma prima di chiamare un tool.
            - Presenta i dati finanziari con importi in Euro (€).
            - In caso di morosità, usa un tono neutro e professionale.
            - Sii sintetico: rispondi solo a ciò che è stato chiesto.
            """;
        }

        // ─── Costruzione messaggi ─────────────────────────────────────────────
        private static List<object> BuildMessages(AiQueryRequest request)
        {
            var messages = new List<object>();
            foreach (var m in request.History ?? new List<AiMessage>())
            {
                if (string.IsNullOrWhiteSpace(m.Content)) continue;
                var role = m.Role == "assistant" ? "assistant" : "user";
                messages.Add(new { role, content = m.Content });
            }
            messages.Add(new { role = "user", content = request.Question });
            return messages;
        }

        // ─── Chiamata HTTP ad Anthropic ───────────────────────────────────────
        private async Task<AnthropicResult> CallAnthropicAsync(
            string apiKey, string systemPrompt, List<object> messages,
            IReadOnlyList<JsonElement> tools, CancellationToken ct)
        {
            var payload = new
            {
                model = _settings.Model,
                max_tokens = _settings.MaxTokens,
                system = systemPrompt,
                messages,
                tools
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("AnthropicClient");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, AnthropicEndpoint) { Content = content };
            httpRequest.Headers.Add("x-api-key", apiKey);

            using var httpResponse = await client.SendAsync(httpRequest, ct).ConfigureAwait(false);
            var body = await httpResponse.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error {Status}: {Body}", (int)httpResponse.StatusCode, body);
                throw new InvalidOperationException(
                    $"Anthropic API ha risposto con stato {(int)httpResponse.StatusCode}: {ExtractAnthropicError(body)}");
            }

            return ParseAnthropicResponse(body);
        }

        // Estrae il messaggio di errore dal body Anthropic: {"type":"error","error":{"message":"..."}}
        private static string ExtractAnthropicError(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "(nessun dettaglio)";
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err)
                    && err.TryGetProperty("message", out var msg))
                {
                    return msg.GetString();
                }
            }
            catch { /* body non JSON: restituiamo grezzo */ }
            return body.Length > 500 ? body.Substring(0, 500) : body;
        }

        private static AnthropicResult ParseAnthropicResponse(string body)
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var result = new AnthropicResult
            {
                StopReason = root.TryGetProperty("stop_reason", out var sr) ? sr.GetString() : null,
                RawContent = root.TryGetProperty("content", out var c) ? c.Clone() : default
            };

            if (root.TryGetProperty("content", out var contentArr) && contentArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var block in contentArr.EnumerateArray())
                {
                    var type = block.TryGetProperty("type", out var t) ? t.GetString() : null;
                    if (type == "text" && block.TryGetProperty("text", out var txt))
                    {
                        result.Text = (result.Text ?? string.Empty) + txt.GetString();
                    }
                    else if (type == "tool_use")
                    {
                        result.ToolUseBlocks.Add(new ToolUseBlock
                        {
                            Id = block.TryGetProperty("id", out var id) ? id.GetString() : null,
                            Name = block.TryGetProperty("name", out var n) ? n.GetString() : null,
                            Input = block.TryGetProperty("input", out var inp) ? inp.Clone() : default
                        });
                    }
                }
            }

            return result;
        }

        // ─── Utility ──────────────────────────────────────────────────────────
        private string ResolveApiKey()
        {
            var key = _settings.ApiKey;
            if (string.IsNullOrWhiteSpace(key))
                return null;

            // Le chiavi Anthropic iniziano con "sk-ant-"; se così non è, è cifrata.
            if (key.StartsWith("sk-ant-", StringComparison.OrdinalIgnoreCase))
                return key;

            try { return key.DecryptString(); }
            catch { return key; }
        }

        private static AiQueryResponse Fail(string message)
            => new() { Success = false, ErrorMessage = message, Answer = message };

        // ─── Modelli interni di parsing ───────────────────────────────────────
        private sealed class AnthropicResult
        {
            public string StopReason { get; set; }
            public string Text { get; set; }
            public JsonElement RawContent { get; set; }
            public List<ToolUseBlock> ToolUseBlocks { get; } = new();
        }

        private sealed class ToolUseBlock
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public JsonElement Input { get; set; }
        }
    }
}
