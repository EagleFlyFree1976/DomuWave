using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DomuWave.Services.Helper;

public class TrackedCancellationTokenSource : CancellationTokenSource
{
    private readonly ILogger? _logger;

    public string? Reason { get; private set; }
    public string? CancelledStackTrace { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public TrackedCancellationTokenSource(ILogger? logger = null)
    {
        _logger = logger;
    }

    public void Cancel(string? reason = null, bool throwOnFirstException = false)
    {
        Reason = reason ?? "No reason provided";
        CancelledStackTrace = new StackTrace(true).ToString();
        CancelledAtUtc = DateTime.UtcNow;

        _logger?.LogInformation(
            "Cancellation requested. Reason: {Reason}, Time: {Time}, StackTrace: {Stack}",
            Reason,
            CancelledAtUtc?.ToString("O"),
            CancelledStackTrace
        );

        base.Cancel(throwOnFirstException);
    }

    // Override dei metodi base per compatibilità
    public new void Cancel()
    {
        Cancel(null, false);
    }

    public new void Cancel(bool throwOnFirstException)
    {
        Cancel(null, throwOnFirstException);
    }
}