using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Beneficiary;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IBeneficiaryService : IServiceBase<Beneficiary, long, BeneficiaryCreateUpdateDto,
    BeneficiaryCreateUpdateDto, BeneficiaryReadDto>
{

    Task<Beneficiary> GetByName(string name, long bookId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Beneficiary>> Find(string q, long bookId, IUser currentUser, CancellationToken cancellationToken);

}