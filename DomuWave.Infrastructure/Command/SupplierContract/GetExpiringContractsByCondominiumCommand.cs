using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class GetExpiringContractsByCondominiumCommand : BaseCommand, IQuery<IList<SupplierContractReadDto>>
{
    public int CondominiumId { get; set; }
    public int DaysAhead     { get; set; }

    public GetExpiringContractsByCondominiumCommand() { }

    public GetExpiringContractsByCondominiumCommand(int currentUserId, int condominiumId, int daysAhead) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        DaysAhead     = daysAhead;
    }
}
