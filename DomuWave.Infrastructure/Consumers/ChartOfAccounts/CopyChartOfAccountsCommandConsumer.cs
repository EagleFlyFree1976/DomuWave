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

public class CopyChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<CopyChartOfAccountsCommand, int>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly ICondominiumService     _condominiumService;
    private readonly IUserService            _userService;

    public CopyChartOfAccountsCommandConsumer(
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
        CopyChartOfAccountsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var target = await _condominiumService
            .GetByIdAsync(command.TargetCondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (target == null)
            throw new NotFoundException("Condominio di destinazione non trovato.");

        var sourceExists = await session.Query<Models.Condominium>()
            .AnyAsync(x => x.Id == command.SourceCondominiumId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (!sourceExists)
            throw new NotFoundException("Condominio sorgente non trovato.");

        // Codici già presenti nel condominio di destinazione
        var existingCodes = await session.Query<ChartOfAccounts>()
            .Where(x => x.Condominium.Id == command.TargetCondominiumId && !x.IsDeleted)
            .Select(x => x.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Tutti i conti sorgente, ordinati per livello (padre prima del figlio)
        var sourceAccounts = await session.Query<ChartOfAccounts>()
            .Where(x => x.Condominium.Id == command.SourceCondominiumId && !x.IsDeleted)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // oldId → nuovo ChartOfAccounts creato nel target (per ricostruire la gerarchia)
        var idMap = new Dictionary<int, ChartOfAccounts>();
        int copied = 0;

        foreach (var src in sourceAccounts)
        {
            if (existingCodes.Contains(src.Code))
                continue;

            ChartOfAccounts? parentInTarget = null;
            if (src.ParentAccount != null && idMap.TryGetValue(src.ParentAccount.Id, out var mappedParent))
                parentInTarget = mappedParent;

            var newAccount = new ChartOfAccounts
            {
                Condominium       = target,
                Tenant            = target.Tenant,
                ParentAccount     = parentInTarget,
                Code              = src.Code,
                Name              = src.Name,
                Description       = src.Description,
                Type              = src.Type,
                ChargeabilityType = src.ChargeabilityType,
                Category          = null,
                Level             = parentInTarget != null ? parentInTarget.Level + 1 : 1,
                IsActive          = src.IsActive,
                IsDeleted         = false,
            };

            newAccount.Trace(currentUser);

            var created = await _accountService
                .CreateAsync(newAccount, currentUser, cancellationToken)
                .ConfigureAwait(false);

            idMap[src.Id] = created;
            existingCodes.Add(src.Code);
            copied++;
        }

        return copied;
    }
}
