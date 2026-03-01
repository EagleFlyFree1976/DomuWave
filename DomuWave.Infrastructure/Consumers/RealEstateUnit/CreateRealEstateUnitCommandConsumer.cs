using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateRealEstateUnitCommandConsumer : InMemoryConsumerBase<CreateRealEstateUnitCommand, RealEstateUnitReadDto>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly ICondominiumService    _condominiumService;
    private readonly IUserService           _userService;

    public CreateRealEstateUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _condominiumService    = condominiumService;
        _userService           = userService;
    }

    protected override async Task<RealEstateUnitReadDto> Consume(
        CreateRealEstateUnitCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var condominium = await _condominiumService
            .GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato");

        /* validazioni */
        if (string.IsNullOrWhiteSpace(command.Dto.InternalNumber))
            throw new ValidatorException("Specificare il numero interno dell'unità");

        var existsNumber = await session.Query<RealEstateUnit>()
            .Where(u => u.InternalNumber == command.Dto.InternalNumber.Trim()
                     && u.Condominium.Id == command.Dto.CondominiumId
                     && !u.IsDeleted)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existsNumber)
            throw new ValidatorException($"Esiste già un'unità con il numero interno {command.Dto.InternalNumber} in questo condominio");

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);

        var created = await _realEstateUnitService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
