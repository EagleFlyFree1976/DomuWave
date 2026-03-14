using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.DynamicFile;
using DomuWave.Services.Dto.DynamicFile;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Interfaces.FileStorage;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.DynamicFile;

public class UploadDynamicFileCommandConsumer
    : InMemoryConsumerBase<UploadDynamicFileCommand, DynamicFileReadDto>
{
    private readonly ITenantService              _tenantService;
    private readonly IFileStorageProviderFactory _storageFactory;
    private readonly IUserService                _userService;

    public UploadDynamicFileCommandConsumer(
        ISessionFactoryProvider      sessionFactoryProvider,
        ITenantService               tenantService,
        IFileStorageProviderFactory  storageFactory,
        IUserService                 userService) : base(sessionFactoryProvider)
    {
        _tenantService  = tenantService;
        _storageFactory = storageFactory;
        _userService    = userService;
    }

    protected override async Task<DynamicFileReadDto> Consume(
        UploadDynamicFileCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException("Tenant non trovato.");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.EntityFullName))
            throw new ValidatorException("EntityFullName e' obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.EntityName))
            throw new ValidatorException("EntityName e' obbligatorio.");
        if (dto.EntityId <= 0)
            throw new ValidatorException("EntityId non valido.");
        if (string.IsNullOrWhiteSpace(dto.FileName))
            throw new ValidatorException("Il nome del file e' obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Base64Data))
            throw new ValidatorException("Il contenuto del file e' obbligatorio.");

        // Recupera il tipo storage predefinito
        var provider    = _storageFactory.GetDefault();
        var storageType = await session.Query<DynamicFileStorageType>()
            .FirstOrDefaultAsync(x => x.Id == provider.StorageTypeId, cancellationToken)
            .ConfigureAwait(false);
        if (storageType == null)
            throw new NotFoundException("Tipo di storage non trovato.");

        // Crea entita' metadati
        var entity = dto.ToEntity(tenant, storageType);
        entity.Trace(currentUser);

        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);

        // Persiste il contenuto tramite provider
        if (provider.StorageTypeId == 0) // Database
        {
            var content = new DynamicFileContent
            {
                File       = entity,
                Base64Data = dto.Base64Data,
            };
            content.Trace(currentUser);
            await session.SaveAsync(content, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var storageRef = await provider
                .SaveAsync(entity.Id, dto.Base64Data, dto.FileName, dto.ContentType, cancellationToken)
                .ConfigureAwait(false);

            entity.StorageReference = storageRef;
            await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
