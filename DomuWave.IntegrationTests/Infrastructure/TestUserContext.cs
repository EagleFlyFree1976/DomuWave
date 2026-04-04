using CPQ.Core.Memberships;
using System.Security.Principal;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// Fake IUser used throughout integration tests.
/// Mirrors FakeUser from unit tests but is self-contained here.
/// </summary>
public class TestUserContext : IUser
{
    private int  _id;
    private bool _isAuthenticated;
    private string _name = "integration.test";

    int IUser.Id   { get => _id;              set => _id = value; }
    bool IUser.IsAuthenticated { get => _isAuthenticated; set => _isAuthenticated = value; }
    string IUser.Name          { get => _name;             set => _name = value; }

    public long   Id            { get; set; } = 1L;
    public string Code          { get; set; } = "TEST";
    public string AvatarPath    { get; set; } = string.Empty;
    public string Token         { get; set; } = "integration-test-token";
    public string Culture       { get; set; } = "it-IT";
    public string Username      { get; set; } = "integration.test";
    public string Email         { get; set; } = "integrationtest@domuwave.it";
    public string FirstName     { get; set; } = "Integration";
    public string LastName      { get; set; } = "Test";
    public string FullName      { get; set; } = "Integration Test User";
    public string Password      { get; set; } = string.Empty;
    public string Path          { get; set; } = string.Empty;
    public bool   IsActive      { get; set; } = true;
    public bool   IsSystemUser  { get; set; } = false;
    public bool   IsDeleted     { get; set; } = false;
    public bool   ItsNotMe      { get; set; } = false;
    public bool   IsFieldForceUser { get; set; } = false;
    public bool   PasswordExpired  { get; set; } = false;
    public Guid   TenantId      { get; set; } = Guid.Empty;
    public IList<string> Roles  { get; set; } = new List<string> { "Admin", "SuperAdmin" };
    public IUser? Supervisor    { get; set; }

    // IIdentity
    public string? AuthenticationType => "IntegrationTest";
    bool IIdentity.IsAuthenticated    => true;
    string? IIdentity.Name            => Username;

    // IPrincipal
    public IIdentity? Identity        => this as IIdentity;
    public bool IsInRole(string role) => Roles.Contains(role);

    public string Login { get; } = "integration.test";

    /// <summary>
    /// Creates a test user from the provided configuration values.
    /// </summary>
    public static TestUserContext Create(int userId, Guid tenantId, string fullName, string email)
    {
        var user = new TestUserContext
        {
            Id       = userId,
            TenantId = tenantId,
            FullName = fullName,
            Email    = email,
        };
        ((IUser)user).Id = userId;
        ((IUser)user).IsAuthenticated = true;
        return user;
    }
}
