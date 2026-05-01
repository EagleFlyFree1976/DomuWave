using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAssembliesByCondominiumCommandConsumer : InMemoryConsumerBase<GetAssembliesByCondominiumCommand, IList<AssemblyReadDto>>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public GetAssembliesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<IList<AssemblyReadDto>> Consume(
        GetAssembliesByCondominiumCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var assemblies  = await _assemblyService.GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false);
        return assemblies.Select(a => a.ToReadDto()).ToList();
    }
}
