using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.DTO;
using CPQ.Core.Persistence.SessionFactories;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class GetTransactionStatusCommandConsumer : InMemoryConsumerBase<GetTransactionStatusCommand, IList<LookupEntityDtoExtended<int>>>
{
    private ITransactionStatusService _transactionStatusService;

    public GetTransactionStatusCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionStatusService transactionStatusService) : base(sessionFactoryProvider)
    {
        _transactionStatusService = transactionStatusService;
    }

    protected override async Task<IList<LookupEntityDtoExtended<int>>> Consume(GetTransactionStatusCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var statuses = await _transactionStatusService.FindAll(cancellationToken).ConfigureAwait(false);

            
        
        return statuses.Select(s => s.ToLookupEntityDto()).ToList();
    }
}