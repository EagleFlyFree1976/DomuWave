using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AssemblyAttendance;
using DomuWave.Services.Dto.AssemblyAttendance;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAttendancesByAssemblyCommandConsumer : InMemoryConsumerBase<GetAttendancesByAssemblyCommand, IList<AssemblyAttendanceReadDto>>
{
    private readonly IAssemblyAttendanceService _attendanceService;
    private readonly IUserService               _userService;

    public GetAttendancesByAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyAttendanceService attendanceService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _attendanceService = attendanceService;
        _userService       = userService;
    }

    protected override async Task<IList<AssemblyAttendanceReadDto>> Consume(
        GetAttendancesByAssemblyCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser  = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var attendances  = await _attendanceService.GetByAssemblyIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false);
        return attendances.Select(a => a.ToReadDto()).ToList();
    }
}
