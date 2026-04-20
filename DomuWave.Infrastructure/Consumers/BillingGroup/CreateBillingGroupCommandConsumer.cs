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

public class CreateBillingGroupCommandConsumer
    : InMemoryConsumerBase<CreateBillingGroupCommand, BillingGroupReadDto>
{
    private readonly IUserService _userService;

    public CreateBillingGroupCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<BillingGroupReadDto> Consume(
        CreateBillingGroupCommand command,
        IMediationContext          mediationContext,
        CancellationToken          cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome è obbligatorio.");

        var condominium = await session.Query<Models.Condominium>()
            .Where(c => c.Id == command.Dto.CondominiumId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);
        entity.Trace(currentUser);

        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);

        // Assign units
        if (command.Dto.UnitIds?.Count > 0)
        {
            var units = await session.Query<RealEstateUnit>()
                .Where(u => command.Dto.UnitIds.Contains(u.Id) && u.Condominium.Id == command.Dto.CondominiumId && !u.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var unit in units)
            {
                unit.BillingGroup = entity;
                unit.Trace(currentUser);
                await session.UpdateAsync(unit, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        await NHibernate.NHibernateUtil.InitializeAsync(entity.Units, cancellationToken)
            .ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
