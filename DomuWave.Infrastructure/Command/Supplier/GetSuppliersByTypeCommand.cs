using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class GetSuppliersByTypeCommand : BaseCommand, IQuery<IList<Models.Supplier>>
{
    public Guid TenantId { get; set; }
    public string SupplierType { get; set; }

    public GetSuppliersByTypeCommand() { }

    public GetSuppliersByTypeCommand(int currentUserId) : base(currentUserId) { }
    public GetSuppliersByTypeCommand(int currentUserId, Guid tenantId, string supplierType) : base(currentUserId)
    {
        TenantId = tenantId;
        SupplierType = supplierType;
    }
}
