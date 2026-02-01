using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Import;
using CPQ.Core.Exceptions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class ImportService : BaseService, IImportService
{
    public override string CacheRegion => "Import";


    public ImportService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
    {
    }
    public async Task<Import> GetByIdAsync(long importId, CancellationToken cancellationToken)
    {
        return await session.GetAsync<Import>(importId, cancellationToken).ConfigureAwait(false);
    }
    public async Task<Import> SaveAsync(Import import, CancellationToken cancellationToken)
    {
        if (import.Id == 0)
        {
            var alreadyExists = await session.Query<Import>().FilterByBook(import.Book.Id)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (alreadyExists != null)
            {
                throw new NotAllowedOperationException("Non è possibile creare un ulteriore processo di importazione");
            }

            await session.SaveAsync(import, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await session.UpdateAsync(import, cancellationToken).ConfigureAwait(false);
        }
        return import;
    }
    public async Task DeleteAsync(Import import, CancellationToken cancellationToken)
    {
        await session.DeleteAsync(import, cancellationToken).ConfigureAwait(false);
    }
    public async Task<IList<Import>> FindByBookIdAsync(int bookId, CancellationToken cancellationToken)
    {
        return await session.Query<Import>().FilterByBook(bookId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
 
}