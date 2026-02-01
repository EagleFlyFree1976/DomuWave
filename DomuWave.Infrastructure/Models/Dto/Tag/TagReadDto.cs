using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Tag;

public class TagReadDto : BookEntityDto<int>
{
   public bool Enabled { get; set; }

}