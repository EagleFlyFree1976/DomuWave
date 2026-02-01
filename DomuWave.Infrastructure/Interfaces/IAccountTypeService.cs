using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;

namespace DomuWave.Services.Interfaces;

public interface
    IAccountTypeService : IGenericService<AccountType, int, AccountTypeCreateUpdateDto, AccountTypeCreateUpdateDto>
{

}