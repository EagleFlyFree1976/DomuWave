using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class UpdateQuoteCommandConsumer
    : InMemoryConsumerBase<UpdateQuoteCommand, WorkQuoteReadDto>
{
    private readonly IWorkQuoteService _quoteService;
    private readonly ISupplierService  _supplierService;
    private readonly IUserService      _userService;

    public UpdateQuoteCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IWorkQuoteService quoteService,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    { _quoteService = quoteService; _supplierService = supplierService; _userService = userService; }

    protected override async Task<WorkQuoteReadDto> Consume(
        UpdateQuoteCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity = await _quoteService.GetByIdAsync(command.QuoteId, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return null;

        Services.Models.Supplier? supplier = null;
        if (command.Dto.SupplierId.HasValue)
        {
            supplier = await _supplierService.GetByIdAsync(command.Dto.SupplierId.Value, currentUser, cancellationToken).ConfigureAwait(false);
            if (supplier == null)
                throw new NotFoundException("Fornitore non trovato");
        }

        entity.ApplyUpdate(command.Dto, supplier);
        var updated = await _quoteService.UpdateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);
        return updated.ToReadDto();
    }
}
