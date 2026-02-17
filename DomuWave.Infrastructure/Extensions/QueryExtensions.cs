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
    ///  Filtra tutti gli elementi in base al tenant specificato
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public static IQueryable<T> FilterByTenant<T>(this IQueryable<T> queryable, Tenant tenant) where T : TenantEntity<long>
    {
        return queryable.AsQueryable().Where(k => k.Tenant.Id == tenant.Id);
    }

    public static IQueryable<T> FilterByTenant<T>(this IQueryable<T> queryable, Guid tenantId) where T : TenantEntity<long>
    {
        return queryable.FilterByTenant<T, long>(tenantId);
    }

    public static IQueryable<T> FilterByTenant<T,T1>(this IQueryable<T> queryable, Guid? tenantId) where T : TenantEntity<T1>
    {
        if (tenantId.HasValue)
        {
            return queryable.AsQueryable().Where(k => k.Tenant != null && k.Tenant.Id == tenantId.Value);
        }
        else
        {
            return queryable.AsQueryable().Where(k => k.Tenant == null );

        }
    }
}