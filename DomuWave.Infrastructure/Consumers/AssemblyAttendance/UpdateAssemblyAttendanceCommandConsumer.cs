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
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateAssemblyAttendanceCommandConsumer : InMemoryConsumerBase<UpdateAssemblyAttendanceCommand, AssemblyAttendanceReadDto>
{
    private readonly IAssemblyAttendanceService _attendanceService;
    private readonly IUserService               _userService;

    public UpdateAssemblyAttendanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyAttendanceService attendanceService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _attendanceService = attendanceService;
        _userService       = userService;
    }

    protected override async Task<AssemblyAttendanceReadDto> Consume(
        UpdateAssemblyAttendanceCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _attendanceService.GetByIdAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Presenza non trovata.");

        AttendanceTypeLookup? attendanceType = null;
        if (command.Dto.AttendanceTypeId.HasValue)
            attendanceType = await session.GetAsync<AttendanceTypeLookup>(command.Dto.AttendanceTypeId.Value, cancellationToken).ConfigureAwait(false);

        entity.ApplyUpdate(command.Dto, attendanceType);
        entity.Trace(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
