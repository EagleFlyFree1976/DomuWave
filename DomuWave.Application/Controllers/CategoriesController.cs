using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Category;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Category;
using CPQ.Core;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.ModelBinders;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Result;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
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
public class CategoriesController(
    ILogger<CategoriesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    IMenuService menuService,
    IMemoryCache cache,
    IMediator mediator)
    : PrivateControllerBase(logger,
        configuration)
{

    protected readonly IMediator _mediator = mediator;
    protected readonly ICoreAuthorizationManager _coreAuthorizationManager;

    /// <summary>
    ///  Se l'utente ha poteri admin allora puoi usare il book richiesto, altrimenti forzo
    /// il suo book di appartenenza 
    /// </summary>
    /// <param name="currentUserBookId"></param>
    /// <param name="requestedBookId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<long?> GetBookId(long currentUserBookId, long? requestedBookId, IUser currentUser, CancellationToken cancellationToken)
    {
        if (!await _coreAuthorizationManager
                .CanAction(currentUser, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule, cancellationToken)
                .ConfigureAwait(false))
        {
            return currentUserBookId;
        }

        return requestedBookId;
    }


    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CategoryReadDto))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.CategoryCreateUpdateDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateCategoryCommand createAccountCommand = new CreateCategoryCommand(currentUserId, BookId)
        {
            CreateDto = new CategoryCreateUpdateDto()
            {

                Description = create.Description,
                Name = create.Name,
                ParentCategoryId = create.ParentCategoryId,

                BookId = this.BookId
            },
            CurrentUserId = currentUserId


        };

        var categoryReadDto = await _mediator.GetResponse(createAccountCommand, cancellationToken).ConfigureAwait(false);
        if (categoryReadDto != null)
        {
            return new PostOkResult(categoryReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPut("{categoryId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CategoryReadDto))]
    public async Task<IActionResult> PutUpdate(int categoryId, DomuWave.Application.Models.CategoryCreateUpdateDto update, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateCategoryCommand updateAccountCommand = new UpdateCategoryCommand(categoryId, currentUserId, BookId)
        {
            
            UpdateDto = new CategoryCreateUpdateDto()
            {
                
                Description = update.Description,
                Name = update.Name,
                ParentCategoryId = update.ParentCategoryId,
                BookId = BookId
                
            },
            CurrentUserId = currentUserId


        };

        var CategoryReadDto = await _mediator.GetResponse(updateAccountCommand, cancellationToken).ConfigureAwait(false);
        if (CategoryReadDto != null)
        {
            return new PostOkResult(CategoryReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<CategoryReadDto>))]
    public async Task<IActionResult> GetFinds(string q, CancellationToken cancellationToken)
    {

        FindCategoryCommand findCategoryCommand = new FindCategoryCommand(CurrentUser.Id, BookId);
        findCategoryCommand.Q = q;

        var categoryReadDtos = await _mediator.GetResponse(findCategoryCommand, cancellationToken).ConfigureAwait(false);
        if (categoryReadDtos != null)
        {
            return new OkObjectResult(categoryReadDtos);
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpGet("lookups")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<LookupEntityDto<long>>))]
    public async Task<IActionResult> GetLookupsFinds(string q, bool? add = false, CancellationToken cancellationToken = default)
    {
        FindCategoryCommand findCategoryCommand = new FindCategoryCommand(CurrentUser.Id, BookId);
        findCategoryCommand.Q = q;

        var categoryReadDtos = await _mediator.GetResponse(findCategoryCommand, cancellationToken).ConfigureAwait(false);
        if (categoryReadDtos != null)
        {
            return new OkObjectResult(categoryReadDtos.Select(k => k.ToLookupEntityDto()));
        }


        return BadRequest("Si sono verificati degli errori");
    }



    [HttpGet("{categoryId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CategoryReadDto))]
    public async Task<IActionResult> GetById(int categoryId,   CancellationToken cancellationToken)
    {

        GetCategoryByIdCommand getAccountById = new GetCategoryByIdCommand(CurrentUser.Id, BookId )
        {
            CategoryId = categoryId,
            
        };

        var categoryReadDto = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (categoryReadDto != null)
        {
            return new OkObjectResult(categoryReadDto);
        }


        return null;
    }

    [HttpDelete("{categoryId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteById(int categoryId, CancellationToken cancellationToken)
    {

        DeleteCategoryByIdCommand getAccountById = new DeleteCategoryByIdCommand(CurrentUser.Id, BookId)
        {
            
             CategoryId = categoryId
            
        };

        var deleteResult = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (deleteResult)
        {
            return new OkObjectResult(deleteResult);
        }


        return null;
    }



     
}