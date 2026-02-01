using MassTransit.Futures.Contracts;

namespace DomuWave.Services.Models.Dto.Beneficiary;

public class BeneficiaryCreateUpdateDto
{
    public long BookId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Iban { get; set; }
    public string Notes { get; set; }
    public long? CategoryId { get; set; }
}