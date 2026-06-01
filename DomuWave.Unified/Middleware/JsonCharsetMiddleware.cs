namespace DomuWave.Unified.Middleware;

/// <summary>
/// Forza il charset=utf-8 sulle response JSON.
/// L'ExceptionMiddleware di CPQ.Core scrive i messaggi di errore in UTF-8 ma senza
/// dichiarare il charset nel Content-Type: senza questa dichiarazione i client
/// interpretano i byte UTF-8 come Latin-1, producendo testo corrotto (es. "giÃ " invece di "già").
/// Questo middleware corregge il Content-Type appena prima dell'invio della response.
/// </summary>
public sealed class JsonCharsetMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var ct = context.Response.ContentType;
            if (!string.IsNullOrEmpty(ct)
                && ct.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                && !ct.Contains("charset", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = "application/json; charset=utf-8";
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}
