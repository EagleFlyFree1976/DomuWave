using CPQ.Core;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.Auth;
using DomuWave.Services.Dto.Auth;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Auth;

public class CheckEmailCommandConsumer
    : InMemoryConsumerBase<CheckEmailCommand, CheckEmailResultDto>
{
    private readonly IAuthorizationClient _authClient;

    public CheckEmailCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAuthorizationClient    authClient) : base(sessionFactoryProvider)
    {
        _authClient = authClient;
    }

    protected override async Task<CheckEmailResultDto> Consume(
        CheckEmailCommand command,
        IMediationContext  mediationContext,
        CancellationToken  cancellationToken)
    {
        var existing = await _authClient.SearchUsersAsync(
            CommonKeys.SystemUserToken,
            search:   command.Email,
            isActive: null,
            roles:    null,
            cancellationToken).ConfigureAwait(false);

        var match = existing?.FirstOrDefault(u =>
            string.Equals(u.Email, command.Email, StringComparison.OrdinalIgnoreCase));

        var isCondomino = match != null &&
            string.Equals(match.RoleCode, "Condomino", StringComparison.OrdinalIgnoreCase);

        return new CheckEmailResultDto
        {
            Email               = command.Email,
            IsExistingCondomino = isCondomino,
        };
    }
}
