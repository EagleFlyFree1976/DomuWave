using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

/// <summary>
/// Provisioning dell'account di piattaforma per un occupante (proprietario/inquilino).
/// Trova-o-crea l'utente auth associato all'email, lo collega al tenant del condominio
/// e — se l'email è reale — invia l'invito per impostare la password.
/// </summary>
public interface IOccupantUserProvisioningService
{
    /// <summary>
    /// Garantisce l'esistenza di un utente auth per l'occupante.
    /// </summary>
    /// <param name="email">Email dell'occupante (può essere null/vuota: si genera un placeholder).</param>
    /// <param name="firstName">Nome.</param>
    /// <param name="lastName">Cognome.</param>
    /// <param name="tenant">Tenant del condominio a cui associare l'utente.</param>
    /// <param name="currentUser">Utente che esegue l'operazione (admin).</param>
    /// <returns>
    /// Id dell'utente auth (long, da assegnare a UnitOwner/UnitTenant.UserId)
    /// e flag che indica se l'email usata è un placeholder (nessun invito inviato).
    /// </returns>
    Task<OccupantProvisioningResult> EnsureUserAsync(
        string? email,
        string? firstName,
        string? lastName,
        Tenant tenant,
        IUser currentUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Aggiorna l'email dell'utente auth collegato e (se l'email è reale) invia
    /// l'invito/reset password. Usato quando l'admin recupera l'email reale.
    /// </summary>
    Task ChangeEmailAndInviteAsync(
        long userId,
        string newEmail,
        IUser currentUser,
        CancellationToken cancellationToken);
}

public record OccupantProvisioningResult(long UserId, bool IsPlaceholderEmail);
