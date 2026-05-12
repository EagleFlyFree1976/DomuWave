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

    public GetRealEstateUnitByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
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

        return unit?.ToReadDto();
    }
}
