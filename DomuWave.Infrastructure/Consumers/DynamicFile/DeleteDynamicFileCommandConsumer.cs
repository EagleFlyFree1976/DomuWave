using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.DynamicFile;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.FileStorage;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.DynamicFile;

public class DeleteDynamicFileCommandConsumer
    : InMemoryConsumerBase<DeleteDynamicFileCommand, bool>
{
    private readonly IDynamicFileService       _dynamicFileService;
    private readonly IFileStorageProviderFactory _storageFactory;
    private readonly IUserService              _userService;

    public DeleteDynamicFileCommandConsumer(
        ISessionFactoryProvider      sessionFactoryProvider,
        IDynamicFileService          dynamicFileService,
        IFileStorageProviderFactory  storageFactory,
        IUserService                 userService) : base(sessionFactoryProvider)
    {
        _dynamicFileService = dynamicFileService;
        _storageFactory     = storageFactory;
        _userService        = userService;
    }

    protected override async Task<bool> Consume(
        DeleteDynamicFileCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _dynamicFileService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return false;

        // Elimina risorsa esterna (per provider non-Database)
        var storageTypeId = existing.StorageType?.Id ?? 0;
        if (storageTypeId != 0 && !string.IsNullOrWhiteSpace(existing.StorageReference))
        {
            var provider = _storageFactory.GetById(storageTypeId);
            await provider
                .DeleteAsync(existing.StorageReference, cancellationToken)
                .ConfigureAwait(false);
        }

        await _dynamicFileService
            .DeleteAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
