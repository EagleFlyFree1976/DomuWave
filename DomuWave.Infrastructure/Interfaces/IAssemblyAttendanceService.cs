using CPQ.Core.Memberships;
using CPQ.Core.Services;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IAssemblyAttendanceService : IBaseService<AssemblyAttendance, int>
{
    Task<IList<AssemblyAttendance>> GetByAssemblyIdAsync(int assemblyId, IUser currentUser, CancellationToken cancellationToken);
}
