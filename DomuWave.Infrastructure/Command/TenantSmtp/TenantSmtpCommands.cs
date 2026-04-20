using DomuWave.Services.Dto.TenantSmtp;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.TenantSmtp;

public class GetTenantSmtpSettingsCommand : BaseCommand, IQuery<TenantSmtpSettingsReadDto?>
{
    public Guid TenantId { get; set; }
    public GetTenantSmtpSettingsCommand() { }
    public GetTenantSmtpSettingsCommand(int currentUserId, Guid tenantId) : base(currentUserId)
        => TenantId = tenantId;
}

public class UpsertTenantSmtpSettingsCommand : BaseCommand, IQuery<TenantSmtpSettingsReadDto>
{
    public Guid                      TenantId { get; set; }
    public UpsertTenantSmtpSettingsDto Dto    { get; set; } = null!;
    public UpsertTenantSmtpSettingsCommand() { }
    public UpsertTenantSmtpSettingsCommand(int currentUserId, Guid tenantId, UpsertTenantSmtpSettingsDto dto) : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}

public class TestTenantSmtpCommand : BaseCommand, IQuery<bool>
{
    public Guid   TenantId    { get; set; }
    public string TestEmailTo { get; set; } = string.Empty;
    public TestTenantSmtpCommand() { }
    public TestTenantSmtpCommand(int currentUserId, Guid tenantId, string testEmailTo) : base(currentUserId)
    {
        TenantId    = tenantId;
        TestEmailTo = testEmailTo;
    }
}
