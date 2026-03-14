using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.DynamicFile;
using DomuWave.Services.Dto.DynamicFile;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.DynamicFile;

public class GetDynamicFilesByEntityCommandConsumer
    : InMemoryConsumerBase<GetDynamicFilesByEntityCommand, IList<DynamicFileReadDto>>
{
    private readonly IDynamicFileService _dynamicFileService;
    private readonly IUserService        _userService;

    public GetDynamicFilesByEntityCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IDynamicFileService dynamicFileService,
        IUserService        userService) : base(sessionFactoryProvider)
    {
        _dynamicFileService = dynamicFileService;
        _userService        = userService;
    }

    protected override async Task<IList<DynamicFileReadDto>> Consume(
        GetDynamicFilesByEntityCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        IList<Models.DynamicFile> files;

        if (!string.IsNullOrWhiteSpace(command.EntityFullName))
        {
            files = await _dynamicFileService
                .GetByEntityFullNameAsync(command.EntityFullName, command.EntityId, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            files = await _dynamicFileService
                .GetByEntityAsync(command.EntityName, command.EntityId, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        return files.Select(f => f.ToReadDto()).ToList();
    }
}
