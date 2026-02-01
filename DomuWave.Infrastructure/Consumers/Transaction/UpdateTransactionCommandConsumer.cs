using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Transaction;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class UpdateTransactionCommandConsumer : InMemoryConsumerBase<UpdateTransactionCommand, TransactionReadDto>
{
    private ITransactionService _transactionService;
    private IUserService _userService;
    public UpdateTransactionCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionService transactionService, IUserService userService) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
    }
        
    protected override async Task<TransactionReadDto> Consume(UpdateTransactionCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
  
        var transactionCreated = await _transactionService.Update(evt.TransactionId,evt.updateDto,  currentUser, cancellationToken).ConfigureAwait(false);
        return transactionCreated.ToDto();
    }
}