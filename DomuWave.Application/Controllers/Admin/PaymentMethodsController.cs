using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.PaymentMethod;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
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



namespace DomuWave.Microservice.Controllers.Admin;

/// <summary>
///     Gestione utenti
/// </summary>
[Route("api/admin/[controller]")]
public class PaymentMethodsController(
    ILogger<PaymentMethodsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    IMenuService menuService,
    IMemoryCache cache,
    IMediator mediator)
    : PrivateAdminControllerBase(logger,
        configuration)
{

    protected readonly IMediator _mediator = mediator;
    protected readonly ICoreAuthorizationManager _coreAuthorizationManager;

 


    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PaymentMethodReadDto))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.PaymentMethodCreateUpdateDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;



        CreatePaymentMethodCommand createAccountCommand = new CreatePaymentMethodCommand(currentUserId, BookId)
        {
            CreateDto = new PaymentMethodCreateUpdateDto()
            {

                Description = create.Description,
                Name = create.Name,
                BookId = this.BookId
            },
            CurrentUserId = currentUserId


        };

        var paymentMethodReadDto = await _mediator.GetResponse(createAccountCommand, cancellationToken).ConfigureAwait(false);
        if (paymentMethodReadDto != null)
        {
            return new PostOkResult(paymentMethodReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPut("{paymentMethodId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PaymentMethodReadDto))]
    public async Task<IActionResult> PutUpdate(int paymentMethodId, DomuWave.Application.Models.PaymentMethodCreateUpdateDto update, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdatePaymentMethodCommand updateAccountCommand = new UpdatePaymentMethodCommand(paymentMethodId,currentUserId, BookId)
        {
            UpdateDto = new PaymentMethodCreateUpdateDto()
            {
                
                Description = update.Description,
                Name = update.Name,
                BookId = BookId
                
            },
            CurrentUserId = currentUserId


        };

        var paymentMethodReadDto = await _mediator.GetResponse(updateAccountCommand, cancellationToken).ConfigureAwait(false);
        if (paymentMethodReadDto != null)
        {
            return new PostOkResult(paymentMethodReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Admin,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<PaymentMethodReadDto>))]
    public async Task<IActionResult> GetFinds(CancellationToken cancellationToken)
    {

        FindPaymentMethodCommand findPaymentMethodCommand = new FindPaymentMethodCommand(CurrentUser.Id, BookId);
        

        var paymentMethodReadDtos = await _mediator.GetResponse(findPaymentMethodCommand, cancellationToken).ConfigureAwait(false);
        if (paymentMethodReadDtos != null)
        {
            return new OkObjectResult(paymentMethodReadDtos);
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("{paymentMethodId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Admin,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PaymentMethodReadDto))]
    public async Task<IActionResult> GetById(int paymentMethodId, long? bookId, CancellationToken cancellationToken)
    {

        GetPaymentMethodByIdCommand getAccountById = new GetPaymentMethodByIdCommand(CurrentUser.Id, BookId)
        {
            PaymentMethodId = paymentMethodId,
            
        };

        var paymentMethodReadDto = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (paymentMethodReadDto != null)
        {
            return new OkObjectResult(paymentMethodReadDto);
        }


        return null;
    }

    [HttpDelete("{paymentMethodId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Admin,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteById(int paymentMethodId, CancellationToken cancellationToken)
    {

        DeletePaymentMethodByIdCommand getAccountById = new DeletePaymentMethodByIdCommand(CurrentUser.Id, BookId)
        {
            
             PaymentMethodId = paymentMethodId
            
        };

        var deleteReslut = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (deleteReslut)
        {
            return new OkObjectResult(deleteReslut);
        }


        return null;
    }



 
}