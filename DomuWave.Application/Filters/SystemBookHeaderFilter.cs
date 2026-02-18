using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SimpleMediator.Core;

namespace DomuWave.Application.Filters;

public class SystemBookHeaderFilter : IAsyncActionFilter
{
 
    protected readonly IMediator _mediator;
    protected readonly IUserService _userService;
    public SystemBookHeaderFilter(IMediator mediator, IUserService userService)
    {
        _mediator = mediator;
        _userService = userService;
    }




    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
       

         


        await next();
    }
}