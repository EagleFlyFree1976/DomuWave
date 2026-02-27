using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class UpdateSupplierCommand : BaseCommand, IQuery<Models.Supplier>
{
    public int SupplierId { get; set; }
    public Models.Supplier Entity { get; set; }

    public UpdateSupplierCommand() { }

    public UpdateSupplierCommand(int currentUserId) : base(currentUserId) { }
    public UpdateSupplierCommand(int currentUserId, int supplierId, Models.Supplier entity) : base(currentUserId)
    {
        SupplierId = supplierId;
        Entity = entity;
    }
}
