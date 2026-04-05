using System.Security.Principal;
using CPQ.Core.Memberships;

namespace DomuWave.Tests.Helpers;

/// <summary>
/// Implementazione minimale di <see cref="IUser"/> usata come utente corrente fittizio
/// negli unit test, in sostituzione dell'utente autenticato reale.
///
/// Nota strutturale: la classe implementa <see cref="IUser"/> tramite interfaccia esplicita
/// per alcune proprietà (es. <c>Id</c>, <c>IsAuthenticated</c>, <c>Name</c>) e tramite
/// proprietà pubbliche per altre. Questo produce campi backing duplicati
/// (<c>_isAuthenticated</c> / <c>_isAuthenticated1</c>, <c>_name</c> / <c>_name1</c>)
/// che esistono perché <see cref="IUser"/> eredita sia da un'interfaccia di dominio
/// (con <c>Id</c> come <c>int</c>) sia da <see cref="IIdentity"/> (con <c>Name</c> come
/// <c>string?</c>). Nei test è sufficiente impostare le proprietà pubbliche (<c>Id</c>,
/// <c>FullName</c>, <c>TenantId</c>); le implementazioni esplicite non vengono usate.
///
/// Valori predefiniti pronti all'uso:
/// <list type="bullet">
///   <item><c>Id = 1</c></item>
///   <item><c>Username = "test.user"</c></item>
///   <item><c>Email = "test@domuwave.it"</c></item>
///   <item><c>FullName = "Test User"</c></item>
///   <item><c>Roles = ["Admin"]</c></item>
///   <item><c>TenantId = Guid.NewGuid()</c> (casuale a ogni istanza)</item>
/// </list>
/// </summary>
public class FakeUser : IUser
{
    // ── Campi backing per le implementazioni esplicite di interfaccia ──────────
    // Questi esistono perché IUser eredita da più interfacce che dichiarano
    // proprietà omonime con tipi diversi (int vs long per Id, bool per IsAuthenticated).

    private bool _isAuthenticated;   // usato da IIdentity.IsAuthenticated (esplicito)
    private string? _name;           // usato da IIdentity.Name (esplicito)
    private int _id;                 // usato da IUser.Id (esplicito, tipo int)
    private bool _isAuthenticated1;  // usato da IUser.IsAuthenticated (esplicito, potrebbe essere un bug di design)
    private string _name1;           // usato da IUser.Name (esplicito)

    // ── Proprietà pubbliche usate nei test ─────────────────────────────────────

    public string Code { get; set; }

    /// <summary>
    /// Implementazione esplicita di <c>IUser.Id</c> (int).
    /// Nei test usare invece la proprietà pubblica <c>Id</c> (long).
    /// </summary>
    int IUser.Id
    {
        get => _id;
        set => _id = value;
    }

    public string AvatarPath { get; set; }
    public string Token { get; set; }
    public string Culture { get; set; }

    /// <summary>Identificativo utente. Valore predefinito: 1.</summary>
    public long Id { get; set; } = 1L;

    /// <summary>Username di accesso. Valore predefinito: "test.user".</summary>
    public string Username { get; set; } = "test.user";

    /// <summary>Email dell'utente. Valore predefinito: "test@domuwave.it".</summary>
    public string Email { get; set; } = "test@domuwave.it";

    public string FirstName { get; set; }

    /// <summary>Nome completo. Valore predefinito: "Test User".</summary>
    public string FullName { get; set; } = "Test User";

    public bool IsActive { get; set; }
    public bool IsSystemUser { get; set; }

    /// <summary>
    /// TenantId dell'utente. Viene inizializzato con un Guid casuale;
    /// usare <see cref="Create(Guid?)"/> per specificarne uno deterministico.
    /// </summary>
    public Guid TenantId { get; set; } = Guid.NewGuid();

    /// <summary>Ruoli dell'utente. Valore predefinito: ["Admin"].</summary>
    public IList<string> Roles { get; set; } = new List<string> { "Admin" };

    /// <summary>
    /// Factory method: crea un <see cref="FakeUser"/> con valori predefiniti,
    /// opzionalmente forzando il <paramref name="tenantId"/> specificato.
    /// </summary>
    /// <param name="tenantId">
    /// Se fornito, sovrascrive il TenantId casuale. Utile quando il test deve
    /// verificare il filtraggio per tenant.
    /// </param>
    public static FakeUser Create(Guid? tenantId = null)
    {
        var user = new FakeUser();
        if (tenantId.HasValue)
            user.TenantId = tenantId.Value;
        return user;
    }

    // ── Implementazioni esplicite di IIdentity ────────────────────────────────

    public string? AuthenticationType { get; }

    bool IUser.IsAuthenticated
    {
        get => _isAuthenticated1;
        set => _isAuthenticated1 = value;
    }

    string IUser.Name
    {
        get => _name1;
        set => _name1 = value;
    }

    // ── Altre proprietà di IUser non rilevanti per i test ─────────────────────

    public string Password { get; set; }
    public bool PasswordExpired { get; set; }
    public string Path { get; set; }
    public IUser Supervisor { get; set; }
    public bool IsFieldForceUser { get; set; }
    public bool IsDeleted { get; set; }
    public bool ItsNotMe { get; set; }
    public string LastName { get; set; }

    // IIdentity esplicito (ereditato da IUser → IPrincipal → IIdentity)
    bool IIdentity.IsAuthenticated => _isAuthenticated;

    public string Login { get; }

    string? IIdentity.Name => _name;

    /// <summary>
    /// Non implementato: non viene usato negli unit test.
    /// Per verificare i ruoli nei test, ispezionare direttamente <see cref="Roles"/>.
    /// </summary>
    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }

    public IIdentity? Identity { get; }
}
