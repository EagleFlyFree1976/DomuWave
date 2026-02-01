using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Helper;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Transaction;
using CPQ.Core.ActionFilters;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Result;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using SimpleMediator.Core;
using AccountCreateDto = DomuWave.Services.Models.Dto.AccountCreateDto;

namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
public class TransactionsController : PrivateControllerBase
{
    protected IMediator _mediator;
    public TransactionsController(ILogger<TransactionsController> logger, IOptionsMonitor<OxCoreSettings> configuration, IMediator mediator) : base(logger, configuration)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TransactionReadDto))]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        GetTransactionByIdCommand byIdCommand = new GetTransactionByIdCommand(id, currentUserId, BookId);

        
        TransactionReadDto transactionCreated = await _mediator.GetResponse(byIdCommand, cancellationToken).ConfigureAwait(false);
        if (transactionCreated != null)
        {
            return new OkObjectResult(transactionCreated);
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpDelete("{id:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TransactionReadDto))]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
         
        DeleteTransactionByIdCommand byIdCommand = new DeleteTransactionByIdCommand(id, currentUserId, BookId);


        bool transactionDeleted = await _mediator.GetResponse(byIdCommand, cancellationToken).ConfigureAwait(false);
        
        return new OkObjectResult(transactionDeleted);
        


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(long))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.TransactionDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateTransactionCommand createAccountCommand = new CreateTransactionCommand(CurrentUser.Id, BookId)
        {
            CreateDto = new TransactionCreateDto()
            {
                Description = create.Description,
                Beneficiary = create.Beneficiary,
                Amount = create.Amount,
                AccountId = create.AccountId,
                CategoryId = create.CategoryId,
                PaymentMethodId = create.PaymentMethodId,
                TransactionDate = create.TransactionDate ?? DateTime.Now,
                Tags = create.Tags,
                Status = create.Status,
                CurrencyId = create.CurrencyId,
                TransactionType = create.TransactionType,
                DestinationAccountId = create.DestinationAccountId

            },
            CurrentUserId = currentUserId
        };

        var transactionCreated = await _mediator.GetResponse(createAccountCommand, cancellationToken).ConfigureAwait(false);
        if (transactionCreated != null)
        {
            return new PostOkResult(transactionCreated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPut("{transactionId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TransactionReadDto))]
    public async Task<IActionResult> PutUpdate(long transactionId, DomuWave.Application.Models.TransactionDto update, CancellationToken _cancellationToken)
    {
        TrackedCancellationTokenSource cancellationToken = new TrackedCancellationTokenSource(_logger);
        var currentUserId = CurrentUser.Id;
        UpdateTransactionCommand updateTransactionCommand = new UpdateTransactionCommand(CurrentUser.Id, BookId)
        {
            updateDto = new TransactionUpdateDto()
            {
                Description = update.Description,
                Beneficiary = update.Beneficiary,
                Amount = update.Amount,
                AccountId = update.AccountId,
                CategoryId = update.CategoryId,
                PaymentMethodId = update.PaymentMethodId,
                TransactionDate = update.TransactionDate ?? DateTime.Now,
                Status = update.Status,
                CurrencyId = update.CurrencyId,
                TransactionType = update.TransactionType,
                DestinationAccountId = update.DestinationAccountId
                
            },
            CurrentUserId = currentUserId,
            TransactionId = transactionId
        };

        var transactionUpdated = await _mediator.GetResponse(updateTransactionCommand, cancellationToken.Token).ConfigureAwait(false);
        if (transactionUpdated != null)
        {
            return new PostOkResult(transactionUpdated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPatch("massive")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TransactionReadDto))]
    public async Task<IActionResult> PatchMassiveUpdate(DomuWave.Application.Models.TransactionMassiveUpdateDto updateDto, CancellationToken _cancellationToken)
    {
        TrackedCancellationTokenSource cancellationToken = new TrackedCancellationTokenSource(_logger);
        var currentUserId = CurrentUser.Id;
        UpdateMassiveTransactionCommand updateTransactionCommand = new UpdateMassiveTransactionCommand(CurrentUser.Id, BookId)
        {
            UpdateDto = updateDto,
            CurrentUserId = currentUserId
            
        };
        updateTransactionCommand.TransactionIds = updateDto.TransactionIds.ToList();
        var transactionUpdated = await _mediator.GetResponse(updateTransactionCommand, cancellationToken.Token).ConfigureAwait(false);
        
        return new PostOkResult(transactionUpdated.ToString());
        


        
    }

    [HttpGet("status")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<LookupEntityDto<int>>))]
    public async Task<IActionResult> GetStatusList(CancellationToken cancellationToken = default)
    {

        GetTransactionStatusCommand command = new GetTransactionStatusCommand(CurrentUser.Id);



        var transactionResults = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (transactionResults != null)
        {
            return new OkObjectResult(transactionResults);
        }


        return null;
    }


    [HttpGet("types")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<LookupEntityDto<int>>))]
    public async Task<IActionResult> GetTransactionTypes(CancellationToken cancellationToken = default)
    {

        List<LookupEntityDto<int>> transationTypes = new List<LookupEntityDto<int>>();
        transationTypes.Add(new LookupEntityDto<int>(){Code = TransactionType.Entrata.GetDisplayName(), Description = TransactionType.Entrata.GetDisplayName(), Id = (int)TransactionType.Entrata});
        transationTypes.Add(new LookupEntityDto<int>(){Code = TransactionType.Uscita.GetDisplayName(), Description = TransactionType.Uscita.GetDisplayName(), Id = (int)TransactionType.Uscita });
        transationTypes.Add(new LookupEntityDto<int>(){Code = TransactionType.Trasferimento.GetDisplayName(), Description = TransactionType.Trasferimento.GetDisplayName(), Id = (int)TransactionType.Trasferimento });

        if (transationTypes != null)
        {
            return new OkObjectResult(transationTypes);
        }


        return null;
    }

    [HttpGet("find")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<TransactionReadDto>))]
    public async Task<IActionResult> GetFind(int? targetAccountId, int? accountId, int? categoryId, 
        DateTime? fromDate, DateTime? toDate, string transactionType = null, string flowDirection = null,
        int? status = null,
        string note = null,
        int page = 1,
        int pageSize = 20, string sortBy = "TransactionDate", bool asc = true, CancellationToken cancellationToken = default)
    {

        FindTransactionsCommand findTransactionCommand = new FindTransactionsCommand();

        
        FindTransaction findParameters = new FindTransaction();
        findParameters.BookId = BookId;
        findParameters.TargetAccountId = targetAccountId;
        findParameters.AccountId = accountId;
        findParameters.CategoryId = categoryId;
        findParameters.Date = new DateRange() { StartDate = fromDate, EndDate = toDate };
        findParameters.TransactionType = TransactionTypeMap.TryGetEnum(transactionType);
        findParameters.FlowDirection = FlowDirectionMap.TryGetEnum(flowDirection);
        findParameters.Status = status;
        findParameters.Note = note; findTransactionCommand.Filters = findParameters;

        findTransactionCommand.CurrentUserId = CurrentUser.Id;
        findTransactionCommand.PageNumber = page;
        findTransactionCommand.PageSize = pageSize;
        findTransactionCommand.SortField = sortBy;
        findTransactionCommand.Asc = asc;



        var transactionResults = await _mediator.GetResponse(findTransactionCommand, cancellationToken).ConfigureAwait(false);
        if (transactionResults != null)
        {
            return new OkObjectResult(transactionResults);
        }


        return null;
    }
}