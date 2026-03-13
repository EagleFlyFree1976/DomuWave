using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

/// <summary>
/// Seeds the standard Italian chart-of-accounts template for a tenant (idempotent).
/// </summary>
public class ChartOfAccountsTemplateSeedService : BaseService, IChartOfAccountsTemplateSeedService
{
    private const string TemplateName = "Piano dei Conti Standard Italiano";

    // (code, name, type, parentCode, sortOrder)
    private static readonly (string Code, string Name, ChartOfAccountsType Type, string? Parent, int Sort)[] DefaultItems =
    [
        // ── USCITE ───────────────────────────────────────────────────────────
        ("U",    "Uscite",                             ChartOfAccountsType.Uscita,       null,  10),
        ("U.01", "Pulizie e igiene",                   ChartOfAccountsType.Uscita,       "U",   20),
        ("U.02", "Giardinaggio e aree verdi",          ChartOfAccountsType.Uscita,       "U",   30),
        ("U.03", "Manutenzione ordinaria",             ChartOfAccountsType.Uscita,       "U",   40),
        ("U.04", "Manutenzione straordinaria",         ChartOfAccountsType.Uscita,       "U",   50),
        ("U.05", "Ascensore",                          ChartOfAccountsType.Uscita,       "U",   60),
        ("U.06", "Riscaldamento centralizzato",        ChartOfAccountsType.Uscita,       "U",   70),
        ("U.07", "Acqua comune",                       ChartOfAccountsType.Uscita,       "U",   80),
        ("U.08", "Energia elettrica parti comuni",     ChartOfAccountsType.Uscita,       "U",   90),
        ("U.09", "Gas parti comuni",                   ChartOfAccountsType.Uscita,       "U",  100),
        ("U.10", "Portineria e vigilanza",             ChartOfAccountsType.Uscita,       "U",  110),
        ("U.11", "Assicurazioni",                      ChartOfAccountsType.Uscita,       "U",  120),
        ("U.12", "Onorario amministratore",            ChartOfAccountsType.Uscita,       "U",  130),
        ("U.13", "Spese legali e consulenze",          ChartOfAccountsType.Uscita,       "U",  140),
        ("U.14", "Spese bancarie e postali",           ChartOfAccountsType.Uscita,       "U",  150),
        ("U.15", "Imposte e tasse",                    ChartOfAccountsType.Uscita,       "U",  160),
        ("U.16", "Fornitura materiali",                ChartOfAccountsType.Uscita,       "U",  170),
        ("U.17", "Lavori strutturali",                 ChartOfAccountsType.Uscita,       "U",  180),
        ("U.18", "Impianti tecnologici",               ChartOfAccountsType.Uscita,       "U",  190),
        ("U.19", "Fondo di riserva",                   ChartOfAccountsType.Uscita,       "U",  200),
        ("U.20", "Spese varie",                        ChartOfAccountsType.Uscita,       "U",  210),

        // ── ENTRATE ──────────────────────────────────────────────────────────
        ("E",    "Entrate",                            ChartOfAccountsType.Entrata,      null,  300),
        ("E.01", "Quote condominiali ordinarie",       ChartOfAccountsType.Entrata,      "E",   310),
        ("E.02", "Quote straordinarie",                ChartOfAccountsType.Entrata,      "E",   320),
        ("E.03", "Interessi attivi",                   ChartOfAccountsType.Entrata,      "E",   330),
        ("E.04", "Proventi da locazione spazi comuni", ChartOfAccountsType.Entrata,      "E",   340),
        ("E.05", "Rimborsi assicurativi",              ChartOfAccountsType.Entrata,      "E",   350),
        ("E.06", "Entrate varie",                      ChartOfAccountsType.Entrata,      "E",   360),

        // ── PATRIMONIALE ─────────────────────────────────────────────────────
        ("P",    "Patrimoniale",                       ChartOfAccountsType.Patrimoniale, null,  500),
        ("P.01", "Fondo cassa",                        ChartOfAccountsType.Patrimoniale, "P",   510),
        ("P.02", "Conto corrente bancario",            ChartOfAccountsType.Patrimoniale, "P",   520),
        ("P.03", "Crediti verso condomini",            ChartOfAccountsType.Patrimoniale, "P",   530),
        ("P.04", "Debiti verso fornitori",             ChartOfAccountsType.Patrimoniale, "P",   540),
        ("P.05", "Fondo di riserva (patrimonio)",      ChartOfAccountsType.Patrimoniale, "P",   550),
    ];

    public ChartOfAccountsTemplateSeedService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "ChartOfAccountsTemplateSeed";

    public async Task SeedAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        // Idempotent: skip if template already exists for this tenant
        var exists = await session.Query<ChartOfAccountsTemplate>()
            .AnyAsync(t => t.Tenant.Id == tenant.Id
                        && t.Name == TemplateName
                        && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (exists) return;

        var template = new ChartOfAccountsTemplate
        {
            Tenant      = tenant,
            Name        = TemplateName,
            Description = "Template standard con le voci più comuni per la gestione condominiale italiana.",
            IsActive    = true,
            IsDefault   = true,
            IsDeleted   = false,
        };

        // Use a system-level trace (tenant owner, no real user available)
        template.CreatedBy         = 1;
        template.CreatedByFullName = "Sistema";
        template.CreationDate      = DateTime.UtcNow;

        await session.SaveAsync(template, cancellationToken).ConfigureAwait(false);

        // code → saved item (to resolve parent FK)
        var codeMap = new Dictionary<string, ChartOfAccountsTemplateItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var (code, name, type, parentCode, sort) in DefaultItems)
        {
            ChartOfAccountsTemplateItem? parent = null;
            if (parentCode != null) codeMap.TryGetValue(parentCode, out parent);

            var item = new ChartOfAccountsTemplateItem
            {
                Template    = template,
                ParentItem  = parent,
                Tenant      = tenant,
                Code        = code,
                Name        = name,
                Type        = type,
                SortOrder   = sort,
                IsActive    = true,
                IsDeleted   = false,
            };

            item.CreatedBy         = 1;
            item.CreatedByFullName = "Sistema";
            item.CreationDate      = DateTime.UtcNow;

            await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);
            codeMap[code] = item;
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
