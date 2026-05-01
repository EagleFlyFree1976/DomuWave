using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AssemblyAttendance;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteAssemblyAttendanceCommandConsumer : InMemoryConsumerBase<DeleteAssemblyAttendanceCommand, bool>
{
    private readonly IAssemblyAttendanceService _attendanceService;
    private readonly IUserService               _userService;

    public DeleteAssemblyAttendanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyAttendanceService attendanceService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _attendanceService = attendanceService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        DeleteAssemblyAttendanceCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        AssemblyAttendance? entity      = await _attendanceService.GetByIdAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return false;
        entity.SoftDelete(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
