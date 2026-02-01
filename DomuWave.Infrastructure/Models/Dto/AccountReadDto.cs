using CPQ.Core;
using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto;

public class AccountReadDto : TraceEntityDTO<long>
{

    public virtual BookReadDto Book { get; set; }
    public virtual LookupEntityDto<int> AccountType { get; set; }

    public virtual LookupEntityDto<int> Currency { get; set; }


    public   bool IsActive { get; set; }
    public   decimal InitialBalance { get; set; }

    public   DateTime OpenDate { get; set; }

    public   DateTime? ClosedDate { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

 
    public long OwnerId { get; set; }


    
}