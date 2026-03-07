using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IWorkQuoteDocumentService : IBaseService<WorkQuoteDocument, int>
{
    Task<IList<WorkQuoteDocument>> GetByQuoteIdAsync(int quoteId, IUser currentUser, CancellationToken cancellationToken);
}
