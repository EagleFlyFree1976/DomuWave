using DomuWave.Services.Command.Book;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SimpleMediator.Core;

namespace DomuWave.Application.Filters;

public class SystemBookHeaderFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Book-Id";
    protected readonly IMediator _mediator;
    protected readonly IUserService _userService;
    public SystemBookHeaderFilter(IMediator mediator, IUserService userService)
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
                context.HttpContext.Items["SystemBookId"] = lBookId;
            }
        }

        if (!context.HttpContext.Items.ContainsKey("SystemBookId"))
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


                GetSystemBookCommand getSystemBookCommand = new GetSystemBookCommand(){CurrentUserId = user.Id};
                var book = await _mediator.GetResponse(getSystemBookCommand, CancellationToken.None)
                    .ConfigureAwait(false);

                if (book != null)
                {
                    context.HttpContext.Items["SystemBookId"] = book.Id;

                }
                


            }

        }


        await next();
    }
}