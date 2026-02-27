using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetCondominiumByCodeCommand : BaseCommand, IQuery<Models.Condominium>
{
    public Guid TenantId { get; set; }
    public string Code { get; set; }

    public GetCondominiumByCodeCommand() { }

    public GetCondominiumByCodeCommand(int currentUserId) : base(currentUserId) { }
    public GetCondominiumByCodeCommand(int currentUserId, Guid tenantId, string code) : base(currentUserId)
    {
        TenantId = tenantId;
        Code = code;
    }
}
