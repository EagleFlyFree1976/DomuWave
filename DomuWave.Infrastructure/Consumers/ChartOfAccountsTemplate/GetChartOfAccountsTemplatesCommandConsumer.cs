using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetChartOfAccountsTemplatesCommandConsumer
    : InMemoryConsumerBase<GetChartOfAccountsTemplatesCommand, IList<ChartOfAccountsTemplateReadDto>>
{
    private readonly IUserService _userService;

    public GetChartOfAccountsTemplatesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<ChartOfAccountsTemplateReadDto>> Consume(
        GetChartOfAccountsTemplatesCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<ChartOfAccountsTemplate>()
            .Where(t => t.Tenant.Id == command.TenantId && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Eager-load items for count
        foreach (var t in list)
            await NHibernate.NHibernateUtil.InitializeAsync(t.Items, cancellationToken).ConfigureAwait(false);

        return list.Select(t => t.ToReadDto()).ToList();
    }
}
