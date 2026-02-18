using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SimpleMediator.Core;

namespace DomuWave.Application.Filters;

public class TenantHeaderFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Tenant-Id";
    protected readonly IMediator _mediator;
    protected readonly IUserService _userService;
    public TenantHeaderFilter(IMediator mediator, IUserService userService)
    {
        _mediator = mediator;
        _userService = userService;
    }




    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.Request.Headers.TryGetValue(HeaderName, out StringValues bookid))
        {
            
                context.HttpContext.Items["TenantId"] = bookid;
            
        }

        if (!context.HttpContext.Items.ContainsKey("TenantId"))
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
                Guid tenantId = new Guid(context.HttpContext.Request.Headers["TenantId"].ToString());
                
                
               
                    GetTenantByIdCommand getBookById = 
                        new GetTenantByIdCommand(user.Id, tenantId) ;
                    var book = await _mediator.GetResponse(getBookById, CancellationToken.None)
                        .ConfigureAwait(false);

                    if (book != null)
                    {
                        context.HttpContext.Items["TenantId"] = book.Id;

                    }
                 


            }

        }


        await next();
    }
}