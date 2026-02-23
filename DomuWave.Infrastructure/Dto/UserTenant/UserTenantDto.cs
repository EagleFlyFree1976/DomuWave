 

namespace DomuWave.Services.Dto.UserTenants;

// ── Read ──────────────────────────────────────────────────────────────────────

/// <summary>Dati restituiti per un'associazione utente-tenant.</summary>
public class UserTenantReadDto
{
    public int UserTenantId { get; set; }
    public long UserId { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

// ── Create ────────────────────────────────────────────────────────────────────

/// <summary>Payload per aggiungere un tenant a un utente.</summary>
public class UserTenantCreateDto
{
    /// <summary>ID dell'utente a cui assegnare il tenant.</summary>
    public long UserId { get; set; }

    /// <summary>Tenant da assegnare.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Se true, diventa il tenant di default (gli altri vengono aggiornati a false).</summary>
    public bool IsDefault { get; set; }

    /// <summary>Attiva o disattiva l'associazione.</summary>
    public bool IsActive { get; set; } = true;
}

// ── Update ────────────────────────────────────────────────────────────────────

/// <summary>Payload per modificare un'associazione esistente.</summary>
public class UserTenantUpdateDto
{
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

// ── Set Default ───────────────────────────────────────────────────────────────

/// <summary>Imposta il tenant di default per un utente.</summary>
public class SetDefaultTenantDto
{
    /// <summary>ID del UserTenant da impostare come default.</summary>
    public int UserTenantId { get; set; }
}