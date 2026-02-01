using System.Net;
using DomuWave.Services.Command;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Menu;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Consumers;
using CPQ.Core.DTO;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Book;

public class PopulateAccountsEventCommandConsumer : InMemoryConsumerBase<PopulateAccountsEventCommand, IList<MenuItemDto>>
{
    private readonly IMediator _mediator;

    public PopulateAccountsEventCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IMediator mediator) : base(sessionFactoryProvider)
    {
        _mediator = mediator;
    }


    protected override async Task<IList<MenuItemDto>> Consume(PopulateAccountsEventCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        GetMenuItemsCommand allAccountsCommand = new GetMenuItemsCommand(evt.CurrentUserId, ((BaseBookRelatedCommand)evt).BookId)
            { BookId = evt.BookId, CurrentUserId = evt.CurrentUserId, OwnerId = evt.CurrentUserId };


        IList<MenuItemDto>? allAccounts = await _mediator.GetResponse(allAccountsCommand, cancellationToken).ConfigureAwait(false);

        return allAccounts;
    }
}