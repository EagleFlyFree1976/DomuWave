using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto;

public class AccountTypeReadDto : TraceEntityDTO<int>
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class AccountTypeCreateUpdateDto
{
    public string Code { get; set; }
    public string Description { get; set; }
    
}