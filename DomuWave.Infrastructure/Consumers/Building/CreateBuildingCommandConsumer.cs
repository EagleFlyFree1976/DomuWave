using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Building;
using DomuWave.Services.Dto.Building;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateBuildingCommandConsumer : InMemoryConsumerBase<CreateBuildingCommand, BuildingReadDto>
{
    private readonly IBuildingService    _buildingService;
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService        _userService;

    public CreateBuildingCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBuildingService buildingService,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _buildingService    = buildingService;
        _condominiumService = condominiumService;
        _userService        = userService;
    }

    protected override async Task<BuildingReadDto> Consume(
        CreateBuildingCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome dell'edificio è obbligatorio.");

        var condominium = await _condominiumService
            .GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

        var duplicate = await session.Query<Building>()
            .AnyAsync(x => x.Condominium.Id == command.Dto.CondominiumId
                        && x.Name == command.Dto.Name.Trim()
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (duplicate)
            throw new ValidatorException("Esiste già un edificio con questo nome nel condominio.");

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);
        entity.Trace(currentUser);

        var created = await _buildingService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
