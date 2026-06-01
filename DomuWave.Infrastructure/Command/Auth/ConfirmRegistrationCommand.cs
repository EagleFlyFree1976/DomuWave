using DomuWave.Services.Dto.Auth;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Auth;

public class ConfirmRegistrationCommand : BaseCommand, IQuery<ConfirmRegistrationResultDto>
{
    public string Token { get; set; } = string.Empty;

    public ConfirmRegistrationCommand() { }

    public ConfirmRegistrationCommand(ConfirmRegistrationDto dto) : base(0)
    {
        Token = dto.Token;
    }
}

public class RequestVerificationCommand : BaseCommand, IQuery<RequestVerificationResultDto>
{
    public Guid    RegistrationId  { get; set; }
    public string? CondominiumName { get; set; }
    public string? CondominiumCode { get; set; }
    public string? CondominiumCity { get; set; }
    public string? CondominiumZip  { get; set; }

    public RequestVerificationCommand() { }

    public RequestVerificationCommand(RequestVerificationDto dto) : base(0)
    {
        RegistrationId  = dto.RegistrationId;
        CondominiumName = dto.CondominiumName;
        CondominiumCode = dto.CondominiumCode;
        CondominiumCity = dto.CondominiumCity;
        CondominiumZip  = dto.CondominiumZip;
    }
}
