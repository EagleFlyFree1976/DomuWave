using System.Net;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Extensions;
using CPQ.Core.Result;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using BookCreateDto = DomuWave.Application.Models.BookCreateDto;

namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
public class BooksController(
    ILogger<BooksController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    IMenuService menuService,
    IMemoryCache cache,
    IMediator mediator)
    : PrivateControllerBase(logger,
        configuration)
{
    protected readonly IMediator _mediator = mediator;



    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Books,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BookCreateDto))]
    public async Task<IActionResult> PostCreateBooks(DomuWave.Application.Models.BookCreateDto create, CancellationToken cancellationToken)
    {
        
        CreateBookCommand createBookCommand = new CreateBookCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id, Description = create.Description, Name = create.Name,
            OwnerId = create.OwnerId,
        
        };

        var bookCreated = await _mediator.GetResponse(createBookCommand, cancellationToken).ConfigureAwait(false);
        if (bookCreated != null)
        {
            return new PostOkResult(bookCreated.Id.ToString());
        }


        return null;
    }

    [HttpPut("primary")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Books, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> PutSetAsPrimary(int bookid, CancellationToken cancellationToken)
    {

        SetBookaAsPrimaryCommand createBookCommand = new SetBookaAsPrimaryCommand(CurrentUser.Id, BookId)
        {
            CurrentUserId = CurrentUser.Id,
           BookId = bookid

        };

        var bookCreated = await _mediator.GetResponse(createBookCommand, cancellationToken).ConfigureAwait(false);
        return new PostOkResult("ok");


        
    }

    [HttpGet("primary")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Books,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BookReadDto))]
    public async Task<IActionResult> GetDefaultBooks(CancellationToken cancellationToken)
    {

        GetPrimaryOrCreateBookCommand getPrimaryOrCreateBookCommand = new GetPrimaryOrCreateBookCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id,
            OwnerId = CurrentUser.Id
        };

        var books = await _mediator.GetResponse(getPrimaryOrCreateBookCommand, cancellationToken).ConfigureAwait(false);
        if (books != null)
        {
            return new OkObjectResult(books);
        }


        return null;
    }



    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Books,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<BookReadDto>))]
    public async Task<IActionResult> GetFindBooks(CancellationToken cancellationToken)
    {

        FindBooksCommand findBooksCommand = new FindBooksCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id, OwnerId = CurrentUser.Id
        };

        var books = await _mediator.GetResponse(findBooksCommand, cancellationToken).ConfigureAwait(false);
        if (books != null )
        {
            return new OkObjectResult(books);
        }


        return null;
    }

    //
}