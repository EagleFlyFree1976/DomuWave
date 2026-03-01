using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Supplier;
using DomuWave.Services.Dto.Supplier;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateSupplierCommandConsumer : InMemoryConsumerBase<CreateSupplierCommand, SupplierReadDto>
{
    private readonly ISupplierService _supplierService;
    private readonly ITenantService   _tenantService;
    private readonly IUserService     _userService;

    public CreateSupplierCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierService supplierService,
        ITenantService   tenantService,
        IUserService     userService) : base(sessionFactoryProvider)
    {
        _supplierService = supplierService;
        _tenantService   = tenantService;
        _userService     = userService;
    }

    protected override async Task<SupplierReadDto> Consume(
        CreateSupplierCommand command,
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
            throw new NotFoundException("Dati non validi");

        if (string.IsNullOrWhiteSpace(command.Dto.CompanyName))
            throw new ValidatorException("La ragione sociale è obbligatoria");

        var entity  = command.Dto.ToEntity(tenant);
        var created = await _supplierService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
