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

public class DeleteChartOfAccountsTemplateCommandConsumer
    : InMemoryConsumerBase<DeleteChartOfAccountsTemplateCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteChartOfAccountsTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteChartOfAccountsTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<ChartOfAccountsTemplate>()
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Template non trovato.");

        // Soft-delete all items first
        await NHibernate.NHibernateUtil.InitializeAsync(entity.Items, cancellationToken).ConfigureAwait(false);
        foreach (var item in entity.Items.Where(i => !i.IsDeleted))
        {
            item.IsDeleted = true;
            item.Trace(currentUser);
            await session.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
        }

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
