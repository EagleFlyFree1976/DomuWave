using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.SupplierContract;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.SupplierContract;

public class DeleteSupplierContractCommandConsumer
    : InMemoryConsumerBase<DeleteSupplierContractCommand, bool>
{
    private readonly ISupplierContractService _contractService;
    private readonly IUserService             _userService;

    public DeleteSupplierContractCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierContractService contractService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _contractService = contractService;
        _userService     = userService;
    }

    protected override async Task<bool> Consume(
        DeleteSupplierContractCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _contractService.DeleteAsync(command.ContractId, currentUser, cancellationToken).ConfigureAwait(false);
    }
}
