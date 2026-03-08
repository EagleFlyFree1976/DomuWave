using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Applica un piano dei conti standard italiano al condominio indicato.
/// Salta i codici già esistenti, preservando personalizzazioni precedenti.
/// </summary>
public class ApplyChartOfAccountsTemplateCommandConsumer
    : InMemoryConsumerBase<ApplyChartOfAccountsTemplateCommand, int>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly ICondominiumService     _condominiumService;
    private readonly IUserService            _userService;

    public ApplyChartOfAccountsTemplateCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IChartOfAccountsService  accountService,
        ICondominiumService      condominiumService,
        IUserService             userService) : base(sessionFactoryProvider)
    {
        _accountService     = accountService;
        _condominiumService = condominiumService;
        _userService        = userService;
    }

    // ── Template: (code, name, type, parentCode) ──────────────────────────────
    private static readonly (string Code, string Name, ChartOfAccountsType Type, string? Parent)[] Template =
    [
        // ── USCITE (spese) ────────────────────────────────────────────────────
        ("U",    "Uscite",                                  ChartOfAccountsType.Uscita, null),
        ("U.01", "Pulizie e igiene",                        ChartOfAccountsType.Uscita, "U"),
        ("U.02", "Giardinaggio e aree verdi",               ChartOfAccountsType.Uscita, "U"),
        ("U.03", "Manutenzione ordinaria",                  ChartOfAccountsType.Uscita, "U"),
        ("U.04", "Manutenzione straordinaria",              ChartOfAccountsType.Uscita, "U"),
        ("U.05", "Ascensore",                               ChartOfAccountsType.Uscita, "U"),
        ("U.06", "Riscaldamento centralizzato",             ChartOfAccountsType.Uscita, "U"),
        ("U.07", "Acqua comune",                            ChartOfAccountsType.Uscita, "U"),
        ("U.08", "Energia elettrica parti comuni",          ChartOfAccountsType.Uscita, "U"),
        ("U.09", "Gas parti comuni",                        ChartOfAccountsType.Uscita, "U"),
        ("U.10", "Portineria e vigilanza",                  ChartOfAccountsType.Uscita, "U"),
        ("U.11", "Assicurazioni",                           ChartOfAccountsType.Uscita, "U"),
        ("U.12", "Onorario amministratore",                 ChartOfAccountsType.Uscita, "U"),
        ("U.13", "Spese legali e consulenze",               ChartOfAccountsType.Uscita, "U"),
        ("U.14", "Spese bancarie e postali",                ChartOfAccountsType.Uscita, "U"),
        ("U.15", "Imposte e tasse",                         ChartOfAccountsType.Uscita, "U"),
        ("U.16", "Fornitura materiali",                     ChartOfAccountsType.Uscita, "U"),
        ("U.17", "Lavori strutturali",                      ChartOfAccountsType.Uscita, "U"),
        ("U.18", "Impianti tecnologici",                    ChartOfAccountsType.Uscita, "U"),
        ("U.19", "Fondo di riserva",                        ChartOfAccountsType.Uscita, "U"),
        ("U.20", "Spese varie",                             ChartOfAccountsType.Uscita, "U"),

        // ── ENTRATE ───────────────────────────────────────────────────────────
        ("E",    "Entrate",                                 ChartOfAccountsType.Entrata, null),
        ("E.01", "Quote condominiali ordinarie",            ChartOfAccountsType.Entrata, "E"),
        ("E.02", "Quote straordinarie",                     ChartOfAccountsType.Entrata, "E"),
        ("E.03", "Interessi attivi",                        ChartOfAccountsType.Entrata, "E"),
        ("E.04", "Proventi da locazione spazi comuni",      ChartOfAccountsType.Entrata, "E"),
        ("E.05", "Rimborsi assicurativi",                   ChartOfAccountsType.Entrata, "E"),
        ("E.06", "Entrate varie",                           ChartOfAccountsType.Entrata, "E"),

        // ── PATRIMONIALE ──────────────────────────────────────────────────────
        ("P",    "Patrimoniale",                            ChartOfAccountsType.Patrimoniale, null),
        ("P.01", "Fondo cassa",                             ChartOfAccountsType.Patrimoniale, "P"),
        ("P.02", "Conto corrente bancario",                 ChartOfAccountsType.Patrimoniale, "P"),
        ("P.03", "Crediti verso condomini",                 ChartOfAccountsType.Patrimoniale, "P"),
        ("P.04", "Debiti verso fornitori",                  ChartOfAccountsType.Patrimoniale, "P"),
        ("P.05", "Fondo di riserva (patrimonio)",           ChartOfAccountsType.Patrimoniale, "P"),
    ];

    protected override async Task<int> Consume(
        ApplyChartOfAccountsTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var condominium = await _condominiumService
            .GetByIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

        // Codici già presenti → skip
        var existingCodes = await session.Query<ChartOfAccounts>()
            .Where(x => x.Condominium.Id == command.CondominiumId && !x.IsDeleted)
            .Select(x => x.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // code → created entity (per risolvere le relazioni padre-figlio)
        var codeMap = new Dictionary<string, ChartOfAccounts>(StringComparer.OrdinalIgnoreCase);
        int created = 0;

        foreach (var (code, name, type, parentCode) in Template)
        {
            if (existingCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
                continue;

            ChartOfAccounts? parent = null;
            if (parentCode != null)
                codeMap.TryGetValue(parentCode, out parent);

            var entity = new ChartOfAccounts
            {
                Condominium   = condominium,
                Tenant        = condominium.Tenant,
                ParentAccount = parent,
                Code          = code,
                Name          = name,
                Type          = type,
                Level         = parent != null ? parent.Level + 1 : 1,
                IsActive      = true,
                IsDeleted     = false,
            };
            entity.Trace(currentUser);

            var saved = await _accountService
                .CreateAsync(entity, currentUser, cancellationToken)
                .ConfigureAwait(false);

            codeMap[code] = saved;
            existingCodes.Add(code);
            created++;
        }

        return created;
    }
}
