using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Supplier;
using DomuWave.Services.Dto.Supplier;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetSupplierByIdCommandConsumer : InMemoryConsumerBase<GetSupplierByIdCommand, SupplierReadDto>
{
    private readonly ISupplierService _supplierService;
    private readonly IUserService _userService;

    public GetSupplierByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _supplierService = supplierService;
        _userService     = userService;
    }

    protected override async Task<SupplierReadDto> Consume(
        GetSupplierByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var supplier = await _supplierService
            .GetByIdAsync(command.SupplierId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return supplier?.ToReadDto();
    }
}
