using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class GetSupplierByVatCommand : BaseCommand, IQuery<Models.Supplier>
{
    public Guid TenantId { get; set; }
    public string VatNumber { get; set; }

    public GetSupplierByVatCommand() { }

    public GetSupplierByVatCommand(int currentUserId) : base(currentUserId) { }
    public GetSupplierByVatCommand(int currentUserId, Guid tenantId, string vatNumber) : base(currentUserId)
    {
        TenantId = tenantId;
        VatNumber = vatNumber;
    }
}
