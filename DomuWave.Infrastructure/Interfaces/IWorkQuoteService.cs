using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IWorkQuoteService : IBaseService<WorkQuote, int>
{
    Task<IList<WorkQuote>> GetByWorkIdAsync(int workId, IUser currentUser, CancellationToken cancellationToken);
    Task<WorkQuote?> ApproveAsync(int quoteId, IUser currentUser, CancellationToken cancellationToken);
    Task<WorkQuote?> RejectAsync(int quoteId, IUser currentUser, CancellationToken cancellationToken);
}
