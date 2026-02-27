using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitsCountCommand : BaseCommand, IQuery<int>
{
    public int CondominiumId { get; set; }

    public GetRealEstateUnitsCountCommand() { }

    public GetRealEstateUnitsCountCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitsCountCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
