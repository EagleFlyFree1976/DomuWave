using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteUnitTenantCommandConsumer : InMemoryConsumerBase<DeleteUnitTenantCommand, UnitTenantReadDto>
{
    private readonly IUnitTenantService _unitTenantService;
    private readonly IUserService       _userService;

    public DeleteUnitTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitTenantService unitTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _unitTenantService = unitTenantService;
        _userService       = userService;
    }

    protected override async Task<UnitTenantReadDto> Consume(
        DeleteUnitTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _unitTenantService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
            throw new NotFoundException("Inquilino non trovato");

        var unitId = tenant.Unit.Id;

        var hasLinkedRecords =
            await session.Query<ExpenseAllocation>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false) ||
            await session.Query<CondominiumFee>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false)     ||
            await session.Query<Receipt>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false)            ||
            await session.Query<Payment>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false);

        if (hasLinkedRecords)
        {
            tenant.IsActive      = false;
            tenant.LeaseEndDate  = DateTime.Today;
            var updated = await _unitTenantService
                .UpdateAsync(tenant, currentUser, cancellationToken)
                .ConfigureAwait(false);
            return updated.ToReadDto();
        }

        await _unitTenantService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return null;
    }
}
