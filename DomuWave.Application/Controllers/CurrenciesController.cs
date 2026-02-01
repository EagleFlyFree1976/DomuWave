using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.DTO;
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
public class CurrenciesController(
    ILogger<CurrenciesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    IMenuService menuService,
    IMemoryCache cache,
    IMediator mediator)
    : PrivateControllerBase(logger,
        configuration)
{

    protected readonly IMediator _mediator = mediator;


    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Currencies, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> PostCreate(DomuWave.Application.Models.CurrencyCreateUpdateDto create, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateCurrencyCommand createCurrencyCommand = new CreateCurrencyCommand(CurrentUser.Id)
        {
            Item = new CurrencyCreateUpdateDto()
            {
                Code = create.Code,
                Name = create.Name,
                IsEnabled = create.IsActive,
                DecimalDigits = create.DecimalDigits,
                Symbol = create.Symbol
            },
            CurrentUserId = currentUserId


        };

        var currencyCreated = await _mediator.GetResponse(createCurrencyCommand, cancellationToken).ConfigureAwait(false);
        if (currencyCreated != null)
        {
            return new PostOkResult(currencyCreated.Id.ToString());
        }


        return null;
    }

    
    [HttpPut("{currencyId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Currencies, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> PutUpdate(int currencyId, DomuWave.Application.Models.CurrencyCreateUpdateDto update, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateCurrencyCommand updateCurrencyCommand = new UpdateCurrencyCommand(CurrentUser.Id)
        {
            Item= new CurrencyCreateUpdateDto()
            {
                Code = update.Code,
                Name = update.Name,
                IsEnabled = update.IsActive,
                DecimalDigits = update.DecimalDigits,
                Symbol = update.Symbol,
            },
            CurrencyId = currencyId,
            CurrentUserId = currentUserId


        };

        var updated = await _mediator.GetResponse(updateCurrencyCommand, cancellationToken).ConfigureAwait(false);
        if (updated != null)
        {
            return new PostOkResult(updated.Id.ToString());
        }


        return null;
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<CurrencyReadDto>))]
    public async Task<IActionResult> GetFinds(string q, CancellationToken cancellationToken)
    {

        FindCurrencyCommand findCurrencyCommand = new FindCurrencyCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id,
            Q = q
            
        };

        var currencies = await _mediator.GetResponse(findCurrencyCommand, cancellationToken).ConfigureAwait(false);
        if (currencies != null)
        {
            return new OkObjectResult(currencies);
        }


        return null;
    }
   

    [HttpGet("lookups")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<LookupEntityDto<int>>))]
    public async Task<IActionResult> GetLookupFinds(string q, CancellationToken cancellationToken)
    {

        FindCurrencyCommand findCurrencyCommand = new FindCurrencyCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id,
            Q = q

        };

        var currencies = await _mediator.GetResponse(findCurrencyCommand, cancellationToken).ConfigureAwait(false);
        if (currencies != null)
        {
            return new OkObjectResult(currencies.Select(k=>
                    new LookupEntityDto<int>(){Code = k.Code, Description = k.Description, Id = k.Id}));
        }


        return null;
    }
    
    [HttpGet("{currencyId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> GetById(int currencyId, CancellationToken cancellationToken)
    {

        GetCurrencyByIdCommand currencyByIdCommand = new GetCurrencyByIdCommand(CurrentUser.Id)
        {
            CurrentUserId = CurrentUser.Id,
            Id = currencyId
        };

        var currencyReadDto = await _mediator.GetResponse(currencyByIdCommand, cancellationToken).ConfigureAwait(false);
        if (currencyReadDto != null)
        {
            return new OkObjectResult(currencyReadDto);
        }


        return null;
    }

    [HttpPatch("exchange/fill")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> PatchFillExchangeRate(CancellationToken cancellationToken)
    {

        FillExchangeRateCommand currencyByIdCommand = new FillExchangeRateCommand(CurrentUser.Id);
        

        var currencyReadDto = await _mediator.GetResponse(currencyByIdCommand, cancellationToken).ConfigureAwait(false);
        if (currencyReadDto != null)
        {
            return new OkObjectResult(currencyReadDto);
        }


        return null;
    }
    [HttpPatch("exchange/history/fill")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> PatchFillHistoryExchangeRate(DateTime targetDate,CancellationToken cancellationToken)
    {

        FillHistoricalExchangeRateCommand currencyByIdCommand = new FillHistoricalExchangeRateCommand(CurrentUser.Id,targetDate);


        var currencyReadDto = await _mediator.GetResponse(currencyByIdCommand, cancellationToken).ConfigureAwait(false);
        if (currencyReadDto != null)
        {
            return new OkObjectResult(currencyReadDto);
        }


        return null;
    }

    [HttpGet("convertTo")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ConvertResult))]
    public async Task<IActionResult> GetConvertValue(int currencyFrom, int currencyTo, decimal amount, DateTime targetDate, CancellationToken cancellationToken)
    {

        GetCurrencyByIdCommand currencyFromByIdCommand = new GetCurrencyByIdCommand(CurrentUser.Id) { Id = currencyFrom };
        GetCurrencyByIdCommand currencyToByIdCommand = new GetCurrencyByIdCommand(CurrentUser.Id) { Id = currencyTo };


        var from = await _mediator.GetResponse(currencyFromByIdCommand, cancellationToken).ConfigureAwait(false);


        var to = await _mediator.GetResponse(currencyToByIdCommand, cancellationToken).ConfigureAwait(false);

        ConvertToCurrencyCommand convertToCurrencyCommand = new ConvertToCurrencyCommand(CurrentUser.Id, from, to, amount, targetDate);


        var convertedValue = await _mediator.GetResponse(convertToCurrencyCommand, cancellationToken).ConfigureAwait(false);
        if (convertedValue != null)
        {
            return new OkObjectResult(convertedValue);
        }


        return null;
    }


    [HttpPost("exchange")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> PostCreateExchangeRate(DomuWave.Application.Models.ExchangeRateHistoryCreateUpdateDto item,CancellationToken cancellationToken)
    {

        CreateExchangeRateCommand createExchangeRateCommand = new CreateExchangeRateCommand(CurrentUser.Id);

        GetDefaultCurrencyCommand getDefaultCurrencyCommand = new GetDefaultCurrencyCommand(CurrentUser.Id);
        var defaultCurrency = await _mediator.GetResponse(getDefaultCurrencyCommand, cancellationToken).ConfigureAwait(false);

        var currencyRange = item.TargetDate.ToCurrencyRange();

        createExchangeRateCommand.Item = new ExchangeRateHistoryCreateUpdateDto()
        {
            FromCurrencyId = defaultCurrency.Id,
            ToCurrencyId = item.ToCurrencyId,
            Rate = item.Rate,
            ValidFrom = currencyRange.from,
            ValidTo = currencyRange.to
        };

        var currencyReadDto = await _mediator.GetResponse(createExchangeRateCommand, cancellationToken).ConfigureAwait(false);
        if (currencyReadDto != null)
        {
            return new OkObjectResult(currencyReadDto);
        }


        return null;
    }
    
    [HttpPut("exchange/{exchangeRateHistoryId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(CurrencyReadDto))]
    public async Task<IActionResult> PutCreateExchangeRate(long exchangeRateHistoryId, DomuWave.Application.Models.ExchangeRateHistoryCreateUpdateDto item,CancellationToken cancellationToken)
    {

        UpdateExchangeRateCommand updateExchangeRateCommand = new UpdateExchangeRateCommand(CurrentUser.Id);

        GetDefaultCurrencyCommand getDefaultCurrencyCommand = new GetDefaultCurrencyCommand(CurrentUser.Id);
        var defaultCurrency = await _mediator.GetResponse(getDefaultCurrencyCommand, cancellationToken).ConfigureAwait(false);

        var currencyRange = item.TargetDate.ToCurrencyRange();

        updateExchangeRateCommand.ExchangeRateHistoryId = exchangeRateHistoryId;
        updateExchangeRateCommand.Item = new ExchangeRateHistoryCreateUpdateDto()
        {
            FromCurrencyId = defaultCurrency.Id,
            ToCurrencyId = item.ToCurrencyId,
            Rate = item.Rate,
            ValidFrom = currencyRange.from,
            ValidTo = currencyRange.to
        };

        var currencyReadDto = await _mediator.GetResponse(updateExchangeRateCommand, cancellationToken).ConfigureAwait(false);
        if (currencyReadDto != null)
        {
            return new OkObjectResult(currencyReadDto);
        }


        return null;
    }


    [HttpGet("exchange/{exchangeRateHistoryId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ExchangeRateHistoryReadDto))]
    public async Task<IActionResult> GetById(long exchangeRateHistoryId, CancellationToken cancellationToken)
    {

        GetExchangeRateByIdCommand getExchangeRateCommand = new GetExchangeRateByIdCommand();
        getExchangeRateCommand.CurrentUserId = CurrentUser.Id;
        getExchangeRateCommand.ExchangeRateId = exchangeRateHistoryId;


        var convertedValue = await _mediator.GetResponse(getExchangeRateCommand, cancellationToken).ConfigureAwait(false);
        if (convertedValue != null)
        {
            return new OkObjectResult(convertedValue);
        }


        return null;
    }

    [HttpGet("exchange/find")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ExchangeRateHistoryReadDto))]
    public async Task<IActionResult> GetFindExchangeRate(DateTime targetDate, int fromCurrencyId, int toCurrencyId, CancellationToken cancellationToken = default)
    {

        GetExchangeRateCommand findExchangeRateCommand = new GetExchangeRateCommand();
        findExchangeRateCommand.CurrentUserId = CurrentUser.Id;
        findExchangeRateCommand.TargetDate = targetDate;
        findExchangeRateCommand.ToCurrencyId = toCurrencyId;
        findExchangeRateCommand.FromCurrencyId = fromCurrencyId;
        findExchangeRateCommand.ExactlyMode = false;



        var convertedValue = await _mediator.GetResponse(findExchangeRateCommand, cancellationToken).ConfigureAwait(false);
        if (convertedValue != null)
        {
            return new OkObjectResult(convertedValue);
        }


        return null;
    }
    [HttpGet("exchange")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<ExchangeRateHistoryReadDto>))]
    public async Task<IActionResult> GetAllByDate(DateTime? targetDate, int? toCurrencyId, int page = 1,
        int pageSize = 20, string sortBy = "ValidFrom", bool asc =true, CancellationToken cancellationToken = default)
    {

        FindExchangeRateCommand findExchangeRateCommand = new FindExchangeRateCommand();
        findExchangeRateCommand.CurrentUserId = CurrentUser.Id;
        findExchangeRateCommand.TargetDate = targetDate;
        findExchangeRateCommand.ToCurrencyId = toCurrencyId;
        findExchangeRateCommand.PageNumber = page;
        findExchangeRateCommand.PageSize = pageSize;
        findExchangeRateCommand.SortField = sortBy;
        findExchangeRateCommand.Asc= asc;
        findExchangeRateCommand.ExactlyMode = false;



        var convertedValue = await _mediator.GetResponse(findExchangeRateCommand, cancellationToken).ConfigureAwait(false);
        if (convertedValue != null)
        {
            return new OkObjectResult(convertedValue);
        }


        return null;
    }

    [HttpDelete("exchange/{exchangeRateId}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Currencies,
        AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteById(int exchangeRateId, CancellationToken cancellationToken)
    {

        DeleteExchangeRateByIdCommand deleteCommand = new DeleteExchangeRateByIdCommand(CurrentUser.Id)
        {

            ExchangeRateId = exchangeRateId

        };

        var deleteReslut = await _mediator.GetResponse(deleteCommand, cancellationToken).ConfigureAwait(false);
        if (deleteReslut)
        {
            return new OkObjectResult(deleteReslut);
        }


        return null;
    }

}