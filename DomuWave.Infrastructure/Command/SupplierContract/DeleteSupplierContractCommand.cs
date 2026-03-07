using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class DeleteSupplierContractCommand : BaseCommand, IQuery<bool>
{
    public int ContractId { get; set; }

    public DeleteSupplierContractCommand() { }

    public DeleteSupplierContractCommand(int currentUserId, int contractId) : base(currentUserId)
    {
        ContractId = contractId;
    }
}
