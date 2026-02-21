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



    }
}
