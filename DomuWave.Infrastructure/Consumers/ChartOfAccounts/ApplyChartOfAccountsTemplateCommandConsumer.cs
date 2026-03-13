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

        foreach (var (code, name, type, parentCode) in ChartOfAccountsDefaultTemplate.Items)
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
