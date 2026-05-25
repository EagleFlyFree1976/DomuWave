using DomuWave.Services.Dto.Auth;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Auth;

public class SelfRegisterCommand : BaseCommand, IQuery<SelfRegisterResultDto>
{
    public string Email      { get; set; } = string.Empty;
    public string Password   { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    public SelfRegisterCommand() { }

    public SelfRegisterCommand(SelfRegisterDto dto) : base(0)
    {
        Email      = dto.Email;
        Password   = dto.Password;
        TenantName = dto.TenantName;
    }
}
