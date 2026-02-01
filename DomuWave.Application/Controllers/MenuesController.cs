using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Menu;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Extensions;
using CPQ.Core.ModelBinders;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Result;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Util;
using SimpleMediator.Core;


namespace DomuWave.Microservice.Controllers;

/// <summary>
///     Gestione utenti
/// </summary>
[Route("api/[controller]")]
public class MenuesController(
    ILogger<MenuesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    IMenuService menuService,
    IMemoryCache cache,
    IMediator mediator)
    : PrivateControllerBase(logger,
        configuration)
{

    protected readonly IMediator _mediator = mediator;
    protected readonly IMenuService _menuService = menuService;



    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<MenuItemDto>))]
    public async Task<IActionResult> GetAllMenues(CancellationToken cancellationToken)
    {
        var allMenuesItems = await _menuService.GetAllMenuItems(this.CurrentUser, BookId, cancellationToken).ConfigureAwait(false);

        if (allMenuesItems == null)
            return NotFound();

        IList<MenuItemDto> itemToReturn = new List<MenuItemDto>();

        // if (authorizedMenuItems.Where(j=>j.Action).Contains(d=>d.))
        foreach (MenuItem item in allMenuesItems)
        {
            MenuItemDto itemDto = item.ToDto();
            itemToReturn.Add(itemDto);
            if (!string.IsNullOrEmpty(item.PopulateEvent))
            {
                string clsName = item.PopulateEvent;

                Type type = Type.GetType(clsName);

                if (type != null)
                {


                    // Crea un'istanza
                    PopulateMenuEventCommand istanza = Activator.CreateInstance(type) as PopulateMenuEventCommand;
                    if (istanza != null)
                    {
                        istanza.BookId = BookId;
                        istanza.CurrentUserId = CurrentUser.Id;

                        var menuToAdd = await _mediator.GetResponse(istanza, cancellationToken).ConfigureAwait(false);

                        if (menuToAdd != null)
                        {
                            int i = 0;
                            foreach (var menuItem in menuToAdd)
                            {
                                i++;
                                var subMenuItemDto = menuItem;
                                subMenuItemDto.ParentMenuId = itemDto.Id;
                                itemToReturn.Add(subMenuItemDto);
                            }
                        }

                    }
                }


            }
        }

        int max = itemToReturn.Max(k => k.Id);
        foreach (MenuItemDto dto in itemToReturn.Where(k => k.Id == 0))
        {
            max++;
            dto.Id = max;
        }

        return Ok(itemToReturn);
        //return Ok(allMenuesItems.Select(k => k.ToDto()));
    }



    //
}