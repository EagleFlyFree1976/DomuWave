using CPQ.Core.Memberships;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// ClaimsPrincipal che implementa anche IUser, così OxCoreControllerBase può fare
/// "HttpContext.User as IUser" e ottenere l'utente di test.
/// </summary>
public class TestClaimsPrincipal(TestUserContext u)
    : ClaimsPrincipal(new ClaimsIdentity(
        new[] { new Claim(ClaimTypes.Name, u.Username) },
        "IntegrationTest")), IUser
{

    // IIdentity (required by IUser : IIdentity)
    string? IIdentity.AuthenticationType => "IntegrationTest";
    bool    IIdentity.IsAuthenticated    => true;
    string? IIdentity.Name               => u.Username;

    // IUser
    int    IUser.Id              { get => (int)u.Id;     set { } }
    string IUser.Name            { get => u.Username;    set { } }
    bool   IUser.IsAuthenticated { get => true;          set { } }
    string IUser.Code            { get => u.Code;        set { } }
    string IUser.AvatarPath      { get => u.AvatarPath;  set { } }
    string IUser.Token           { get => u.Token;       set { } }
    string IUser.Culture         { get => u.Culture;     set { } }
    string IUser.Email           { get => u.Email;       set { } }
    string IUser.FirstName       { get => u.FirstName;   set { } }
    string IUser.FullName        => u.FullName;
    string IUser.LastName        { get => u.LastName;    set { } }
    string IUser.Login           => u.Login;
    string IUser.Password        { get => u.Password;    set { } }
    string IUser.Path            { get => u.Path;        set { } }
    bool   IUser.IsActive        { get => u.IsActive;    set { } }
    bool   IUser.IsSystemUser    { get => u.IsSystemUser;    set { } }
    bool   IUser.IsDeleted       { get => u.IsDeleted;       set { } }
    bool   IUser.ItsNotMe        { get => u.ItsNotMe;        set { } }
    bool   IUser.IsFieldForceUser{ get => u.IsFieldForceUser; set { } }
    bool   IUser.PasswordExpired { get => u.PasswordExpired;  set { } }
    IUser  IUser.Supervisor      { get => u.Supervisor!;      set { } }
}

/// <summary>
/// Middleware che risolve l'utente di test dal token Bearer e imposta HttpContext.User.
/// Supporta tutti i ruoli: DMW_SU, PRT_ADM, AMS, CLB, Condomino.
/// </summary>
public class TestAuthMiddleware(
    RequestDelegate              next,
    IntegrationTestFactory       factory)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Risolve l'utente dal token Bearer (es. "test-token-prt_adm")
        var token = context.Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", string.Empty)
            .Trim();

        var user = factory.Users.Values.FirstOrDefault(u => u.Token == token)
                   ?? factory.TestUser;

        context.User = new TestClaimsPrincipal(user);

        await next(context);
    }
}
