using CPQ.Core.Memberships;
using DomuWave.Domain.Models;

namespace DomuWave.Services.Interfaces;

/// <summary>
/// Gestione delle associazioni utente-tenant.
/// Permette di assegnare più tenant a un utente e definirne uno come default.
/// </summary>
public interface IUserTenantService : IBaseService<UserTenant, int>
{
    /// <summary>Restituisce tutti i tenant assegnati a un utente.</summary>
    Task<IList<UserTenant>> GetByUserIdAsync(
        long userId, IUser currentUser, CancellationToken ct);

    /// <summary>Restituisce il tenant di default per un utente. Null se non configurato.</summary>
    Task<UserTenant?> GetDefaultByUserIdAsync(
        long userId, IUser currentUser, CancellationToken ct);

    /// <summary>Restituisce una singola associazione per ID.</summary>
    Task<UserTenant?> GetByIdAsync(
        int userTenantId, IUser currentUser, CancellationToken ct);

    

    /// <summary>
    /// Imposta un tenant come default per l'utente,
    /// rimuovendo automaticamente il flag dagli altri.
    /// </summary>
    Task<UserTenant> SetDefaultAsync(
        long userId, int userTenantId, IUser currentUser, CancellationToken ct);

    /// <summary>Soft delete dell'associazione.</summary>
    Task<bool> DeleteAsync(
        int userTenantId, IUser currentUser, CancellationToken ct);
}