using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Beneficiary;

public class BeneficiaryReadDto : BookEntityDto<long>
{
    public bool Fake { get; set; } = false;
    public string Iban { get; set; }
    public string Notes { get; set; }

    public CategoryMinReadDto Category { get; set; }


 

}


public class BeneficiaryLookupReadDto : LookupEntityDto<long>
{
    public CategoryMinReadDto Category { get; set; }
}