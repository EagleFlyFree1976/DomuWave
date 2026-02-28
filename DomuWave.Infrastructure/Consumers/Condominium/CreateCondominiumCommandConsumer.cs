using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateCondominiumCommandConsumer : InMemoryConsumerBase<CreateCondominiumCommand, CondominiumReadDto>
{
    private readonly ICondominiumService _condominiumService;
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public CreateCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _tenantService      = tenantService;
        _userService        = userService;
    }

    protected override async Task<CondominiumReadDto> Consume(
        CreateCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException($"Dati non validi");
        
        var entity = command.Dto.ToEntity(tenant);
        

        var created = await _condominiumService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
