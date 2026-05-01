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

public class CreateAssemblyAttendanceCommandConsumer : InMemoryConsumerBase<CreateAssemblyAttendanceCommand, AssemblyAttendanceReadDto>
{
    private readonly IUserService _userService;

    public CreateAssemblyAttendanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<AssemblyAttendanceReadDto> Consume(
        CreateAssemblyAttendanceCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var assembly = await session.GetAsync<Models.Assembly>(command.Dto.AssemblyId, cancellationToken).ConfigureAwait(false)
                       ?? throw new NotFoundException("Assemblea non trovata.");

        var unitOwner = await session.GetAsync<UnitOwner>(command.Dto.UnitOwnerId, cancellationToken).ConfigureAwait(false)
                        ?? throw new NotFoundException("Proprietario non trovato.");

        var alreadyExists = await session.Query<AssemblyAttendance>()
            .AnyAsync(a => a.Assembly.Id == command.Dto.AssemblyId && a.UnitOwner.Id == command.Dto.UnitOwnerId && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (alreadyExists)
            throw new ValidatorException("Questo proprietario è già registrato per questa assemblea.");

        var attendanceType = await session.GetAsync<AttendanceTypeLookup>(command.Dto.AttendanceTypeId, cancellationToken).ConfigureAwait(false)
                             ?? throw new NotFoundException("Tipo presenza non valido.");

        var entity = command.Dto.ToEntity(assembly, assembly.Tenant, unitOwner, attendanceType);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
