using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteChartOfAccountsTemplateItemCommandConsumer
    : InMemoryConsumerBase<DeleteChartOfAccountsTemplateItemCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteChartOfAccountsTemplateItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteChartOfAccountsTemplateItemCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<ChartOfAccountsTemplateItem>()
            .FirstOrDefaultAsync(i => i.Id == command.Id && !i.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Voce non trovata.");

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
