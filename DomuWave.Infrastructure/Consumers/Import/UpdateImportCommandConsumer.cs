using DomuWave.Services.Command.Import;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class UpdateImportCommandConsumer : InMemoryConsumerBase<UpdateImportCommand, Models.Import.Import>

{
    private IUserService _userService;
    private IImportService _importService;

    public UpdateImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
    }

    protected override async Task<Models.Import.Import> Consume(UpdateImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var importToUpdate = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);
        if (importToUpdate == null)
            throw new NotFoundException("Import non trovato");


        var targetAccount = await session.GetAsync<Account>(evt.TargetAccountId, cancellationToken)
            .ConfigureAwait(false);
        if (targetAccount == null)
        {
            throw new NotFoundException("L'account specificato non esiste");
        }

        if (evt.ImportConfigurationDto == null)
        {
            throw new ValidatorException("Impostare la configurazione corretta");
        }

        if (evt.ImportConfigurationDto.Fields.Where(k=> k.TargetField != null && k.TargetField.Code.NotIsNullOrEmpty()).GroupBy(k => k.TargetField.Code).Select(k=>k.Count()).Any(i=>i > 1))
        {
            var listDups = evt.ImportConfigurationDto.Fields
                .Where(k => k.TargetField != null && k.TargetField.Code.NotIsNullOrEmpty())
                .GroupBy(k => k.TargetField.Code).Select(k =>
                {
                    return new {
                        Key = k.Key, 
                        Cnt = k.Count()
                    }
                    ;
                }).Where(k=>k.Cnt > 1).ToList();

            throw new ValidatorException($"Sono presenti dei duplicati");
        }

        var actualConfiguration = importToUpdate.Configuration;

        if (string.IsNullOrEmpty(actualConfiguration))
        {
            throw new ValidatorException("Configurazione non valida, rieseguire l'upload del file");
        }

        ImportConfigurationDto actualImportConfigurationDto = actualConfiguration.parseJson<ImportConfigurationDto>();
        if (actualImportConfigurationDto == null)
        {
            throw new ValidatorException("Configurazione non valida, rieseguire l'upload del file");
        }

        
        actualImportConfigurationDto.Fields = evt.ImportConfigurationDto.Fields;
        actualImportConfigurationDto.CultureInfo = evt.ImportConfigurationDto.CultureInfo;

        importToUpdate.TargetAccount = targetAccount;
        importToUpdate.Name = evt.Name;

        importToUpdate.Configuration = actualImportConfigurationDto.toJSon();

        importToUpdate.Trace(currentUser);

        await session.SaveOrUpdateAsync(importToUpdate, cancellationToken).ConfigureAwait(false);
        return importToUpdate;



    }
}