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

public class CreateAssemblyCommandConsumer : InMemoryConsumerBase<CreateAssemblyCommand, AssemblyReadDto>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public CreateAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<AssemblyReadDto> Consume(
        CreateAssemblyCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo è obbligatorio.");

        var condominium = await session.GetAsync<Models.Condominium>(command.Dto.CondominiumId, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        var assemblyType = await session.GetAsync<AssemblyTypeLookup>(command.Dto.AssemblyTypeId, cancellationToken).ConfigureAwait(false)
                           ?? throw new NotFoundException("Tipo assemblea non valido.");

        var status = await session.GetAsync<AssemblyStatusLookup>(AssemblyStatusLookup.Bozza, cancellationToken).ConfigureAwait(false)!;

        FiscalYear? fiscalYear = null;
        if (command.Dto.FiscalYearId.HasValue)
            fiscalYear = await session.GetAsync<FiscalYear>(command.Dto.FiscalYearId.Value, cancellationToken).ConfigureAwait(false);

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant, assemblyType, status, fiscalYear);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
