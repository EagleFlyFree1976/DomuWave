using Auth.Services.Extensions;
using Auth.Services.Interfaces;
using Auth.Services.Orchestators;
using Auth.Services.Command;
using CPQ.Core.Security;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Clients.Response;
using NHibernate.Linq;

namespace DomuWave.Microservice.Clients;

/// <summary>
/// Implementazione locale di IAuthorizationClient che delega a IAuthUserService/IAuthorizationManager
/// invece di usare HTTP. Usata quando Auth.Services è embedded nella stessa applicazione.
/// Per tornare al microservizio esterno basta registrare il Refit client al posto di questo.
/// </summary>
public class LocalAuthorizationClient(
    IAuthUserService authUserService,
    IAuthorizationManager authorizationManager,
    AuthOrchestator authOrchestator) : IAuthorizationClient
{
    public async Task<IList<UserAuthorizationResponse>> GetAllUserAuthorizations(
        string token, int idUser, CancellationToken cancellationToken)
    {
        var authUser = await authUserService.GetByIdAsync(idUser, cancellationToken).ConfigureAwait(false);
        if (authUser == null) return new List<UserAuthorizationResponse>();

        var flat = await authorizationManager.AllUserAuthorization(authUser, null).ConfigureAwait(false);
        return flat.Select(f => new UserAuthorizationResponse
        {
            AuthCode = f.AuthorizationCode,
            Can = new CPQ.Core.Services.CanAuthorization
            {
                CanView   = f.CanView,
                CanCreate = f.CanCreate,
                CanModify = f.CanModify,
                CanDelete = f.CanDelete,
                CanAction = f.CanAction,
            }
        }).ToList();
    }

    public async Task<CPQ.Core.DTO.UserDto> Login(
        string token, UserLogin logininfo, CancellationToken cancellationToken)
    {
        var user = await authUserService.GetQueryable()
            .Where(u => u.Name == logininfo.Email && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (user == null || user.Password != logininfo.Password.EncryptString())
            return null;

        return user.ToDto();
    }

    public Task RequestResetPassword(
        string token, RequestResetPasswordRequest request, CancellationToken cancellationToken)
        => authOrchestator.GeneratePasswordResetAsync(request.Email, cancellationToken);

    public async Task<IList<CPQ.Core.DTO.UserDto>> SearchUsersAsync(
        string token, string? search, bool? isActive, string? roles, CancellationToken cancellationToken)
    {
        var roleArray = roles?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var users = await authUserService.FindUser(search ?? string.Empty, roleArray, null, isActive, cancellationToken)
            .ConfigureAwait(false);
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<CPQ.Core.DTO.UserDto> GetUserByIdAsync(
        string token, int id, CancellationToken cancellationToken)
    {
        var user = await authUserService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return user?.ToDto();
    }

    public async Task<CPQ.Core.DTO.UserDto> CreateUserAsync(
        string token, CreateAuthUserRequest request, CancellationToken cancellationToken)
    {
        var created = await authOrchestator.CreateUser(new CreateUser
        {
            Email      = request.Email,
            Name       = request.Name,
            SurName    = request.SurName,
            Password   = request.Password,
            RoleCode   = request.RoleCode,
            ModuleCode = request.ModuleCode,
        }, cancellationToken).ConfigureAwait(false);

        return created?.ToDto();
    }

    public async Task UpdateUserAsync(
        string token, int id, UpdateAuthUserRequest request, CancellationToken cancellationToken)
    {
        await authUserService.UpdateUser(id, new Auth.Services.Command.UpdateUser
        {
            Email    = request.Email,
            Name     = request.Name,
            SurName  = request.SurName,
            IsActive = request.IsActive,
            RoleCode = request.RoleCode,
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteUserAsync(
        string token, int id, CancellationToken cancellationToken)
    {
        var user = await authUserService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (user == null) return;

        await authUserService.UpdateUser(id, new Auth.Services.Command.UpdateUser
        {
            Email    = user.Email,
            Name     = user.FirstName,
            SurName  = user.LastName,
            IsActive = false,
            RoleCode = user.Role?.Code,
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task ResetPasswordByIdAsync(
        string token, int id, CancellationToken cancellationToken)
    {
        var user = await authUserService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (user?.Email != null)
            await authOrchestator.GeneratePasswordResetAsync(user.Email, cancellationToken).ConfigureAwait(false);
    }

    public Task ResetPasswordByEmailAsync(
        string email, CancellationToken cancellationToken)
        => authOrchestator.GeneratePasswordResetAsync(email, cancellationToken);
}
