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

public class CreateQuoteCommandConsumer
    : InMemoryConsumerBase<CreateQuoteCommand, WorkQuoteReadDto>
{
    private readonly IWorkQuoteService         _quoteService;
    private readonly IExtraordinaryWorkService _workService;
    private readonly ISupplierService          _supplierService;
    private readonly IUserService              _userService;

    public CreateQuoteCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IWorkQuoteService quoteService,
        IExtraordinaryWorkService workService,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    { _quoteService = quoteService; _workService = workService; _supplierService = supplierService; _userService = userService; }

    protected override async Task<WorkQuoteReadDto> Consume(
        CreateQuoteCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var work = await _workService.GetByIdAsync(command.Dto.WorkId, currentUser, cancellationToken).ConfigureAwait(false);
        if (work == null)
            throw new NotFoundException("Lavoro straordinario non trovato");

        Services.Models.Supplier supplier = null;
        if (command.Dto.SupplierId.HasValue)
        {
            supplier = await _supplierService.GetByIdAsync(command.Dto.SupplierId.Value, currentUser, cancellationToken).ConfigureAwait(false);
            if (supplier == null)
                throw new NotFoundException("Fornitore non trovato");
        }

        var entity  = command.Dto.ToEntity(work, supplier);
        var created = await _quoteService.CreateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);
        return created.ToReadDto();
    }
}
