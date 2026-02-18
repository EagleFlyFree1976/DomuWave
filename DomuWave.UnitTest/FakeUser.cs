using System.Security.Principal;
using CPQ.Core.Memberships;

namespace DomuWave.Tests.Helpers;

/// <summary>
/// Implementazione minimale di IUser per i test.
/// </summary>
public class FakeUser : IUser
{
    private bool _isAuthenticated;
    private string? _name;
    private int _id;
    private bool _isAuthenticated1;
    private string _name1;
    public string Code { get; set; }

    int IUser.Id
    {
        get => _id;
        set => _id = value;
    }

    public string AvatarPath { get; set; }
    public string Token { get; set; }
    public string Culture { get; set; }
    public long Id { get; set; } = 1L;
    public string Username { get; set; } = "test.user";
    public string Email { get; set; } = "test@domuwave.it";
    public string FirstName { get; set; }
    public string FullName { get; set; } = "Test User";
    public bool IsActive { get; set; }
    public bool IsSystemUser { get; set; }
    public Guid TenantId { get; set; } = Guid.NewGuid();
    public IList<string> Roles { get; set; } = new List<string> { "Admin" };

    public static FakeUser Create(Guid? tenantId = null)
    {
        var user = new FakeUser();
        if (tenantId.HasValue)
            user.TenantId = tenantId.Value;
        return user;
    }

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

    public string Password { get; set; }
    public bool PasswordExpired { get; set; }
    public string Path { get; set; }
    public IUser Supervisor { get; set; }
    public bool IsFieldForceUser { get; set; }
    public bool IsDeleted { get; set; }
    public bool ItsNotMe { get; set; }
    public string LastName { get; set; }

    bool IIdentity.IsAuthenticated => _isAuthenticated;

    public string Login { get; }

    string? IIdentity.Name => _name;

    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }

    public IIdentity? Identity { get; }
}
