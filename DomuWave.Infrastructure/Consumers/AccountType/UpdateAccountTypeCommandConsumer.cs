using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using MassTransit;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType;

public class UpdateAccountTypeCommandConsumer : InMemoryConsumerBase<UpdateAccountTypeCommand, AccountTypeReadDto>
{
    private readonly IAccountTypeService _accountTypeService;
    private readonly IUserService _userService;

    public UpdateAccountTypeCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService accountTypeService, IUserService userService) : base(sessionFactoryProvider)
    {
        _accountTypeService = accountTypeService;
        _userService = userService;
    }

    protected override async Task<AccountTypeReadDto> Consume(UpdateAccountTypeCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);



        var x = await _accountTypeService.Update(@event.AccountTypeId,
            @event.UpdateDto, currentUser, cancellationToken).ConfigureAwait(false);

        return x.ToDto();
    }
}