using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Consumers.AccountType;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
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
using AccountTypeCreateUpdateDto = DomuWave.Services.Models.Dto.AccountTypeCreateUpdateDto;


namespace DomuWave.Microservice.Controllers;

/// <summary>
///     Gestione tipologia di conto
/// </summary>
[Route("api/[controller]")]
public class AccountTypeController(
    ILogger<AccountTypeController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
 
    IMediator mediator)
    : PrivateControllerBase(logger,
        configuration)
{

    protected readonly IMediator _mediator = mediator;


    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.AccountType, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountTypeReadDto))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.AccountTypeCreateUpdateDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateAccountTypeCommand createAccountTypeCommand = new CreateAccountTypeCommand(CurrentUser.Id)
        {
            Item = new AccountTypeCreateUpdateDto(){Code  = create.Code, Description = create.Description}
            
        };
        var accountTypeCreatedDto = await _mediator.GetResponse(createAccountTypeCommand, cancellationToken).ConfigureAwait(false);
        if (accountTypeCreatedDto != null)
        {
            return new PostOkResult(accountTypeCreatedDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPut("{accountTypeId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.AccountType, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PutUpdate(int accountTypeId, DomuWave.Application.Models.AccountTypeCreateUpdateDto update, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateAccountTypeCommand updateAccountTypeCommand = new UpdateAccountTypeCommand(CurrentUser.Id)
        {
            UpdateDto = new AccountTypeCreateUpdateDto()
            {
                
                Description = update.Description,
                Code= update.Code

            },
            AccountTypeId = accountTypeId,
            CurrentUserId = currentUserId


        };

        var accountTypeReadDto = await _mediator.GetResponse(updateAccountTypeCommand, cancellationToken).ConfigureAwait(false);
        if (accountTypeReadDto != null)
        {
            return new PostOkResult(accountTypeReadDto.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.AccountType,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<AccountTypeReadDto>))]
    public async Task<IActionResult> GetFinds(CancellationToken cancellationToken)
    {

        FindAccountTypeCommand findAccountTypeCommand = new FindAccountTypeCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id
            
        };

        var accountTypes = await _mediator.GetResponse(findAccountTypeCommand, cancellationToken).ConfigureAwait(false);
        if (accountTypes != null)
        {
            return new OkObjectResult(accountTypes);
        }


        return BadRequest("Si sono verificati degli errori");
    }
    
    [HttpGet("{accountTypeid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.AccountType,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountTypeReadDto))]
    public async Task<IActionResult> GetById(int accountTypeid, CancellationToken cancellationToken)
    {

        GetAccountTypeByIdCommand getAccountTypeByIdCommand = new GetAccountTypeByIdCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id,Id = accountTypeid
            
        };

        var accountTypeReadDto = await _mediator.GetResponse(getAccountTypeByIdCommand, cancellationToken).ConfigureAwait(false);
        if (accountTypeReadDto != null)
        {
            return new OkObjectResult(accountTypeReadDto);
        }


        return null;
    }


    /// <summary>
    ///     Associa la tipologia di pagamento specificata alla tipologia account
    /// </summary>
    /// <param name="accountTypeid"></param>
    /// <param name="paymentMethodId"></param>
    /// <param name="isDefault"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("{accountTypeid}/associate/{paymentMethodId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.AccountType, AuthorizationKeys.DomuWaveModule)]
    [AuthorizationApiFactory(AuthorizationFilterType.CanAction, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountTypeReadDto))]
    public async Task<IActionResult> PatchAssociateToPaymentMethod(int accountTypeid, int paymentMethodId, bool isDefault, CancellationToken cancellationToken)
    {
        AssociatePaymentMethodToAccountType associateAccountToPayment =
            new AssociatePaymentMethodToAccountType(accountTypeid, paymentMethodId, isDefault, CurrentUser.Id);


         
        var accountTypeReadDto = await _mediator.GetResponse(associateAccountToPayment, cancellationToken).ConfigureAwait(false);
        
        return new OkObjectResult(accountTypeReadDto);
        
   
    }

    [HttpGet("{accountTypeid}/paymentmethods")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.AccountType,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<PaymentMethodDto>))]
    public async Task<IActionResult> GetAllPaymentMethods(int accountTypeid, CancellationToken cancellationToken)
    {
        GetPaymentMethodsForAccountType getPaymentMethodsForAccountType =
            new GetPaymentMethodsForAccountType(accountTypeid, CurrentUser.Id);

        
        var paymentMethodForAccountType = await _mediator.GetResponse(getPaymentMethodsForAccountType, cancellationToken).ConfigureAwait(false);
        if (paymentMethodForAccountType != null)
        {
            IList<PaymentMethodDto> returnList = new List<PaymentMethodDto>();

            foreach ((PaymentMethod paymentMethod, bool IsDefault) tuple in paymentMethodForAccountType)
            {
                PaymentMethodDto item = new PaymentMethodDto();
                
                item.IsDefault = tuple.IsDefault;
                item.PaymentMethod = tuple.paymentMethod.ToDto();

                returnList.Add(item);
            }

            return new OkObjectResult(returnList);
        }


        return null;
    }

}