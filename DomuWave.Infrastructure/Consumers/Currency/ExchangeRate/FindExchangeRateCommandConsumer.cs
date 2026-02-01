using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Extensions;
using DomuWave.Services.Helper;
using DomuWave.Services.Implementations;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency.Exchange;

public class FindExchangeRateCommandConsumer : InMemoryConsumerBase<FindExchangeRateCommand, PagedResult<ExchangeRateHistoryReadDto>>
{
    private IExchangeRateHistoryService _exchangeRateHistoryService;
    private IUserService _userService;
    public FindExchangeRateCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IExchangeRateHistoryService exchangeRateHistoryService, IUserService userService) : base(sessionFactoryProvider)
    {
        _exchangeRateHistoryService = exchangeRateHistoryService;
        _userService = userService;
    }
    protected override async Task<PagedResult<ExchangeRateHistoryReadDto>> Consume(FindExchangeRateCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);


        PagedResult<ExchangeRateHistory>? result = null;
  
       result =  await _exchangeRateHistoryService.GetByDateAsync(evt.TargetDate, evt.ToCurrencyId, evt.PageNumber, evt.PageSize, evt.SortField, evt.Asc,currentUser, cancellationToken).ConfigureAwait(false);

        if (result == null)
        {

            return new PagedResult<ExchangeRateHistoryReadDto>
            {
                PageNumber = 0,
                PageSize = evt.PageSize,
                TotalCount = 0,
                Items = []
            };
        }

            return new PagedResult<ExchangeRateHistoryReadDto>
            {
                PageNumber = evt.PageNumber,
                PageSize = evt.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(x => x.ToDto()).ToList()
            };
        

        
    }
}