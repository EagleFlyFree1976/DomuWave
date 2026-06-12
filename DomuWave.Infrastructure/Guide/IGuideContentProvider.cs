namespace DomuWave.Services.Guide;

/// <summary>
/// Fornisce i contenuti della guida. Astrazione che consente di passare da
/// contenuti su file Markdown (oggi) a contenuti su DB (in futuro) senza
/// modificare i consumatori (controller, servizio AI RAG).
/// </summary>
public interface IGuideContentProvider
{
    /// <summary>Indice della guida (sezioni + metadati articoli, senza corpi).</summary>
    Task<GuideIndex> GetIndexAsync(CancellationToken cancellationToken);

    /// <summary>Tutti gli articoli con corpo completo (corpus per il RAG), ordinati.</summary>
    Task<IReadOnlyList<GuideArticle>> GetAllArticlesAsync(CancellationToken cancellationToken);

    /// <summary>Un singolo articolo per slug, o null se non esiste.</summary>
    Task<GuideArticle?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}
