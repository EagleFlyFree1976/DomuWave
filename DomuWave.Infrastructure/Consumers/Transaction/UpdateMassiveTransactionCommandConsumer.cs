using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class UpdateMassiveTransactionCommandConsumer : InMemoryConsumerBase<UpdateMassiveTransactionCommand, bool>
{
    private ITransactionService _transactionService;
    private IUserService _userService;
    private IMediator _mediator;
    public UpdateMassiveTransactionCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionService transactionService, IUserService userService, IMediator mediator) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
        _mediator = mediator;
    }

    protected override async Task<bool> Consume(UpdateMassiveTransactionCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);


        var allTransactionToEdit = await _transactionService.GetQueryable()
            .Where(k => !k.IsDeleted && evt.BookId == k.Book.Id && evt.TransactionIds.Contains(k.Id))
            .ToListAsync(cancellationToken);

        foreach (Models.Transaction? transaction in allTransactionToEdit)
        {
            var transactionUpdateDto = transaction.ToUpdateDto();

            if (evt.UpdateDto.UpdateAccountId)
                transactionUpdateDto.AccountId = evt.UpdateDto.AccountId ?? transactionUpdateDto.AccountId;
            if (evt.UpdateDto.UpdateAmount)
                transactionUpdateDto.Amount = evt.UpdateDto.Amount.GetValueOrDefault();
            if (evt.UpdateDto.UpdateBeneficiary)
                transactionUpdateDto.Beneficiary = evt.UpdateDto.Beneficiary ?? transactionUpdateDto.Beneficiary;
            if (evt.UpdateDto.UpdateCategoryId)
                transactionUpdateDto.CategoryId = evt.UpdateDto.CategoryId.GetValueOrDefault();
            if (evt.UpdateDto.UpdateCurrencyId) 
                transactionUpdateDto.CurrencyId = evt.UpdateDto.CurrencyId;
            if (evt.UpdateDto.UpdateDescription)
                transactionUpdateDto.Description = evt.UpdateDto.Description;
            if (evt.UpdateDto.UpdateDestinationAccountId)
                transactionUpdateDto.DestinationAccountId = evt.UpdateDto.DestinationAccountId;
            if (evt.UpdateDto.UpdatePaymentMethodId)
                transactionUpdateDto.PaymentMethodId = evt.UpdateDto.PaymentMethodId;
            if (evt.UpdateDto.UpdateStatus)
                transactionUpdateDto.Status = evt.UpdateDto.Status.GetValueOrDefault();
            if (evt.UpdateDto.UpdateTransactionDate)
                transactionUpdateDto.TransactionDate = evt.UpdateDto.TransactionDate.GetValueOrDefault();
            if (evt.UpdateDto.UpdateTransactionType)
                transactionUpdateDto.TransactionType = evt.UpdateDto.TransactionType.GetValueOrDefault();


            UpdateTransactionCommand command = new UpdateTransactionCommand(evt.CurrentUserId, evt.BookId);
            command.TransactionId = transaction.Id;
            command.updateDto = transactionUpdateDto;
            await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);

        }
    

        //var transactionCreated = await _transactionService.Update(evt.TransactionId, evt.updateDto, currentUser, cancellationToken).ConfigureAwait(false);
        return true;
    }
}