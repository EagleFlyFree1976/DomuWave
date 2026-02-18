namespace DomuWave.Services.Command;

public abstract class BaseTenantRelatedCommand : BaseCommand
{
    
    

    public Guid TenantId { get; set; }

    protected BaseTenantRelatedCommand()
    {
    }


    protected BaseTenantRelatedCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}

