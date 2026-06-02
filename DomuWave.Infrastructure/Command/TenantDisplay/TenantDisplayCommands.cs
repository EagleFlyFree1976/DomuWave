using DomuWave.Services.Dto.TenantDisplay;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.TenantDisplay;

public class GetTenantDisplaySettingsCommand : BaseCommand, IQuery<TenantDisplaySettingsReadDto?>
{
    public Guid TenantId { get; set; }
    public GetTenantDisplaySettingsCommand() { }
    public GetTenantDisplaySettingsCommand(int currentUserId, Guid tenantId) : base(currentUserId)
        => TenantId = tenantId;
}

public class UpsertTenantDisplaySettingsCommand : BaseCommand, IQuery<TenantDisplaySettingsReadDto>
{
    public Guid                          TenantId { get; set; }
    public UpsertTenantDisplaySettingsDto Dto     { get; set; } = null!;
    public UpsertTenantDisplaySettingsCommand() { }
    public UpsertTenantDisplaySettingsCommand(int currentUserId, Guid tenantId, UpsertTenantDisplaySettingsDto dto) : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}
