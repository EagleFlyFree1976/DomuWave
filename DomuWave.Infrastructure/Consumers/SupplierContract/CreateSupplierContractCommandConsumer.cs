using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.SupplierContract;
using DomuWave.Services.Dto.SupplierContract;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.SupplierContract;

public class CreateSupplierContractCommandConsumer
    : InMemoryConsumerBase<CreateSupplierContractCommand, SupplierContractReadDto>
{
    private readonly ISupplierContractService _contractService;
    private readonly ICondominiumService      _condominiumService;
    private readonly ISupplierService         _supplierService;
    private readonly IUserService             _userService;

    public CreateSupplierContractCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierContractService contractService,
        ICondominiumService condominiumService,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _contractService    = contractService;
        _condominiumService = condominiumService;
        _supplierService    = supplierService;
        _userService        = userService;
    }

    protected override async Task<SupplierContractReadDto> Consume(
        CreateSupplierContractCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Subject))
            throw new ValidatorException("L'oggetto del contratto è obbligatorio");

        var condominium = await _condominiumService.GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato");

        var supplier = await _supplierService.GetByIdAsync(command.Dto.SupplierId, currentUser, cancellationToken).ConfigureAwait(false);
        if (supplier == null)
            throw new NotFoundException("Fornitore non trovato");

        var entity  = command.Dto.ToEntity(condominium, supplier);
        var created = await _contractService.CreateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }
}
