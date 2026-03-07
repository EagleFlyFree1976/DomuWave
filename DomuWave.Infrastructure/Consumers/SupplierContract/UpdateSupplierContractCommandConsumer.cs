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

public class UpdateSupplierContractCommandConsumer
    : InMemoryConsumerBase<UpdateSupplierContractCommand, SupplierContractReadDto>
{
    private readonly ISupplierContractService _contractService;
    private readonly IUserService             _userService;

    public UpdateSupplierContractCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ISupplierContractService contractService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _contractService = contractService;
        _userService     = userService;
    }

    protected override async Task<SupplierContractReadDto> Consume(
        UpdateSupplierContractCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await _contractService.GetByIdAsync(command.ContractId, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return null;

        if (string.IsNullOrWhiteSpace(command.Dto.Subject))
            throw new ValidatorException("L'oggetto del contratto è obbligatorio");

        entity.ApplyUpdate(command.Dto);
        var updated = await _contractService.UpdateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
