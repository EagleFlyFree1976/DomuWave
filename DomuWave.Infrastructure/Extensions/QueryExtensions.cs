using DomuWave.Services.Models;
using CPQ.Core;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Extensions;

public static class QueryExtensions
{
    /// <summary>
    /// ritorna i queryable filtratto tutti gli elementi non cancellati
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static IQueryable<T> GetQueryable<T>(this IQueryable<T> queryable) where T : TraceEntity<long>
    {
        return queryable.AsQueryable().Where(k=>!k.IsDeleted);
    }
    public static IQueryable<T> GetQueryable<T,T1>(this IQueryable<T> queryable) where T : TraceEntity<T1>
    {
        return queryable.AsQueryable().Where(k => !k.IsDeleted);
    }
    /// <summary>
    /// filtra il queryable limitando le entità appartenendi all'utente in input
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public static IQueryable<T> FilterByOwner<T>(this IQueryable<T> queryable, IUser currentUser) where T : IownerEntity
    {
        return queryable.AsQueryable().Where(k=>k.OwnerId == currentUser.Id);
    }

    /// <summary>
    /// Filtra gli elementi che sono legati ad uno specifico book
    /// se crossElement  = true, prendo solo gli elementi cross, se false prendo solo gli elementi del book in input, se null ritorna gli elementi del book specificato e
    /// anche gli elementi non assegnati a nessun book (elementi cross utilizzabili da tutti)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="book"></param>
    /// <param name="crossElement"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public static IQueryable<T> FilterByOwner<T,T1>(this IQueryable<T> queryable, Book book,  bool? crossElement, IUser currentUser) where T : BookEntity<T1>
    {

        if (!crossElement.HasValue)
        {
            
            return queryable.AsQueryable().Where(k => ((k.Book.IsSystem || (k.Book.Id == book.Id))));
            
        }

        if (!crossElement.Value)
        {
            return queryable.AsQueryable().Where(k => (!k.Book.IsSystem && k.Book.Id == book.Id));
        }
        else
        {
            return queryable.AsQueryable().Where(k => (k.Book.IsSystem));

        }
    }


    /// <summary>
    ///  Filtra tutti gli elementi in base al book specificato
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="book"></param>
    /// <returns></returns>
    public static IQueryable<T> FilterByBook<T>(this IQueryable<T> queryable, Book book) where T : BookEntity<long>
    {
        return queryable.AsQueryable().Where(k => k.Book.Id == book.Id);
    }

    public static IQueryable<T> FilterByBook<T>(this IQueryable<T> queryable, long bookId) where T : BookEntity<long>
    {
        return queryable.FilterByBook<T, long>(bookId);
    }

    public static IQueryable<T> FilterByBook<T,T1>(this IQueryable<T> queryable, long? bookId) where T : BookEntity<T1>
    {
        if (bookId.HasValue)
        {
            return queryable.AsQueryable().Where(k => k.Book != null && k.Book.Id == bookId.Value);
        }
        else
        {
            return queryable.AsQueryable().Where(k => k.Book == null );

        }
    }
}