using DomuWave.Web.Clients.Response;
using Refit;

namespace DomuWave.Web.Clients
{
    public interface IAuthApi
    {
        [Post("/api/auth/login")]
        Task<TokenResponse> Login([Body] DomuWave.Web.Clients.Request.LoginRequest request, CancellationToken cancellationToken);
    }

}
