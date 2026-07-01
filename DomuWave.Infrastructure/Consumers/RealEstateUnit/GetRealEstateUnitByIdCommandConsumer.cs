using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetRealEstateUnitByIdCommandConsumer : InMemoryConsumerBase<GetRealEstateUnitByIdCommand, RealEstateUnitReadDto>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;
    private readonly IUserTenantService     _userTenantService;

    public GetRealEstateUnitByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService,
        IUserTenantService userTenantService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
        _userTenantService     = userTenantService;
    }

    protected override async Task<RealEstateUnitReadDto> Consume(
        GetRealEstateUnitByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var unit = await _realEstateUnitService
            .GetByIdAsync(command.UnitId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (unit == null) return null;

        // Condòmino NEL TENANT ATTIVO: può vedere solo le proprie unità.
        var isCondomino = await _userTenantService
            .IsCondominoInTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);
        if (isCondomino)
        {
            var ownUnitIds = await _realEstateUnitService
                .GetCondominoUnitIdsAsync(command.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);
            if (!ownUnitIds.Contains(unit.Id)) return null;
        }

        return unit.ToReadDto();
    }
}
