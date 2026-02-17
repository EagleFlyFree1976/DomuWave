using System.Globalization;
using System.Security.Cryptography;
using System.Transactions;
using CPQ.Core.Extensions;
using DomuWave.Services.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using NHibernate.Mapping.ByCode;
using MenuItem = Hangfire.Dashboard.MenuItem;


using Bogus.DataSets;
using CPQ.Core;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using DocumentFormat.OpenXml.Math;
using NHibernate.Engine.Query;
using NHibernate.Proxy;


namespace DomuWave.Services.Extensions;

public static class DtoExtensions
{
    public static BookEntityDto<T> fillBookEntityData<T>(this BookEntityDto<T> bookEntityDto, TenantEntity<T> tenantEntity)
    {
        bookEntityDto.Id = tenantEntity.Id;
        bookEntityDto.Name = tenantEntity.Name;
        bookEntityDto.Description= tenantEntity.Description;
        bookEntityDto.fillTraceData(tenantEntity);

        return bookEntityDto;
    }
    public static TraceEntityDTO<T> fillTraceData<T>(this TraceEntityDTO<T> traceEntityDto, TraceEntity<T> traceEntity)
    {
        

        traceEntityDto.LastUpdatedByFullName = traceEntity.LastUpdatedByFullName;
        traceEntityDto.CreatedByFullName = traceEntity.CreatedByFullName;
        traceEntityDto.CreatedById = traceEntity.CreatedBy;
        traceEntityDto.LastUpdatedById= traceEntity.LastUpdatedBy;
        traceEntityDto.CreationDate = traceEntity.CreationDate.ToUniversalTime();
        traceEntityDto.LastUpdateDate = traceEntity.LastUpdateDate.ToUniversalTime();

        return traceEntityDto;
    }

    

}