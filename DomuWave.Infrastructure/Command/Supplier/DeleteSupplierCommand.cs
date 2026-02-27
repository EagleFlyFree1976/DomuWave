using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class DeleteSupplierCommand : BaseCommand, IQuery<bool>
{
    public int SupplierId { get; set; }

    public DeleteSupplierCommand() { }

    public DeleteSupplierCommand(int currentUserId) : base(currentUserId) { }
    public DeleteSupplierCommand(int currentUserId, int supplierId) : base(currentUserId)
    {
        SupplierId = supplierId;
    }
}
