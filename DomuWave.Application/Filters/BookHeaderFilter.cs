using DomuWave.Services.Command.Book;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SimpleMediator.Core;

namespace DomuWave.Application.Filters;

public class BookHeaderFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Book-Id";
    protected readonly IMediator _mediator;
    protected readonly IUserService _userService;
    public BookHeaderFilter(IMediator mediator, IUserService userService)
    {
        _mediator = mediator;
        _userService = userService;
    }




    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.Request.Headers.TryGetValue(HeaderName, out StringValues bookid))
        {
            long lBookId = 0;
            if (long.TryParse(bookid.ToString(), out lBookId))
            {
                context.HttpContext.Items["BookId"] = lBookId;
            }
        }

        if (!context.HttpContext.Items.ContainsKey("BookId"))
        {
            IUser user = context.HttpContext.User as IUser;

            if (user == null && context.HttpContext.Request.Headers.ContainsKey("X-Auth-Token"))
            {
                var userToken = context.HttpContext.Request.Headers["X-Auth-Token"].ToString();
                if (!string.IsNullOrEmpty(userToken))
                {
                    user = await _userService.GetByTokenAsync(userToken, CancellationToken.None)
                        .ConfigureAwait(false);
                }
            }
            if (user != null)
            {
                var appBookIdn = context.HttpContext.Request.Headers["X-BookId"].ToString();
                long bookId = 0;
                if (!string.IsNullOrEmpty(appBookIdn))
                {
                    long.TryParse(appBookIdn, out bookId);
                }

                if (bookId != 0)
                {
                    GetBookByIdCommand getBookById =
                        new GetBookByIdCommand(user.Id, bookId, bookId) { CurrentUserId = user.Id, BookId = bookId };
                    var book = await _mediator.GetResponse(getBookById, CancellationToken.None)
                        .ConfigureAwait(false);

                    if (book != null)
                    {
                        context.HttpContext.Items["BookId"] = book.Id;

                    }
                }
                else
                {
                    GetPrimaryOrCreateBookCommand primaryOrCreateBookCommand =
                        new GetPrimaryOrCreateBookCommand(user.Id) { CurrentUserId = user.Id, OwnerId = user.Id };
                    var defaultBook = await _mediator.GetResponse(primaryOrCreateBookCommand, CancellationToken.None)
                        .ConfigureAwait(false);

                    if (defaultBook != null)
                    {
                        context.HttpContext.Items["BookId"] = defaultBook.Id;

                    }
                }


            }

        }


        await next();
    }
}