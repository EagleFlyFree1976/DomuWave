using DomuWave.Services.Dto.Auth;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Auth;

public class ConfirmRegistrationCommand : BaseCommand, IQuery<ConfirmRegistrationResultDto>
{
    public Guid RegistrationId { get; set; }

    public ConfirmRegistrationCommand() { }

    public ConfirmRegistrationCommand(ConfirmRegistrationDto dto) : base(0)
    {
        RegistrationId = dto.RegistrationId;
    }
}
