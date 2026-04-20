using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BillingGroup;
using DomuWave.Services.Dto.BillingGroup;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateBillingGroupCommandConsumer
    : InMemoryConsumerBase<UpdateBillingGroupCommand, BillingGroupReadDto>
{
    private readonly IUserService _userService;

    public UpdateBillingGroupCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<BillingGroupReadDto> Consume(
        UpdateBillingGroupCommand command,
        IMediationContext          mediationContext,
        CancellationToken          cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await session.Query<DomuWave.Services.Models.BillingGroup>()
            .Where(g => g.Id == command.Id && !g.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            throw new NotFoundException("Gruppo di fatturazione non trovato.");

        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);

        // Reassign units: detach all current, then attach new list
        var currentUnits = await session.Query<RealEstateUnit>()
            .Where(u => u.BillingGroup != null && u.BillingGroup.Id == entity.Id && !u.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var unit in currentUnits)
        {
            unit.BillingGroup = null;
            unit.Trace(currentUser);
            await session.UpdateAsync(unit, cancellationToken).ConfigureAwait(false);
        }

        if (command.Dto.UnitIds?.Count > 0)
        {
            var newUnits = await session.Query<RealEstateUnit>()
                .Where(u => command.Dto.UnitIds.Contains(u.Id) && !u.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var unit in newUnits)
            {
                unit.BillingGroup = entity;
                unit.Trace(currentUser);
                await session.UpdateAsync(unit, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        await NHibernate.NHibernateUtil.InitializeAsync(entity.Units, cancellationToken)
            .ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
