using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using MassTransit.Initializers;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

/// <summary>
/// Implementazione di <see cref="IUserTenantService"/> tramite NHibernate.
/// Gestisce le associazioni utente-tenant con logica di default esclusivo.
/// </summary>
public class UserTenantService : BaseService, IUserTenantService
{
    public UserTenantService(
        ISessionFactoryProvider sessionFactoryProvider,
        ICacheManager cache)
        : base(sessionFactoryProvider, cache)
    {
    }

    public override string CacheRegion => "UserTenants";

    // ── Query ────────────────────────────────────────────────────────────────

    public async Task<IList<UserTenant>> GetByUserIdAsync(
        long userId, IUser currentUser, CancellationToken ct)
    {
        return await session.Query<UserTenant>()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Fetch(x => x.Tenant)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Tenant.Name)
            .ToListAsync(ct);
    }
    
    public async Task<IList<Tenant>> GetByCondominoUserIdAsync(long userId, IUser currentUser, CancellationToken ct)
    {
        List<UnitOwner> unitOwners = await session.Query<UnitOwner>()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Fetch(x => x.Tenant)

            .ToListAsync(ct);
        return unitOwners.ToList().Select(k=>k.Unit.Condominium.Tenant).ToList();
    }

    public async Task<UserTenant?> GetDefaultByUserIdAsync(
        long userId, IUser currentUser, CancellationToken ct)
    {
        return await session.Query<UserTenant>()
            .Where(x => x.UserId == userId && x.IsDefault && x.IsActive && !x.IsDeleted)
            .Fetch(x => x.Tenant)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserTenant?> GetByIdAsync(
        int userTenantId, IUser currentUser, CancellationToken ct)
    {
        return await session.Query<UserTenant>()
            .Where(x => x.Id == userTenantId && !x.IsDeleted)
            .Fetch(x => x.Tenant)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IList<UserTenant>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<UserTenant>().Where(k => !k.IsDeleted && k.UserId == currentUser.Id)
            .ToListAsync(cancellationToken);

    }

    public async Task<IList<UserTenant>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<UserTenant>().Where(k => !k.IsDeleted && k.Tenant.Id == tenantId).ToListAsync(cancellationToken);
    }

    public async Task<IList<UserTenant>> FindAsync(Expression<Func<UserTenant, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<UserTenant>().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<UserTenant> CreateAsync(UserTenant entity, IUser currentUser, CancellationToken cancellationToken)
    {
        // Verifica che l'associazione non esista già
        var existing = await session.Query<UserTenant>()
            .FirstOrDefaultAsync(x =>
                x.UserId == entity.UserId &&
                x.Tenant.Id == entity.Tenant.Id &&
                !x.IsDeleted, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException(
                $"L'utente {entity.UserId} è già associato al tenant {entity.Tenant.Id}.");

        // Se il nuovo è default, azzera gli altri utenti default sullo stesso tenant
        if (entity.IsDefault)
            await ClearDefaultFlagAsync(entity.Tenant.Id, null, cancellationToken);

        var tenant = await session.GetAsync<Tenant>(entity.Tenant.Id, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Tenant {entity.Tenant.Id} non trovato.");

        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);

        return entity;
    }

    public async Task<UserTenant> UpdateAsync(UserTenant input, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(input.Id, currentUser, cancellationToken)
                     ?? throw new KeyNotFoundException($"UserTenant {input.Id} non trovato.");

        // Se si sta impostando come default, azzera gli altri
        if (input.IsDefault && !entity.IsDefault)
            await ClearDefaultFlagAsync(entity.Tenant.Id, input.Id, cancellationToken);
 

        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }
 
       
 
    public async Task<UserTenant> SetDefaultAsync(
        long userId, int userTenantId, IUser currentUser, CancellationToken ct)
    {
        var entity = await GetByIdAsync(userTenantId, currentUser, ct)
                     ?? throw new KeyNotFoundException($"UserTenant {userTenantId} non trovato.");

        if (entity.UserId != userId)
            throw new InvalidOperationException(
                "Il UserTenant specificato non appartiene all'utente indicato.");

        // Rimuove il default dagli altri e imposta questo
        await ClearDefaultFlagAsync(entity.Tenant.Id, userTenantId, ct);

        entity.IsDefault = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct);
        await session.FlushAsync(ct);

        return entity;
    }

    public async Task<bool> DeleteAsync(
        int userTenantId, IUser currentUser, CancellationToken ct)
    {
        var entity = await GetByIdAsync(userTenantId, currentUser, ct);
        if (entity == null) return false;

        if (entity.IsDefault)
            throw new InvalidOperationException(
                "Impossibile eliminare il tenant di default. Impostare prima un altro tenant come default.");

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct);
        await session.FlushAsync(ct);

        return true;
    }

    public Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(Expression<Func<UserTenant, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<UserTenant>().Where(j => j.Id == id
        ).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IList<UserTenant> Items, int TotalCount)> GetPagedAsync(Expression<Func<UserTenant, bool>> filter,
        int pageNumber, int pageSize, Expression<Func<UserTenant, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    // ── Helpers privati ──────────────────────────────────────────────────────

    /// <summary>
    /// Azzera il flag IsDefault su tutte le associazioni dell'utente,
    /// escludendo opzionalmente un record specifico (quello che diventerà default).
    /// </summary>
    private async Task ClearDefaultFlagAsync(
        Guid tenantId, int? excludeUserTenantId, CancellationToken ct)
    {
        var currentDefaults = await session.Query<UserTenant>()
            .Where(x =>
                x.Tenant.Id == tenantId &&
                x.IsDefault &&
                !x.IsDeleted &&
                (excludeUserTenantId == null || x.Id != excludeUserTenantId))
            .ToListAsync(ct);

        foreach (var item in currentDefaults)
        {
            item.IsDefault = false;
            await session.SaveOrUpdateAsync(item, ct);
        }

        if (currentDefaults.Any())
            await session.FlushAsync(ct);
    }
}