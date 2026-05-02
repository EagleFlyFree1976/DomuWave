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
using NHibernate.Linq;
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

        // Prepopulate attendances for all active owners (Assente by default)
        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Condominium.Id == condominium.Id && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var millesimali = await session.Query<UnitMillesimal>()
            .Where(m => m.MillesimalTable.Condominium.Id == condominium.Id
                     && m.MillesimalTable.IsDefault
                     && !m.IsDeleted)
            .Select(m => new { UnitId = m.Unit.Id, m.Millesimal })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var millesimaliByUnit = millesimali
            .GroupBy(m => m.UnitId)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Millesimal));

        var assenteType = await session.GetAsync<AttendanceTypeLookup>(AttendanceTypeLookup.Assente, cancellationToken).ConfigureAwait(false)!;

        foreach (var owner in owners)
        {
            millesimaliByUnit.TryGetValue(owner.Unit?.Id ?? 0, out var millesimalValue);
            var attendance = new AssemblyAttendance
            {
                Assembly        = entity,
                Tenant          = entity.Tenant,
                UnitOwner       = owner,
                AttendanceType  = assenteType,
                MillesimalValue = millesimalValue,
            };
            attendance.Trace(currentUser);
            await session.SaveAsync(attendance, cancellationToken).ConfigureAwait(false);
        }

        if (owners.Count > 0)
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
