using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetUnitTenantByIdCommandConsumer : InMemoryConsumerBase<GetUnitTenantByIdCommand, UnitTenantReadDto>
{
    private readonly IUnitTenantService _unitTenantService;
    private readonly IUserService       _userService;

    public GetUnitTenantByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitTenantService unitTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _unitTenantService = unitTenantService;
        _userService       = userService;
    }

    protected override async Task<UnitTenantReadDto> Consume(
        GetUnitTenantByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _unitTenantService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return tenant?.ToReadDto();
    }
}
