using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using System.Linq;

namespace DomuWave.Services.Consumers;

public class DeleteUnitOwnerCommandConsumer : InMemoryConsumerBase<DeleteUnitOwnerCommand, UnitOwnerReadDto>
{
    private readonly IUnitOwnerService      _unitOwnerService;
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;

    public DeleteUnitOwnerCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitOwnerService unitOwnerService,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _unitOwnerService      = unitOwnerService;
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
    }

    protected override async Task<UnitOwnerReadDto> Consume(
        DeleteUnitOwnerCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var owner = await _unitOwnerService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (owner == null)
            throw new NotFoundException("Proprietario non trovato");

        var unitId = owner.Unit.Id;

        var hasLinkedRecords =
            await session.Query<ExpenseAllocation>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false) ||
            await session.Query<CondominiumFee>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false)     ||
            await session.Query<Receipt>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false)            ||
            await session.Query<Payment>().AnyAsync(x => x.Unit.Id == unitId, cancellationToken).ConfigureAwait(false);

        if (hasLinkedRecords)
        {
            owner.IsActive = false;
            owner.EndDate  = DateTime.Today;
            var updated = await _unitOwnerService
                .UpdateAsync(owner, currentUser, cancellationToken)
                .ConfigureAwait(false);
            return updated.ToReadDto();
        }

        await _unitOwnerService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Ricalcola DisplayName rimanente dopo rimozione proprietario
        await RefreshUnitDisplayName(unitId, currentUser, cancellationToken).ConfigureAwait(false);

        return null;
    }

    private async Task RefreshUnitDisplayName(int unitId, CPQ.Core.Memberships.IUser currentUser, CancellationToken ct)
    {
        var unit = await _realEstateUnitService.GetByIdAsync(unitId, currentUser, ct).ConfigureAwait(false);
        if (unit == null) return;

        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Id == unitId && o.IsActive && !o.IsDeleted)
            .OrderBy(o => o.LastName)
            .ToListAsync(ct).ConfigureAwait(false);

        if (owners.Any())
        {
            var displayName = string.Join(" / ", owners
                .Select(o => string.IsNullOrWhiteSpace(o.LastName) ? o.FirstName : o.LastName)
                .Where(n => !string.IsNullOrWhiteSpace(n)));
            unit.RefreshDisplayName(displayName);
        }

        await _realEstateUnitService.UpdateAsync(unit, currentUser, ct).ConfigureAwait(false);
    }
}
