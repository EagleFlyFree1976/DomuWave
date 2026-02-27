using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class UpdateTenantCommandConsumer
    : InMemoryConsumerBase<UpdateTenantCommand, TenantReadDto>
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public UpdateTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<TenantReadDto> Consume(
        UpdateTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existingByCode = await _tenantService
            .GetByCodeAsync(command.Code, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (existingByCode != null && existingByCode.Id != command.TenantId)
            throw new ValidatorException($"Esiste già un tenant con il codice '{command.Code}'.");


        var existing = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Restituisce null → il controller risponderà 404
        if (existing == null) return null;






        existing.Name     = command.Name;
        existing.Code     = command.Code;
        existing.IsActive = command.IsActive;

        var updated = await _tenantService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
