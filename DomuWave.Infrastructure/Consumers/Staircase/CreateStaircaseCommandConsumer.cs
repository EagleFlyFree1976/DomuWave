using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Staircase;
using DomuWave.Services.Dto.Staircase;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateStaircaseCommandConsumer : InMemoryConsumerBase<CreateStaircaseCommand, StaircaseReadDto>
{
    private readonly IStaircaseService    _staircaseService;
    private readonly ICondominiumService  _condominiumService;
    private readonly IBuildingService     _buildingService;
    private readonly IUserService         _userService;

    public CreateStaircaseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IStaircaseService staircaseService,
        ICondominiumService condominiumService,
        IBuildingService buildingService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _staircaseService   = staircaseService;
        _condominiumService = condominiumService;
        _buildingService    = buildingService;
        _userService        = userService;
    }

    protected override async Task<StaircaseReadDto> Consume(
        CreateStaircaseCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome della scala è obbligatorio.");

        var condominium = await _condominiumService
            .GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

        Building? building = null;
        if (command.Dto.BuildingId.HasValue)
        {
            building = await _buildingService
                .GetByIdAsync(command.Dto.BuildingId.Value, currentUser, cancellationToken)
                .ConfigureAwait(false);
            if (building == null)
                throw new NotFoundException("Edificio non trovato.");
        }

        var duplicate = await session.Query<Staircase>()
            .AnyAsync(x => x.Condominium.Id == command.Dto.CondominiumId
                        && x.Name == command.Dto.Name.Trim()
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (duplicate)
            throw new ValidatorException($"Esiste già una scala '{command.Dto.Name}' in questo condominio.");

        var entity = command.Dto.ToEntity(condominium, building, condominium.Tenant);
        entity.Trace(currentUser);

        var created = await _staircaseService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
