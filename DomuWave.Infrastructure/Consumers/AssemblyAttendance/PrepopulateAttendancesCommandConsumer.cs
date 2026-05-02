using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AssemblyAttendance;
using DomuWave.Services.Dto.AssemblyAttendance;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class PrepopulateAttendancesCommandConsumer : InMemoryConsumerBase<PrepopulateAttendancesCommand, IList<AssemblyAttendanceReadDto>>
{
    private readonly IUserService _userService;

    public PrepopulateAttendancesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<AssemblyAttendanceReadDto>> Consume(
        PrepopulateAttendancesCommand command,
        IMediationContext              mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var assembly = await session.GetAsync<Models.Assembly>(command.AssemblyId, cancellationToken).ConfigureAwait(false)
                       ?? throw new NotFoundException("Assemblea non trovata.");

        // Carica tutti i proprietari attivi del condominio
        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Condominium.Id == assembly.Condominium.Id && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Carica i proprietari già registrati (inclusi i soft-deleted: il vincolo UQ non filtra su IsDeleted)
        var registeredOwnerIds = await session.Query<AssemblyAttendance>()
            .Where(a => a.Assembly.Id == command.AssemblyId)
            .Select(a => a.UnitOwner.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var registeredSet = new HashSet<int>(registeredOwnerIds);

        var assenteType = await session.GetAsync<AttendanceTypeLookup>(AttendanceTypeLookup.Assente, cancellationToken).ConfigureAwait(false)!;

        // Carica i valori millesimali dalla tabella attiva (IsActive=true, IsDraft=false) del condominio
        var condominiumId = assembly.Condominium.Id;
        var millesimali = await session.Query<UnitMillesimal>()
            .Where(m => m.MillesimalTable.Condominium.Id == condominiumId
                     && m.MillesimalTable.IsDefault
                     && !m.IsDeleted)
            .Select(m => new { UnitId = m.Unit.Id, m.Millesimal })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var millesimaliByUnit = millesimali
            .GroupBy(m => m.UnitId)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Millesimal));

        var created = new List<AssemblyAttendance>();
        foreach (var owner in owners.Where(o => !registeredSet.Contains(o.Id)))
        {
            millesimaliByUnit.TryGetValue(owner.Unit?.Id ?? 0, out var millesimalValue);
            var entity = new AssemblyAttendance
            {
                Assembly        = assembly,
                Tenant          = assembly.Tenant,
                UnitOwner       = owner,
                AttendanceType  = assenteType,
                MillesimalValue = millesimalValue,
            };
            entity.Trace(currentUser);
            await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
            created.Add(entity);
        }

        if (created.Count > 0)
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // Ricarica tutto l'elenco presenze aggiornato
        var all = await session.Query<AssemblyAttendance>()
            .Where(a => a.Assembly.Id == command.AssemblyId && !a.IsDeleted)
            .OrderBy(a => a.UnitOwner.LastName).ThenBy(a => a.UnitOwner.FirstName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return all.Select(a => a.ToReadDto()).ToList();
    }
}
