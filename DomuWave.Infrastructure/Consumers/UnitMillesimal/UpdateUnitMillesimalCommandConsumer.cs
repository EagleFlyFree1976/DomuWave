using CPQ.Core.Consumers;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateUnitMillesimalCommandConsumer
    : InMemoryConsumerBase<UpdateUnitMillesimalCommand, UnitMillesimalReadDto>
{
    private readonly IUnitMillesimalService _unitMillesimalService;
    private readonly IUserService           _userService;

    public UpdateUnitMillesimalCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitMillesimalService  unitMillesimalService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _unitMillesimalService = unitMillesimalService;
        _userService           = userService;
    }

    protected override async Task<UnitMillesimalReadDto> Consume(
        UpdateUnitMillesimalCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _unitMillesimalService
            .GetByIdAsync(command.EntryId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null) return null;

        var table = entity.MillesimalTable;

        entity.ApplyUpdate(command.Dto);
        var updated = await _unitMillesimalService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        await UpdateTableStatusAsync(table, currentUser, cancellationToken).ConfigureAwait(false);

        return updated.ToReadDto();
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
