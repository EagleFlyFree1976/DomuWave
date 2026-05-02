using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Interfaces;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteMillesimalTableCommandConsumer : InMemoryConsumerBase<DeleteMillesimalTableCommand, bool>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService _userService;

    public DeleteMillesimalTableCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMillesimalTableService millesimalTableService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        DeleteMillesimalTableCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var table = await _millesimalTableService
            .GetByIdAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (table == null) return false;
        if (table.IsDefault)
            throw new ValidatorException("Impossibile eliminare la tabella millesimale predefinita.");

        var usedInAccount = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.DefaultMillesimalTable != null
                        && a.DefaultMillesimalTable.Id == command.TableId
                        && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (usedInAccount)
            throw new ValidatorException("Impossibile eliminare la tabella millesimale: è impostata come predefinita in uno o più conti del piano dei conti. Disabilitarla invece.");

        var usedInExpense = await session.Query<Expense>()
            .AnyAsync(e => e.MillesimalTable != null
                        && e.MillesimalTable.Id == command.TableId
                        && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (usedInExpense)
            throw new ValidatorException("Impossibile eliminare la tabella millesimale: è utilizzata in una o più spese. Disabilitarla invece.");

        return await _millesimalTableService
            .DeleteAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
