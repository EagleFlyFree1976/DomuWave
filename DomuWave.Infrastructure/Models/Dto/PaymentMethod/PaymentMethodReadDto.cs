using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.PaymentMethod;

public class PaymentMethodReadDto : BookEntityDto<int>
{
   public bool Enabled { get; set; }

}