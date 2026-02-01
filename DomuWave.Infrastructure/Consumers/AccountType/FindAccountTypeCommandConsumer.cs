using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType;

public class FindAccountTypeCommandConsumer : InMemoryConsumerBase<FindAccountTypeCommand, IList<AccountTypeReadDto>>
{
    private readonly IAccountTypeService _AccountTypeService;
    private readonly IUserService _userService;

    public FindAccountTypeCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService AccountTypeService, IUserService userService) : base(sessionFactoryProvider)
    {
        _AccountTypeService = AccountTypeService;
        _userService = userService;
    }

    protected override async Task<IList<AccountTypeReadDto>> Consume(FindAccountTypeCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        IList<Models.AccountType> currencies = await _AccountTypeService.GetAll(currentUser, cancellationToken).ConfigureAwait(false);

        return currencies.Select(j=>j.ToDto()).ToList();
    }
}