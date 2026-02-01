using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType;

public class GetAccountTypeByIdCommandConsumer : InMemoryConsumerBase<GetAccountTypeByIdCommand, AccountTypeReadDto>
{
    private readonly IAccountTypeService _AccountTypeService;
    private readonly IUserService _userService;

    public GetAccountTypeByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService AccountTypeService, IUserService userService) : base(sessionFactoryProvider)
    {
        _AccountTypeService = AccountTypeService;
        _userService = userService;
    }

    protected override async Task<AccountTypeReadDto> Consume(GetAccountTypeByIdCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return (await _AccountTypeService.GetById(@event.Id,currentUser, cancellationToken).ConfigureAwait(false)).ToDto();
    }
}