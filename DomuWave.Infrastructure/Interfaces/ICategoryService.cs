using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface ICategoryService : IServiceBase<Category, long, CategoryCreateUpdateDto, CategoryCreateUpdateDto, CategoryReadDto>
{
    Task<Category> GetById(long categoryId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Category>> Find(long bookId, string q, bool includeAll, IUser currentUser,
        CancellationToken cancellationToken);

 
}