using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateUnitOwnerCommandConsumer : InMemoryConsumerBase<CreateUnitOwnerCommand, UnitOwnerReadDto>
{
    private readonly IUnitOwnerService       _unitOwnerService;
    private readonly IRealEstateUnitService  _realEstateUnitService;
    private readonly IUserService            _userService;

    public CreateUnitOwnerCommandConsumer(
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
        CreateUnitOwnerCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var unit = await _realEstateUnitService
            .GetByIdAsync(command.Dto.UnitId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (unit == null)
            throw new NotFoundException("Unità immobiliare non trovata");

        if (command.Dto.UserId <= 0)
            throw new ValidatorException("Specificare un utente condomino valido");

        var entity  = command.Dto.ToEntity(unit, unit.Tenant);
        var created = await _unitOwnerService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
