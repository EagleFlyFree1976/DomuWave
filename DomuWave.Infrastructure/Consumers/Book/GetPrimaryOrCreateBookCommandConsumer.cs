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
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using MassTransit;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book
{
    public class
        GetPrimaryOrCreateBookCommandConsumer : InMemoryConsumerBase<GetPrimaryOrCreateBookCommand, BookReadDto>
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;

        public GetPrimaryOrCreateBookCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService) : base(sessionFactoryProvider)
        {
            _bookService = bookService;
            _userService = userService;
        }

        protected override async Task<BookReadDto> Consume(GetPrimaryOrCreateBookCommand @event, IMediationContext mediationContext,
            CancellationToken cancellationToken)
        {
            var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);

            Models.Book x = await _bookService.GetPrimaryBook(currentUser, cancellationToken).ConfigureAwait(false);

                if (x != null)
                    return x.ToDto();
                else
                {
                    BookDto primary = new BookDto()
                    {
                        Name = $"[{currentUser.Login}]",
                        Description = @event.Description,
                        IsPrimary = true,
                        OwnerId = @event.OwnerId
                    }
                        ;

                    x = await _bookService.Create(primary, currentUser, cancellationToken)
                        .ConfigureAwait(false);
                    if (x != null)
                        return x.ToDto();


                }

                throw new NotFoundException("Elemento non trovato");


        }
    }
 
}
