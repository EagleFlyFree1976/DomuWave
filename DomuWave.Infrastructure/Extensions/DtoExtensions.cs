using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using DomuWave.Services.Models;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Models;

namespace DomuWave.Services.Extensions
{
    public static class DtoExtensions
    {

        public static UserTenantReadDto ToDto(this UserTenant souce)
        {

            UserTenantReadDto dto = new UserTenantReadDto();
            dto.UserTenantId = souce.Id;
            dto.UserId = souce.UserId;
            dto.TenantId = souce.Tenant.Id;
            dto.TenantName = souce.Tenant.Name;
            dto.TenantCode = souce.Tenant.Code;
            dto.IsActive = souce.IsActive;
            dto.IsDefault = souce.IsDefault;
            dto.SetTraceInfo(souce);
            return dto;
        }

        public static UserTenant FillEntity(this UserTenantUpdateDto source, UserTenant entity)
        {
            
            
            entity.IsDefault = source.IsDefault;
            entity.IsActive = source.IsActive;

            return entity;
        }
        public static UserTenant ToEntity(this UserTenantCreateDto source)
        {
            UserTenant entity = new UserTenant();
            entity.UserId = source.UserId;
            entity.Tenant = new Tenant
            {
                Id = source.TenantId,

            };
            entity.IsDefault = source.IsDefault;
            entity.IsActive = source.IsActive;

            return entity;
        }
    }
}
