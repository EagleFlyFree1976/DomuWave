using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.SupplierContract;
using DomuWave.Services.Dto.SupplierContract;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.SupplierContract;

public class GetExpiringContractsByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetExpiringContractsByCondominiumCommand, IList<SupplierContractReadDto>>
{
    private readonly ISupplierContractService _contractService;
    private readonly IUserService             _userService;

    public GetExpiringContractsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierContractService contractService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _contractService = contractService;
        _userService     = userService;
    }

    protected override async Task<IList<SupplierContractReadDto>> Consume(
        GetExpiringContractsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var contracts   = await _contractService.GetExpiringContractsAsync(command.CondominiumId, command.DaysAhead, currentUser, cancellationToken).ConfigureAwait(false);
        return contracts.Select(c => c.ToReadDto()).ToList();
    }
}
