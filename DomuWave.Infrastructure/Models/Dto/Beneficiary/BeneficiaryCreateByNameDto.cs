namespace DomuWave.Services.Models.Dto.Beneficiary;

public class BeneficiaryCreateByNameDto
{
    public long BookId { get; set; }
    public string Name { get; set; }

    public int? CategoryId { get; set; }
}