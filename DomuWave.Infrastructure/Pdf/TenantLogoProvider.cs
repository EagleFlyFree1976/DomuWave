using DomuWave.Services.Models;
using NHibernate;
using NHibernate.Linq;

namespace DomuWave.Services.Pdf;

/// <summary>
/// Helper condiviso per recuperare il logo del tenant (BLOB) da usare come
/// intestazione nei documenti PDF generati lato backend (avvisi, comunicazioni).
/// </summary>
public static class TenantLogoProvider
{
    /// <summary>
    /// Restituisce il contenuto binario del logo del tenant proprietario del condominio,
    /// oppure null se non impostato o non renderizzabile da QuestPDF.
    /// NOTA: QuestPDF <c>.Image(byte[])</c> gestisce solo immagini raster (PNG/JPEG/WebP);
    /// gli SVG userebbero un'API diversa (<c>.Svg</c>) e qui vengono esclusi per non far
    /// fallire la generazione del PDF (in tal caso il documento esce semplicemente senza logo).
    /// </summary>
    public static async Task<byte[]?> GetLogoForCondominiumAsync(
        ISession        session,
        Condominium?    condominium,
        CancellationToken cancellationToken)
    {
        var tenantId = condominium?.Tenant?.Id;
        if (tenantId == null || tenantId == Guid.Empty)
            return null;

        var settings = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == tenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var logo = settings?.LogoContent;
        if (logo == null || logo.Length == 0)
            return null;

        // Escludi SVG (non renderizzabile via .Image()): content-type dichiarato o firma "<svg"/"<?xml".
        var contentType = settings!.LogoContentType ?? string.Empty;
        if (contentType.Contains("svg", StringComparison.OrdinalIgnoreCase) || LooksLikeSvg(logo))
            return null;

        return logo;
    }

    private static bool LooksLikeSvg(byte[] content)
    {
        // Ispeziona i primi byte per il marcatore SVG/XML.
        var len = Math.Min(content.Length, 256);
        var head = System.Text.Encoding.UTF8.GetString(content, 0, len);
        return head.Contains("<svg", StringComparison.OrdinalIgnoreCase)
            || head.Contains("<?xml", StringComparison.OrdinalIgnoreCase);
    }
}
