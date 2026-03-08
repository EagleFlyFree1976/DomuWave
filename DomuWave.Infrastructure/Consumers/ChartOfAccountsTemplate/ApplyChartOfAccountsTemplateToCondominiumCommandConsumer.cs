using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class ApplyChartOfAccountsTemplateToCondominiumCommandConsumer
    : InMemoryConsumerBase<ApplyChartOfAccountsTemplateToCondominiumCommand, int>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly ICondominiumService     _condominiumService;
    private readonly IUserService            _userService;

    public ApplyChartOfAccountsTemplateToCondominiumCommandConsumer(
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
        ApplyChartOfAccountsTemplateToCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await _condominiumService
            .GetByIdAsync(command.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

        var template = await session.Query<ChartOfAccountsTemplate>()
            .FirstOrDefaultAsync(t => t.Id == command.TemplateId && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (template == null)
            throw new NotFoundException("Template non trovato.");

        // Load all items ordered by SortOrder so parents come before children
        var items = await session.Query<ChartOfAccountsTemplateItem>()
            .Where(i => i.Template.Id == command.TemplateId && !i.IsDeleted && i.IsActive)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Code)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Existing codes → skip duplicates
        var existingCodes = await session.Query<ChartOfAccounts>()
            .Where(x => x.Condominium.Id == command.CondominiumId && !x.IsDeleted)
            .Select(x => x.Code)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // templateItemId → created ChartOfAccounts (to resolve parent references)
        var idMap  = new Dictionary<int, ChartOfAccounts>();
        int created = 0;

        foreach (var item in items)
        {
            if (existingCodes.Contains(item.Code, StringComparer.OrdinalIgnoreCase))
                continue;

            ChartOfAccounts? parent = null;
            if (item.ParentItem != null && idMap.TryGetValue(item.ParentItem.Id, out var parentAccount))
                parent = parentAccount;

            var entity = new ChartOfAccounts
            {
                Condominium   = condominium,
                Tenant        = condominium.Tenant,
                ParentAccount = parent,
                Code          = item.Code,
                Name          = item.Name,
                Type          = item.Type,
                Level         = parent != null ? parent.Level + 1 : 1,
                IsActive      = true,
                IsDeleted     = false,
            };
            entity.Trace(currentUser);

            var saved = await _accountService
                .CreateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);

            idMap[item.Id] = saved;
            existingCodes.Add(item.Code);
            created++;
        }

        return created;
    }
}
