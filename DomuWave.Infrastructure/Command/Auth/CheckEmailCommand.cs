using DomuWave.Services.Dto.Auth;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Auth;

public class CheckEmailCommand : BaseCommand, IQuery<CheckEmailResultDto>
{
    public string Email { get; set; } = string.Empty;

    public CheckEmailCommand() { }

    public CheckEmailCommand(CheckEmailDto dto) : base(0)
    {
        Email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
