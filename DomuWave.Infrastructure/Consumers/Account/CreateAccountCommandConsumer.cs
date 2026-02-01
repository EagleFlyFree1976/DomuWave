using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Bus;
using CPQ.Core.Consumers;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using MassTransit;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book
{
    public class CreateAccountCommandConsumer : InMemoryConsumerBase<CreateAccountCommand, AccountReadDto>
    {
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private IMediator _mediator;

        public CreateAccountCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountService accountService, IUserService userService, IMediator mediator) : base(sessionFactoryProvider)
        {
            _accountService = accountService;
            _userService = userService;
            _mediator = mediator;
        }


        protected override async  Task<AccountReadDto> Consume(CreateAccountCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
        {
            var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);



            Models.Account x = await _accountService.Create(
                @event.CreateDto, currentUser, cancellationToken).ConfigureAwait(false);


            GetPaymentMethodsForAccountType getPaymentMethodsForAccountType = new GetPaymentMethodsForAccountType()
            {
                AccountTypeId = x.AccountType.Id, CurrentUserId = @event.CurrentUserId
            };
            IList<(PaymentMethod paymentMethod, bool IsDefault)>? paymentMethods = await _mediator.GetResponse(getPaymentMethodsForAccountType, cancellationToken)
                .ConfigureAwait(false);

            SetPaymentMethodDefaultForAccount setDefaultForAccount = null;
            foreach ((PaymentMethod paymentMethod, bool IsDefault) tuple in paymentMethods)
            {
                AssociateAccountToPaymentMethod associateAccountToPayment = new AssociateAccountToPaymentMethod()
                {
                    AccountId = x.Id,
                    BookId = x.Book.Id,
                    CurrentUserId = @event.CurrentUserId, PaymentMethodId = tuple.paymentMethod.Id

                };

                await _mediator.GetResponse(associateAccountToPayment, cancellationToken).ConfigureAwait(false);
                if (tuple.IsDefault)
                {
                    setDefaultForAccount = new SetPaymentMethodDefaultForAccount()
                    {
                        AccountId = x.Id, BookId = x.Book.Id, CurrentUserId = @event.CurrentUserId,
                        PaymentMethodId = tuple.paymentMethod.Id
                    };
                }
            }

            if (setDefaultForAccount != null)
            {
                await _mediator.GetResponse(setDefaultForAccount, cancellationToken).ConfigureAwait(false);
            }

            return x.ToDto();

            

        }
        
    }
}
