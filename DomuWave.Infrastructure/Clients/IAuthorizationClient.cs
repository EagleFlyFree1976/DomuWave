using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Clients.Response;
using CPQ.Core;
using CPQ.Core.DTO;
using CPQ.Core.Services;
using DomuWave.Services.Clients.Request;
using Refit;

namespace DomuWave.Services.Clients
{
    public interface IAuthorizationClient
    {
        [Get("/api/authorization/users/{idUser}/authorizations")]
        Task<IList<UserAuthorizationResponse>> GetAllUserAuthorizations([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, int idUser, CancellationToken cancellationToken);


        [Post("/api/PublicUser/login")]
        Task<CPQ.Core.DTO.UserDto> Login([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, UserLogin logininfo, CancellationToken cancellationToken);

        [Post("/api/PublicUser/request-reset-password")]
        Task RequestResetPassword([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, [Body] RequestResetPasswordRequest request, CancellationToken cancellationToken);

        [Put("/api/users/{id}")]
        Task UpdateUserAsync([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, int id, [Body] UpdateAuthUserRequest request, CancellationToken cancellationToken);

        [Get("/api/users/search")]
        Task<IList<CPQ.Core.DTO.UserDto>> SearchUsersAsync([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, [Query] string? search, [Query] bool? isActive, [Query] string? roles, CancellationToken cancellationToken);

        [Get("/api/users/{id}")]
        Task<CPQ.Core.DTO.UserDto> GetUserByIdAsync([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, int id, CancellationToken cancellationToken);

        [Post("/api/users/register")]
        Task<CPQ.Core.DTO.UserDto> CreateUserAsync([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, [Body] CreateAuthUserRequest request, CancellationToken cancellationToken);

        [Delete("/api/users/{id}")]
        Task DeleteUserAsync([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, int id, CancellationToken cancellationToken);

        [Post("/api/users/{id}/reset-password")]
        Task ResetPasswordByIdAsync([Header(SettingKey.HTTPHeaderAccessTokenKey)] string token, int id, CancellationToken cancellationToken);
        
        [Post("/api/PublicUser/reset-password")]
        Task ResetPasswordByEmailAsync(string email, CancellationToken cancellationToken);
    }
}
