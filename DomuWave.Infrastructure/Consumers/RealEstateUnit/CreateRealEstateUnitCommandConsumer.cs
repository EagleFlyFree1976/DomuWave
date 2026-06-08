using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using LicenseManager.Client.Context;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateRealEstateUnitCommandConsumer : InMemoryConsumerBase<CreateRealEstateUnitCommand, RealEstateUnitReadDto>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly ICondominiumService    _condominiumService;
    private readonly IBuildingService       _buildingService;
    private readonly IStaircaseService      _staircaseService;
    private readonly IUserService           _userService;
    private readonly ILicenseContext        _licenseContext;

    public CreateRealEstateUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        ICondominiumService condominiumService,
        IBuildingService buildingService,
        IStaircaseService staircaseService,
        IUserService userService,
        ILicenseContext licenseContext) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _condominiumService    = condominiumService;
        _buildingService       = buildingService;
        _staircaseService      = staircaseService;
        _userService           = userService;
        _licenseContext        = licenseContext;
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

        // Pre-verifica del plafond UNITS (feature a consumo / Resource): se esaurito, blocca
        // PRIMA di creare. Il consumo effettivo avviene solo a creazione riuscita (in fondo).
        var unitsUsage = _licenseContext.GetUsageSnapshot(FeatureKeys.UNITS);
        if (unitsUsage is { Remaining: <= 0 })
            throw new ValidatorException(
                "Hai raggiunto il numero massimo di unità immobiliari della tua licenza. Acquista altre licenze per aggiungerne.");

        /* validazioni */
        if (!string.IsNullOrWhiteSpace(command.Dto.InternalNumber))
        {
            var existsNumber = await session.Query<RealEstateUnit>()
                .Where(u => u.InternalNumber == command.Dto.InternalNumber.Trim()
                         && u.Condominium.Id == command.Dto.CondominiumId
                         && !u.IsDeleted)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existsNumber)
                throw new ValidatorException($"Esiste già un'unità con il numero interno {command.Dto.InternalNumber} in questo condominio");
        }

        DomuWave.Services.Models.Building building = null;
        if (command.Dto.BuildingId.HasValue)
            building = await _buildingService
                .GetByIdAsync(command.Dto.BuildingId.Value, currentUser, cancellationToken)
                .ConfigureAwait(false);

        DomuWave.Services.Models.Staircase staircase = null;
        if (command.Dto.StaircaseId.HasValue)
            staircase = await _staircaseService
                .GetByIdAsync(command.Dto.StaircaseId.Value, currentUser, cancellationToken)
                .ConfigureAwait(false);

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant, building, staircase);

        var created = await _realEstateUnitService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Creazione riuscita → consuma 1 utilizzo della feature UNITS + sync immediato verso LM.
        var consumeResult = _licenseContext.Consume(FeatureKeys.UNITS, 1);
        if (consumeResult.Allowed)
            _ = _licenseContext.SyncNowAsync(FeatureKeys.UNITS);

        return created.ToReadDto();
    }
}
