using System.Globalization;
using System.Security.Cryptography;
using System.Transactions;
using CPQ.Core.Extensions;
using DomuWave.Services.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using NHibernate.Mapping.ByCode;
 


using Bogus.DataSets;
using CPQ.Core;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using DocumentFormat.OpenXml.Math;
using NHibernate.Engine.Query;
using NHibernate.Proxy;


namespace DomuWave.Services.Interfaces.Extensions;

public static class DtoExtensions
{
    public static BookEntityDto<T> fillBookEntityData<T>(this BookEntityDto<T> bookEntityDto,
        TenantEntity<T> tenantEntity)
    {
        bookEntityDto.Id = tenantEntity.Id;
        bookEntityDto.Name = tenantEntity.Name;
        bookEntityDto.Description = tenantEntity.Description;
        bookEntityDto.SetTraceInfo(tenantEntity);

        return bookEntityDto;
    }

    public static MenuItemDto ToDto(this MenuItem item)
    {
        if (item == null) return null;

        MenuItemDto dto = new MenuItemDto
        {
            Id = item.Id,
            Icon = item.Icon,
            ParentMenuId = item.ParentMenuId,
            Action = item.Action,
            AuthorizationCode = item.AuthorizationCode,
            Description = item.Description
        };
        return dto;
    }



}