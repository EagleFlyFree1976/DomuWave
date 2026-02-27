using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class GetSupplierByIdCommand : BaseCommand, IQuery<Models.Supplier>
{
    public int SupplierId { get; set; }

    public GetSupplierByIdCommand() { }

    public GetSupplierByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetSupplierByIdCommand(int currentUserId, int supplierId) : base(currentUserId)
    {
        SupplierId = supplierId;
    }
}
