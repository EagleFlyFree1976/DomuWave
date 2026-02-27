using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Supplier;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SearchSuppliersCommandConsumer : InMemoryConsumerBase<SearchSuppliersCommand, IList<Supplier>>
{
    private readonly ISupplierService _supplierService;
    private readonly IUserService _userService;

    public SearchSuppliersCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _supplierService = supplierService;
        _userService = userService;
    }

    protected override async Task<IList<Supplier>> Consume(
        SearchSuppliersCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _supplierService
            .SearchSuppliersAsync(command.TenantId, command.Query, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
