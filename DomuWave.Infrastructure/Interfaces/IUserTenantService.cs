using CPQ.Core.Memberships;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

/// <summary>
/// Gestione delle associazioni utente-tenant.
/// Permette di assegnare pi� tenant a un utente e definirne uno come default.
/// </summary>
public interface IUserTenantService : IBaseService<UserTenant, int>
{
    /// <summary>Restituisce tutti i tenant assegnati a un utente.</summary>
    Task<IList<UserTenant>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken ct);

    /// <summary>Restituisce tutte le associazioni utente per un determinato tenant.</summary>
    new Task<IList<UserTenant>> GetByTenantIdAsync(
        Guid tenantId, IUser currentUser, CancellationToken cancellationToken);

    /// <summary>Restituisce il tenant di default per un utente. Null se non configurato.</summary>
    Task<UserTenant?> GetDefaultByUserIdAsync(
        long userId, IUser currentUser, CancellationToken ct);

    /// <summary>Restituisce una singola associazione per ID.</summary>
    new Task<UserTenant?> GetByIdAsync(
        int userTenantId, IUser currentUser, CancellationToken ct);

    

    /// <summary>
    /// Imposta un tenant come default per l'utente,
    /// rimuovendo automaticamente il flag dagli altri.
    /// </summary>
    Task<UserTenant> SetDefaultAsync(
        long userId, int userTenantId, IUser currentUser, CancellationToken ct);

    /// <summary>
    /// Imposta come default l'associazione utente↔tenant individuata per tenantId
    /// (comodo dal frontend, che ha il tenantId ma non lo userTenantId).
    /// </summary>
    Task<UserTenant?> SetDefaultByTenantAsync(
        long userId, Guid tenantId, IUser currentUser, CancellationToken ct);

    /// <summary>Soft delete dell'associazione.</summary>
    new Task<bool> DeleteAsync(
        int userTenantId, IUser currentUser, CancellationToken ct);


    Task<IList<Tenant>> GetByCondominoUserIdAsync(long userId, IUser currentUser, CancellationToken ct);

    /// <summary>
    /// Ruolo dell'utente nel tenant indicato ("Admin" | "Condomino"), in base a
    /// UserTenant.RoleCode. Ritorna null se non esiste un'associazione esplicita
    /// (il chiamante può ripiegare sul ruolo globale dell'utente auth).
    /// </summary>
    Task<string?> GetRoleCodeForTenantAsync(long userId, Guid tenantId, CancellationToken ct);

    /// <summary>True se l'utente è condòmino nel tenant indicato.</summary>
    Task<bool> IsCondominoInTenantAsync(long userId, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// True se l'utente è condòmino nel tenant a cui appartiene il condominio indicato.
    /// Comodo per i consumer che hanno il CondominiumId ma non il TenantId.
    /// </summary>
    Task<bool> IsCondominoInCondominiumAsync(long userId, int condominiumId, CancellationToken ct);

    /// <summary>
    /// Restituisce la lista dei condomini (con nome e tenantId) per un utente Condomino,
    /// deduplicata per tenant.
    /// </summary>
    Task<IList<CondominiumSummaryDto>> GetCondominiumsByCondominoUserIdAsync(long userId, IUser currentUser, CancellationToken ct);
}