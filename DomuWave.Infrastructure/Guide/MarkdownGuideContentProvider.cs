using System.Text;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Guide;

/// <summary>
/// Content provider che legge gli articoli della guida da file Markdown con
/// frontmatter YAML, dalla cartella configurata in <see cref="GuideSettings.ContentRoot"/>.
///
/// I risultati sono in cache e invalidati quando cambia l'insieme dei file o il
/// loro timestamp di ultima modifica (così le modifiche si vedono senza riavvio).
/// </summary>
public class MarkdownGuideContentProvider : IGuideContentProvider
{
    private readonly GuideSettings _settings;
    private readonly ILogger<MarkdownGuideContentProvider> _logger;

    private readonly object _lock = new();
    private string? _cacheSignature;
    private List<GuideArticle> _cachedArticles = new();

    public MarkdownGuideContentProvider(
        IOptions<GuideSettings> settings,
        ILogger<MarkdownGuideContentProvider> logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    public Task<GuideIndex> GetIndexAsync(CancellationToken cancellationToken)
    {
        var articles = Load();
        var metas = articles
            .Select(a => new GuideArticleMeta { Slug = a.Slug, Title = a.Title, Section = a.Section, Order = a.Order })
            .ToList();

        var sections = metas
            .GroupBy(m => m.Section)
            .Select(g => new GuideSection
            {
                Title    = g.Key,
                Order    = g.Min(m => m.Order),
                Articles = g.OrderBy(m => m.Order).ToList(),
            })
            .OrderBy(s => s.Order)
            .ToList();

        return Task.FromResult(new GuideIndex { Sections = sections, Articles = metas });
    }

    public Task<IReadOnlyList<GuideArticle>> GetAllArticlesAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<GuideArticle>>(Load());

    public Task<GuideArticle?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => Task.FromResult(Load().FirstOrDefault(a =>
            string.Equals(a.Slug, slug, StringComparison.OrdinalIgnoreCase)));

    // ── Caricamento + cache ──────────────────────────────────────────────────
    private List<GuideArticle> Load()
    {
        var root = ResolveRoot();
        if (!Directory.Exists(root))
        {
            _logger.LogWarning("Cartella guida non trovata: {Root}", root);
            return new List<GuideArticle>();
        }

        var files = Directory.GetFiles(root, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Firma = elenco file + last-write-time: cambia se un file viene aggiunto/modificato.
        var sig = string.Join("|", files.Select(f => $"{f}:{File.GetLastWriteTimeUtc(f).Ticks}"));

        lock (_lock)
        {
            if (sig == _cacheSignature) return _cachedArticles;

            var list = new List<GuideArticle>();
            foreach (var file in files)
            {
                try
                {
                    var raw = File.ReadAllText(file, Encoding.UTF8);
                    var article = Parse(raw, file);
                    if (article != null) list.Add(article);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Impossibile leggere l'articolo guida {File}", file);
                }
            }

            _cachedArticles = list.OrderBy(a => a.Order).ToList();
            _cacheSignature = sig;
            return _cachedArticles;
        }
    }

    private string ResolveRoot()
    {
        var configured = _settings.ContentRoot;
        if (string.IsNullOrWhiteSpace(configured))
            configured = "wwwroot/guide";
        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);
    }

    // ── Parsing frontmatter (--- ... ---) ────────────────────────────────────
    private static GuideArticle? Parse(string raw, string filePath)
    {
        string body = raw.Trim();
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (raw.StartsWith("---"))
        {
            var end = raw.IndexOf("\n---", 3, StringComparison.Ordinal);
            if (end > 0)
            {
                var frontmatter = raw.Substring(3, end - 3);
                body = raw.Substring(end + 4).TrimStart('\r', '\n').Trim();

                foreach (var line in frontmatter.Split('\n'))
                {
                    var idx = line.IndexOf(':');
                    if (idx <= 0) continue;
                    var key = line.Substring(0, idx).Trim();
                    var val = line.Substring(idx + 1).Trim().Trim('"', '\'');
                    if (key.Length > 0) meta[key] = val;
                }
            }
        }

        var slug = meta.GetValueOrDefault("slug");
        if (string.IsNullOrWhiteSpace(slug))
        {
            // fallback: nome file senza prefisso numerico ed estensione
            slug = Path.GetFileNameWithoutExtension(filePath);
            var dash = slug.IndexOf('-');
            if (dash > 0 && int.TryParse(slug.Substring(0, dash), out _))
                slug = slug.Substring(dash + 1);
        }

        int.TryParse(meta.GetValueOrDefault("order"), out var order);

        return new GuideArticle
        {
            Slug    = slug,
            Title   = meta.GetValueOrDefault("title") ?? slug,
            Section = meta.GetValueOrDefault("section") ?? "Guida",
            Order   = order == 0 ? 999 : order,
            Body    = body,
        };
    }
}
