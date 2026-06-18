using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.AI.Models;

namespace DomuWave.Services.AI
{
    /// <summary>
    /// Orchestratore del modulo AI Assistant: costruisce il contesto del tenant,
    /// dialoga con Anthropic (function calling) ed esegue i tool sui service layer.
    /// </summary>
    public interface IAiOrchestratorService
    {
        Task<AiQueryResponse> HandleQueryAsync(
            AiQueryRequest request,
            IUser currentUser,
            CancellationToken ct);
    }
}
