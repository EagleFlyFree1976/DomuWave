using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class UpdateAccountCommandConsumer : InMemoryConsumerBase<UpdateAccountCommand, AccountReadDto>
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;

    public UpdateAccountCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountService accountService, IUserService userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService = userService;
    }

    protected override async Task<AccountReadDto> Consume(UpdateAccountCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);



        Models.Account x = await _accountService.Update(@event.UpdateDto.AccountId,@event.UpdateDto, currentUser, cancellationToken).ConfigureAwait(false);

        return x.ToDto();
        }
}