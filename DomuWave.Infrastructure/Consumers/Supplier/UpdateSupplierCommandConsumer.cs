using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Supplier;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateSupplierCommandConsumer : InMemoryConsumerBase<UpdateSupplierCommand, Supplier>
{
    private readonly ISupplierService _supplierService;
    private readonly IUserService _userService;

    public UpdateSupplierCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _supplierService = supplierService;
        _userService = userService;
    }

    protected override async Task<Supplier> Consume(
        UpdateSupplierCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var exists = await _supplierService
            .ExistsAsync(command.SupplierId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (!exists) return null;
        command.Entity.Id = command.SupplierId;
        return await _supplierService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
