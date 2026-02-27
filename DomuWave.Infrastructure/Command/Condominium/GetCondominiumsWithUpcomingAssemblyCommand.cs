using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetCondominiumsWithUpcomingAssemblyCommand : BaseCommand, IQuery<IList<Models.Condominium>>
{
    public Guid TenantId { get; set; }
    public int DaysAhead { get; set; }

    public GetCondominiumsWithUpcomingAssemblyCommand() { }

    public GetCondominiumsWithUpcomingAssemblyCommand(int currentUserId) : base(currentUserId) { }
    public GetCondominiumsWithUpcomingAssemblyCommand(int currentUserId, Guid tenantId, int daysAhead) : base(currentUserId)
    {
        TenantId = tenantId;
        DaysAhead = daysAhead;
    }
}
