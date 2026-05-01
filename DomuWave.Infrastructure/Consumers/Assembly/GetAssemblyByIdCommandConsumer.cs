using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAssemblyByIdCommandConsumer : InMemoryConsumerBase<GetAssemblyByIdCommand, AssemblyReadDto>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public GetAssemblyByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<AssemblyReadDto> Consume(
        GetAssemblyByIdCommand command,
        IMediationContext       mediationContext,
        CancellationToken      cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var assembly    = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false);
        if (assembly == null) return null!;

        var dto = assembly.ToReadDto();

        var communication = await session.Query<Communication>()
            .Where(c => c.Assembly != null && c.Assembly.Id == assembly.Id && !c.IsDeleted)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (communication != null)
        {
            dto.CommunicationId    = communication.Id;
            dto.CommunicationTitle = communication.Name;
        }

        return dto;
    }
}
