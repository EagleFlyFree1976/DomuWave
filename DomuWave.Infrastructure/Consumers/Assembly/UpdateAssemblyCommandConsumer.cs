using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateAssemblyCommandConsumer : InMemoryConsumerBase<UpdateAssemblyCommand, AssemblyReadDto>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public UpdateAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<AssemblyReadDto> Consume(
        UpdateAssemblyCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Assemblea non trovata.");

        AssemblyTypeLookup? assemblyType = null;
        if (command.Dto.AssemblyTypeId.HasValue)
            assemblyType = await session.GetAsync<AssemblyTypeLookup>(command.Dto.AssemblyTypeId.Value, cancellationToken).ConfigureAwait(false);

        FiscalYear? fiscalYear = null;
        if (command.Dto.FiscalYearId.HasValue)
            fiscalYear = await session.GetAsync<FiscalYear>(command.Dto.FiscalYearId.Value, cancellationToken).ConfigureAwait(false);

        entity.ApplyUpdate(command.Dto, assemblyType, fiscalYear);
        entity.Trace(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
