using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class GetTenantByIdCommandConsumer
    : InMemoryConsumerBase<GetTenantByIdCommand, TenantReadDto>
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public GetTenantByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<TenantReadDto> Consume(
        GetTenantByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return tenant?.ToReadDto();
    }
}
