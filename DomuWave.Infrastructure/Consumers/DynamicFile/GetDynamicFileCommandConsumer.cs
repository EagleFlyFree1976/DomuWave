using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.DynamicFile;
using DomuWave.Services.Dto.DynamicFile;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Interfaces.FileStorage;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.DynamicFile;

public class GetDynamicFileCommandConsumer
    : InMemoryConsumerBase<GetDynamicFileCommand, DynamicFileReadDto>
{
    private readonly IDynamicFileService       _dynamicFileService;
    private readonly IFileStorageProviderFactory _storageFactory;
    private readonly IUserService              _userService;

    public GetDynamicFileCommandConsumer(
        ISessionFactoryProvider      sessionFactoryProvider,
        IDynamicFileService          dynamicFileService,
        IFileStorageProviderFactory  storageFactory,
        IUserService                 userService) : base(sessionFactoryProvider)
    {
        _dynamicFileService = dynamicFileService;
        _storageFactory     = storageFactory;
        _userService        = userService;
    }

    protected override async Task<DynamicFileReadDto> Consume(
        GetDynamicFileCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var file = await _dynamicFileService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (file == null) return null;

        string? base64Data = null;

        if (command.IncludeContent)
        {
            var storageTypeId = file.StorageType?.Id ?? 0;
            var provider      = _storageFactory.GetById(storageTypeId);

            if (storageTypeId == 0) // Database
            {
                var content = await _dynamicFileService
                    .GetContentAsync(file.Id, currentUser, cancellationToken)
                    .ConfigureAwait(false);
                base64Data = content?.Base64Data;
            }
            else
            {
                base64Data = await provider
                    .ReadAsync(file.StorageReference!, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return file.ToReadDto(base64Data);
    }
}
