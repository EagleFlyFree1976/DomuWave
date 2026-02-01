using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IPaymentMethodService : IServiceBase<PaymentMethod, int, PaymentMethodCreateUpdateDto, PaymentMethodCreateUpdateDto, PaymentMethodReadDto>
{
    Task<PaymentMethod> GetById(int paymentMethodId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<PaymentMethod>> Find(long bookId, bool includeAll, IUser currentUser, CancellationToken cancellationToken);


    Task Disable(int paymentMethodId, IUser currentUser, CancellationToken cancellationToken);
    Task Enable(int paymentMethodId, IUser currentUser, CancellationToken cancellationToken);
}