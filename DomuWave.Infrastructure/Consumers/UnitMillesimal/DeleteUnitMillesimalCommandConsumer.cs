using CPQ.Core.Consumers;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteUnitMillesimalCommandConsumer
    : InMemoryConsumerBase<DeleteUnitMillesimalCommand, bool>
{
    private readonly IUnitMillesimalService _unitMillesimalService;
    private readonly IUserService           _userService;

    public DeleteUnitMillesimalCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitMillesimalService  unitMillesimalService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _unitMillesimalService = unitMillesimalService;
        _userService           = userService;
    }

    protected override async Task<bool> Consume(
        DeleteUnitMillesimalCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Load entry before delete to get the parent table reference
        var entry = await _unitMillesimalService
            .GetByIdAsync(command.EntryId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var table = entry?.MillesimalTable;

        var result = await _unitMillesimalService
            .DeleteAsync(command.EntryId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (result && table != null)
            await UpdateTableStatusAsync(table, currentUser, cancellationToken).ConfigureAwait(false);

        return result;
    }

    private async Task UpdateTableStatusAsync(MillesimalTable table, IUser currentUser, CancellationToken cancellationToken)
    {
        var sum = await session.Query<UnitMillesimal>()
            .Where(x => x.MillesimalTable.Id == table.Id && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.Millesimal, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var shouldBeActive = sum == table.TotalMillesimal && table.TotalMillesimal > 0;
        if (table.IsActive != shouldBeActive || table.IsDraft != !shouldBeActive)
        {
            table.IsActive = shouldBeActive;
            table.IsDraft  = !shouldBeActive;
            await session.SaveOrUpdateAsync(table, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
