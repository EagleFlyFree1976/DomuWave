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
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using MassTransit;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book
{
    public class CreateBookCommandConsumer : InMemoryConsumerBase<CreateBookCommand, BookReadDto>
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        public CreateBookCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IBookService bookService, IUserService userService) : base(sessionFactoryProvider)
        {
            _bookService = bookService;
            _userService = userService;
        }


        protected override async  Task<BookReadDto> Consume(CreateBookCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
        {
            var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);

            Models.Book x = await _bookService.Create(new BookDto()
            {
                Description = @event.Description, Name = @event.Name, OwnerId = @event.OwnerId

            }, currentUser, cancellationToken).ConfigureAwait(false);

            if (x != null)
            {
                return x.ToDto();

            }

            return null;

        }
     
    }
}
