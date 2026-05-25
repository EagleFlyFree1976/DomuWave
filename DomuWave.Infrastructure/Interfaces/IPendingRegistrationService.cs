using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IPendingRegistrationService
{
    Task<PendingRegistration?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PendingRegistration?> GetPendingByEmailAsync(string email, CancellationToken ct);
    Task<PendingRegistration>  CreateAsync(PendingRegistration entity, CancellationToken ct);
    Task<PendingRegistration>  UpdateAsync(PendingRegistration entity, CancellationToken ct);
}
