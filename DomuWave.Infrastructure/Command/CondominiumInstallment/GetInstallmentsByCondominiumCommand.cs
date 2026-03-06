using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetInstallmentsByCondominiumCommand : BaseCommand, IQuery<IList<CondominiumInstallmentReadDto>>
{
    public int CondominiumId { get; set; }

    public GetInstallmentsByCondominiumCommand() { }

    public GetInstallmentsByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetInstallmentsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
