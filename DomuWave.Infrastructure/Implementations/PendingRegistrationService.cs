using CPQ.Core;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class PendingRegistrationService : BaseService, IPendingRegistrationService
{
    public PendingRegistrationService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "PendingRegistrations";

    public async Task<PendingRegistration?> GetByIdAsync(Guid id, CancellationToken ct)
        => await session.Query<PendingRegistration>()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<PendingRegistration?> GetPendingByEmailAsync(string email, CancellationToken ct)
        => await session.Query<PendingRegistration>()
            .FirstOrDefaultAsync(x => x.Email == email && x.Status == PendingRegistrationStatus.Pending, ct)
            .ConfigureAwait(false);

    public async Task<PendingRegistration?> GetByTokenAsync(string token, CancellationToken ct)
        => await session.Query<PendingRegistration>()
            .FirstOrDefaultAsync(x => x.VerificationToken == token, ct)
            .ConfigureAwait(false);

    public async Task<PendingRegistration> CreateAsync(PendingRegistration entity, CancellationToken ct)
    {
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<PendingRegistration> UpdateAsync(PendingRegistration entity, CancellationToken ct)
    {
        await session.UpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }
}
