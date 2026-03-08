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

public class GetChartOfAccountsTemplateItemsCommandConsumer
    : InMemoryConsumerBase<GetChartOfAccountsTemplateItemsCommand, IList<ChartOfAccountsTemplateItemReadDto>>
{
    private readonly IUserService _userService;

    public GetChartOfAccountsTemplateItemsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<ChartOfAccountsTemplateItemReadDto>> Consume(
        GetChartOfAccountsTemplateItemsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var items = await session.Query<ChartOfAccountsTemplateItem>()
            .Where(i => i.Template.Id == command.TemplateId && !i.IsDeleted)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Code)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return items.Select(i => i.ToReadDto()).ToList();
    }
}
