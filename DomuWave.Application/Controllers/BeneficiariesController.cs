using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Beneficiary;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Beneficiary;
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
public class BeneficiariesController(
    ILogger<BeneficiariesController> logger,
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
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BeneficiaryReadDto))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.BeneficiaryCreateUpdateDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateBeneficiaryCommand createBeneficiaryCommand = new CreateBeneficiaryCommand(currentUserId, BookId)
        {
            CreateDto = new BeneficiaryCreateUpdateDto()
            {

                Description = create.Description,
                CategoryId = create.CategoryId,
                Iban = create.Iban,
                Notes = create.Notes,
                Name = create.Name,
                BookId = this.BookId
            },
            CurrentUserId = currentUserId


        };

        var beneficiaryReadDto = await _mediator.GetResponse(createBeneficiaryCommand, cancellationToken).ConfigureAwait(false);
        if (beneficiaryReadDto != null)
        {
            return new PostOkResult(beneficiaryReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }



    [HttpPost("byname")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BeneficiaryReadDto))]
    public async Task<IActionResult> PostCreateByName(DomuWave.Application.Models.BeneficiaryCreateByNameDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateBeneficiaryByNameCommand createBeneficiaryCommand = new CreateBeneficiaryByNameCommand(currentUserId, BookId)
        {
            CreateDto = new BeneficiaryCreateByNameDto()
            {
                CategoryId = create.CategoryId,
                Name = create.Name,
                BookId = this.BookId
            },
            CurrentUserId = currentUserId


        };

        var beneficiaryReadDto = await _mediator.GetResponse(createBeneficiaryCommand, cancellationToken).ConfigureAwait(false);
        if (beneficiaryReadDto != null)
        {
            return new PostOkResult(beneficiaryReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPut("{beneficiaryId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BeneficiaryReadDto))]
    public async Task<IActionResult> PutUpdate(long beneficiaryId, DomuWave.Application.Models.BeneficiaryCreateUpdateDto update, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateBeneficiaryCommand updateBeneficiaryCommand = new UpdateBeneficiaryCommand(beneficiaryId,currentUserId, BookId)
        {
            UpdateDto = new BeneficiaryCreateUpdateDto()
            {
                
                Description = update.Description,
                Name = update.Name,
                BookId = BookId
                
            },
            CurrentUserId = currentUserId


        };

        var beneficiaryReadDto = await _mediator.GetResponse(updateBeneficiaryCommand, cancellationToken).ConfigureAwait(false);
        if (beneficiaryReadDto != null)
        {
            return new PostOkResult(beneficiaryReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<BeneficiaryReadDto>))]
    public async Task<IActionResult> GetFinds(string q, bool? add = false,CancellationToken cancellationToken = default)
    {

        FindBeneficiaryCommand findBeneficiaryCommand = new FindBeneficiaryCommand(CurrentUser.Id, BookId,q);
        findBeneficiaryCommand.AddIfNotExists = add.GetValueOrDefault();


        var beneficiaryReadDtos = await _mediator.GetResponse(findBeneficiaryCommand, cancellationToken).ConfigureAwait(false);
        if (beneficiaryReadDtos != null)
        {
            return new OkObjectResult(beneficiaryReadDtos);
        }


        return BadRequest("Si sono verificati degli errori");
    }
    
    [HttpGet("lookups")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<BeneficiaryLookupReadDto>))]
    public async Task<IActionResult> GetLookupsFinds(string q, bool? add = false,CancellationToken cancellationToken = default)
    {

        FindBeneficiaryCommand findBeneficiaryCommand = new FindBeneficiaryCommand(CurrentUser.Id, BookId,q);
        findBeneficiaryCommand.AddIfNotExists = add.GetValueOrDefault();


        var beneficiaryReadDtos = await _mediator.GetResponse(findBeneficiaryCommand, cancellationToken).ConfigureAwait(false);
        if (beneficiaryReadDtos != null)
        {
            return new OkObjectResult(beneficiaryReadDtos.Select(k=>k.ToLookupEntityDto()));
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("{beneficiaryId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BeneficiaryReadDto))]
    public async Task<IActionResult> GetById(long beneficiaryId,   CancellationToken cancellationToken)
    {

        GetBeneficiaryByIdCommand getAccountById = new GetBeneficiaryByIdCommand(CurrentUser.Id, BookId )
        {
            BeneficiaryId = beneficiaryId,
            
        };

        var beneficiaryReadDto = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (beneficiaryReadDto != null)
        {
            return new OkObjectResult(beneficiaryReadDto);
        }


        return null;
    }

    [HttpDelete("{beneficiaryId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteById(long beneficiaryId, CancellationToken cancellationToken)
    {

        DeleteBeneficiaryByIdCommand getAccountById = new DeleteBeneficiaryByIdCommand(CurrentUser.Id, BookId)
        {
            
             BeneficiaryId = beneficiaryId

        };

        var deleteReslut = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (deleteReslut)
        {
            return new OkObjectResult(deleteReslut);
        }


        return null;
    }



    
}