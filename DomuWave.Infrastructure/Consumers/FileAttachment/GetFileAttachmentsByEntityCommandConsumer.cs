using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FileAttachment;
using DomuWave.Services.Dto.FileAttachment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.FileAttachment;

public class GetFileAttachmentsByEntityCommandConsumer
    : InMemoryConsumerBase<GetFileAttachmentsByEntityCommand, IList<FileAttachmentReadDto>>
{
    private readonly IUserService _userService;

    public GetFileAttachmentsByEntityCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<FileAttachmentReadDto>> Consume(
        GetFileAttachmentsByEntityCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var results = await session.Query<Models.FileAttachment>()
            .Where(f => f.EntityKey == command.EntityKey && f.EntityId == command.EntityId)
            .OrderByDescending(f => f.CreationDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.Select(f => f.ToReadDto()).ToList();
    }
}
