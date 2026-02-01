using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType
{
    public class CreateAccountTypeCommandConsumer : InMemoryConsumerBase<CreateAccountTypeCommand, AccountTypeReadDto>
    {
        private readonly IAccountTypeService _AccountTypeService;
        private readonly IUserService _userService;

        public CreateAccountTypeCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService AccountTypeService, IUserService userService) : base(sessionFactoryProvider)
        {
            _AccountTypeService = AccountTypeService;
            _userService = userService;
        }

        protected override async Task<AccountTypeReadDto> Consume(CreateAccountTypeCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
        {
            
            var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);



            var x = await _AccountTypeService.Create(
                @event.Item, currentUser, cancellationToken).ConfigureAwait(false);

            return x.ToDto();

            

        }
        
    }
}
