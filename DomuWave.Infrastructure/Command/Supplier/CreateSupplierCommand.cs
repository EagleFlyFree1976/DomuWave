using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class CreateSupplierCommand : BaseCommand, IQuery<Models.Supplier>
{
    public Models.Supplier Entity { get; set; }

    public CreateSupplierCommand() { }

    public CreateSupplierCommand(int currentUserId) : base(currentUserId) { }
    public CreateSupplierCommand(int currentUserId, Models.Supplier entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
