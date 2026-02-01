using DomuWave.Application.Code;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Category;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Helper;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Dto.Transaction;
using DomuWave.Services.Models.Import;
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
using ImportDto = DomuWave.Application.Models.ImportDto;

namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
public class ImporterController : PrivateControllerBase
{
    protected IMediator _mediator;
    public ImporterController(ILogger<ImporterController> logger, IOptionsMonitor<OxCoreSettings> configuration, IMediator mediator) : base(logger, configuration)
    {
        _mediator = mediator;
    }

   
    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PostCreate([FromForm] IFormFile importFile, [FromForm]DomuWave.Application.Models.ImportDto importToCreate, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateImportCommand command = new CreateImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            FileName = importFile.FileName,
            HasHeader = importToCreate.HasHeader,
            TargetAccountId = importToCreate.TargetAccountId,
            csvStream = importFile.OpenReadStream()
        };



        var importCreated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importCreated != null)
        {
            return new PostOkResult(importCreated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPost("{importId}/clone")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PostClone(long importId, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CloneImportCommand command = new CloneImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            ImportId = importId
            
            
            
        };



        var importCreated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importCreated != null)
        {
            return new PostOkResult(importCreated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPatch("{importId:long}/updatefile")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PatchUpdateImportFile(long importId, [FromForm] IFormFile importFile, [FromForm] DomuWave.Application.Models.ImportDto importToCreate, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateImportFileCommand command = new UpdateImportFileCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            FileName = importFile.FileName,
            HasHeader = importToCreate.HasHeader,
            TargetAccountId = importToCreate.TargetAccountId,
            Name = importToCreate.Name,
            csvStream = importFile.OpenReadStream(),
            ImportId = importId
        };



        var importCreated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importCreated != null)
        {
            return new PostOkResult(importCreated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpPatch("{importId:long}/fullimport")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PatchFullImportFile(long importId, [FromForm] IFormFile importFile, [FromForm] DomuWave.Application.Models.ImportDto importToCreate, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        FullImportFileCommand command = new FullImportFileCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            FileName = importFile.FileName,
            HasHeader = importToCreate.HasHeader,
            TargetAccountId = importToCreate.TargetAccountId,
            csvStream = importFile.OpenReadStream(),
            ImportId = importId
        };



        var importCreated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importCreated != null)
        {
            return new PostOkResult(importCreated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPatch("{importId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PatchUpdateConfiguration(long importId, [FromBody]DomuWave.Application.Models.UpdateImportDto importToCreate, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        UpdateImportCommand command = new UpdateImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            TargetAccountId = importToCreate.TargetAccountId,
            ImportConfigurationDto = importToCreate.Configuration,
            Name = importToCreate.Name,
            ImportId = importId

        };



        var importUpdated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importUpdated != null)
        {
            return new PostOkResult(importUpdated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }
    
    [HttpGet("{importId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ImportDto))]
    public async Task<IActionResult> GetImportById(long importId, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        GetByIdImportCommand command = new GetByIdImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            ImportId = importId

        };



        var byId = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (byId != null)
        {

            return new OkObjectResult(byId);
        }


        return BadRequest("Si sono verificati degli errori");
    }

    [HttpGet("finds")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<ImportDto>))]
    public async Task<IActionResult> GetFindImports(CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        FindImportCommand command = new FindImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId

        };



        var byId = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (byId != null)
        {

            return new OkObjectResult(byId);
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpGet("availabletargetfields")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<TargetField>))]
    public async Task<IActionResult> GetavailableTargetField(CancellationToken cancellationToken)
    {

        GetAvailableTargetFieldCommand command = new GetAvailableTargetFieldCommand();
        
        var availableTargetFields
            = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (availableTargetFields != null)
        {
            return new OkObjectResult(availableTargetFields);
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPatch("{importId:long}/extract")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PatchExtractImport(long importId, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        ExtractImportCommand command = new ExtractImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            ImportId = importId

        };



        var importUpdated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importUpdated != null)
        {
            return new PostOkResult(importUpdated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }
    
    [HttpPatch("{importId:long}/importrows")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(bool))]
    public async Task<IActionResult> PatchImportRows(long importId, IList<ImportRowDto> rows, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        CreateImportRowsFromWebCommand command = new CreateImportRowsFromWebCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            ImportId = importId
        };
        command.Rows = rows;
        var importUpdated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        return new PostOkResult(importUpdated.ToString());
    }


    [HttpPatch("{importId:long}/validate")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PatchValidateImport(long importId, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        ValidateImportCommand command = new ValidateImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            ImportId = importId

        };



        var importUpdated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importUpdated != null)
        {
            return new PostOkResult(importUpdated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }


    [HttpPatch("{importId:long}/finalize")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Accounts, AuthorizationKeys.DomuWaveModule)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AccountReadDto))]
    public async Task<IActionResult> PatchFinalizeImport(long importId, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        FinalizeImportCommand command = new FinalizeImportCommand()
        {
            BookId = BookId,
            CurrentUserId = currentUserId,
            ImportId = importId

        };



        var importUpdated = await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);
        if (importUpdated != null)
        {
            return new PostOkResult(importUpdated.Id.ToString());
        }


        return BadRequest("Si sono verificati degli errori");
    }
}