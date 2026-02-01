using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Implementations;
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
using AccountCreateDto = DomuWave.Services.Models.Dto.AccountCreateDto;
using AccountUpdateDto = DomuWave.Services.Models.Dto.AccountUpdateDto;


namespace DomuWave.Microservice.Controllers;

/// <summary>
///     Gestione utenti
/// </summary>
[Route("api/[controller]")]
public class AccountsController(
    ILogger<AccountsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    IMenuService menuService,
    IMemoryCache cache,
    IMediator mediator,
    IStoreCommitAction storeCommitAction)
    : PrivateControllerBase(logger,
        configuration)
{

    protected readonly IMediator _mediator = mediator;
    protected readonly IStoreCommitAction _storeCommitAction = storeCommitAction;

    [HttpPost("{accountid}/recalculatebalance")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PostRecalculateBalance(long accountid, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;

        
        _storeCommitAction.EnqueueJob<ServiceJob>(j => j.RecalculateAccountBalance(accountid, BookId, currentUserId, CancellationToken.None));
        return new PostOkResult("true");
    }


    [HttpPost("recalculatebalance")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PostRecalculateBalance(CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        


        _storeCommitAction.EnqueueJob<ServiceJob>(j => j.CalculateAccountBalanceForAllActiveAccount(BookId, currentUserId, CancellationToken.None));
        return new PostOkResult("true");
    }



    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.AccountCreateDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateAccountCommand createAccountCommand = new CreateAccountCommand(CurrentUser.Id, BookId)
        {
            CreateDto = new AccountCreateDto()
            {
                AccountTypeId = create.AccountTypeId,
                Description = create.Description,
                Name = create.Name,
                OwnerId = CurrentUser.Id, 
                InitialBalance = create.InitialBalance, 
                BookId = this.BookId, 
                OpenDate = create.OpenDate,
                ClosedDate = null,
                CurrencyId = create.CurrencyId,
                
            },
            CurrentUserId = currentUserId


        };

        var accountCreated = await _mediator.GetResponse(createAccountCommand, cancellationToken).ConfigureAwait(false);
        if (accountCreated != null)
        {
            return new PostOkResult(accountCreated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPut("{accountId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PutUpdate(long accountId, DomuWave.Application.Models.AccountUpdateDto update, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateAccountCommand updateAccountCommand = new UpdateAccountCommand(CurrentUser.Id, BookId)
        {
            UpdateDto = new AccountUpdateDto()
            {
                AccountId = accountId,
                Description = update.Description,
                Name = update.Name,
                
                InitialBalance = update.InitialBalance,
                
                OpenDate = update.OpenDate,
                ClosedDate= update.ClosedDate,
                
                CurrencyId = update.CurrencyId,

            },
            CurrentUserId = currentUserId


        };

        var accountUpdated = await _mediator.GetResponse(updateAccountCommand, cancellationToken).ConfigureAwait(false);
        if (accountUpdated != null)
        {
            return new PostOkResult(accountUpdated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<AccountReadDto>))]
    public async Task<IActionResult> GetFinds(CancellationToken cancellationToken)
    {

        FindAccountsCommand findAccountsCommand = new FindAccountsCommand(CurrentUser.Id, BookId)
        {
            CurrentUserId = CurrentUser.Id,
            OwnerId = CurrentUser.Id
        };

        var accounts = await _mediator.GetResponse(findAccountsCommand, cancellationToken).ConfigureAwait(false);
        if (accounts != null)
        {
            return new OkObjectResult(accounts);
        }


        return BadRequest("Si sono verificati degli errori");
    }
    
    [HttpGet("{accountid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> GetById(long accountid, CancellationToken cancellationToken)
    {

        GetAccountByIdCommand getAccountById = new GetAccountByIdCommand(CurrentUser.Id, BookId)
        {
            CurrentUserId = CurrentUser.Id,
            OwnerId = CurrentUser.Id,
            AccountId = accountid,
            BookId = BookId
        };

        var accounts = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (accounts != null)
        {
            return new OkObjectResult(accounts);
        }


        return null;
    }

    [HttpDelete("{accountid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteById(int accountid, CancellationToken cancellationToken)
    {

        DeleteAccountByIdCommand getAccountById = new DeleteAccountByIdCommand(CurrentUser.Id, BookId)
        {
            CurrentUserId = CurrentUser.Id,
            AccountId = accountid,
            BookId = BookId
        };

        var deleteReslut = await _mediator.GetResponse(getAccountById, cancellationToken).ConfigureAwait(false);
        if (deleteReslut )
        {
            return new OkObjectResult(deleteReslut);
        }


        return null;
    }



    [HttpGet("{accountid}/dashboard")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountDashboardDto))]
    public async Task<IActionResult> GetDashboardById(int accountid, CancellationToken cancellationToken)
    {

        GetAccountDashboardCommand accountDashboardCommand = new GetAccountDashboardCommand(CurrentUser.Id, BookId)
        {
            CurrentUserId = CurrentUser.Id,
            OwnerId = CurrentUser.Id,
            AccountId = accountid,
            BookId = BookId
        };

        var accounts = await _mediator.GetResponse(accountDashboardCommand, cancellationToken).ConfigureAwait(false);
        if (accounts != null)
        {
            return new OkObjectResult(accounts);
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPatch("{accountid}/{paymentMethodId}/setasdefault")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts,AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountDashboardDto))]
    public async Task<IActionResult> PatchSetPaymentMEthodAsDefault(int accountid, int paymentMethodId,CancellationToken cancellationToken)
    {

        SetPaymentMethodDefaultForAccount setPaymentMethodDefault =
            new SetPaymentMethodDefaultForAccount(accountid, paymentMethodId, CurrentUser.Id, BookId);


        
        var result = await _mediator.GetResponse(setPaymentMethodDefault, cancellationToken).ConfigureAwait(false);
        if (result )
        {
            return new OkObjectResult(result);
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("{accountid}/paymentmethods")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.AccountType,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<PaymentMethodDto>))]
    public async Task<IActionResult> GetAllPaymentMethods(int accountid, CancellationToken cancellationToken)
    {
        GetPaymentMethodsForAccountCommand getPaymentMethodsForAccountCommandType =
            new GetPaymentMethodsForAccountCommand(accountid, CurrentUser.Id);


        var paymentMethodForAccount = await _mediator.GetResponse(getPaymentMethodsForAccountCommandType, cancellationToken).ConfigureAwait(false);
        if (paymentMethodForAccount != null)
        {
            IList<PaymentMethodDto> returnList = new List<PaymentMethodDto>();

            foreach ((PaymentMethod paymentMethod, bool IsDefault) tuple in paymentMethodForAccount)
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




    [HttpPatch("{accountid}/reset")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountDashboardDto))]
    public async Task<IActionResult> PatchResetAccount(int accountid, CancellationToken cancellationToken)
    {

        ResetAccountCommand resetAccountCommand =
            new ResetAccountCommand(CurrentUser.Id, BookId);
        resetAccountCommand.AccountId = accountid;



        var result = await _mediator.GetResponse(resetAccountCommand, cancellationToken).ConfigureAwait(false);
        if (result)
        {
            return new OkObjectResult(result);
        }


        return BadRequest("Si sono verificati degli errori");
    }

    //
}