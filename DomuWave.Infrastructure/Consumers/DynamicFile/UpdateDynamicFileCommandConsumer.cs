using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.DynamicFile;
using DomuWave.Services.Dto.DynamicFile;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.DynamicFile;

public class UpdateDynamicFileCommandConsumer
    : InMemoryConsumerBase<UpdateDynamicFileCommand, DynamicFileReadDto>
{
    private readonly IDynamicFileService _dynamicFileService;
    private readonly IUserService        _userService;

    public UpdateDynamicFileCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IDynamicFileService     dynamicFileService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _dynamicFileService = dynamicFileService;
        _userService        = userService;
    }

    protected override async Task<DynamicFileReadDto> Consume(
        UpdateDynamicFileCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _dynamicFileService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        existing.ApplyUpdate(command.Dto);

        var updated = await _dynamicFileService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
